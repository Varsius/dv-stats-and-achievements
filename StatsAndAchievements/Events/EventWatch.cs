using DV.CabControls;
using DV.HUD;
using DV.Simulation.Cars;
using DV.Simulation.Controllers;
using DV.Simulation.Ports;
using DV.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StatsAndAchievements.Events
{
	internal class EventWatch : MonoBehaviour
	{
		// TODO: this class needs to be refactored,
		// ideally I want all state variables to be defined in
		// their respective checks.
		private float _elapsed;
		private float _previousSpeed;
		private float _previousHorn;
		private float _previousBell;
		private int _previousUnlockedGarages = -1;
		private static TrainCar? _trainCar;
		private bool _hornNeutralAt0 = true;
		private string? _subscribedBellPortId;
		private Action? _unsubscribeBellPort;
		private Action? _unsubscribeFromTrainCarEvents;

		public static event Action<float>? SpeedIncreased;
		public static event Action? HonkStarted;
		public static event Action? HonkEnded;
		public static event Action? BellStarted;
		public static event Action? BellEnded;
		public static event Action<int>? UnlockedGaragesChanged;

		void OnEnable()
		{
			if (PlayerManager.Car != null)
			{
				OnCarChanged(PlayerManager.Car);
			}
			PlayerManager.CarChanged += OnCarChanged;
		}

		void OnDisable()
		{
			Main.Log("watch disabled");
			PlayerManager.CarChanged -= OnCarChanged;
			EndActiveAudioControls("watch disabled");
			ResetAudioControlStates();
			UnsubscribeBellPort();
			_unsubscribeFromTrainCarEvents?.Invoke();
		}

		private void OnCarChanged(TrainCar? newCar)
		{
			EndActiveAudioControls("car changed");
			ResetAudioControlStates();
			UnsubscribeBellPort();
			_unsubscribeFromTrainCarEvents?.Invoke();
			_unsubscribeFromTrainCarEvents = null;

			_trainCar = null;

			if (newCar == null)
			{
				return;
			}

			_trainCar = newCar;
			_unsubscribeFromTrainCarEvents = SubscribeToTrainCarEvents(newCar);
		}

		private Action SubscribeToTrainCarEvents(TrainCar trainCar)
		{
			var unsubscribeActions = new List<Action>();
			Action<GameObject> onInteriorLoaded = OnInteriorLoaded;
			Action<GameObject> onInteriorAboutToBeUnloaded = _ => UnsubscribeBellPort();
			Action<GameObject> onExternalInteractableLoaded = OnExternalInteractableLoaded;
			Action<GameObject> onExternalInteractableAboutToBeUnloaded = _ => UnsubscribeBellPort();

			var simController = trainCar.SimController;
			if (simController == null)
			{
				Main.Warning("Train car has no SimController");
				return () => { };
			}

			var simFlow = simController.simFlow;
			HornControl? hornControl = simController.controlsOverrider?.Horn;
			_hornNeutralAt0 = hornControl?.neutralAt0 ?? true;
			string hornPortId = hornControl?.portId ?? "horn.HORN";
			if (simFlow.TryGetPort(hornPortId, out var simPortHorn))
			{
				simPortHorn.ValueUpdatedInternally += CheckHornHorn;
				unsubscribeActions.Add(() => simPortHorn.ValueUpdatedInternally -= CheckHornHorn);
				_previousHorn = simPortHorn.Value;
			}
			else
			{
				Main.Warning($"Could not get simPort {hornPortId}");
			}

			if (simFlow.TryGetPort("sand.AMOUNT", out var simPortSandAmount))
			{
				simPortSandAmount.ValueUpdatedInternally += CheckSandAmount;
				unsubscribeActions.Add(() => simPortSandAmount.ValueUpdatedInternally -= CheckSandAmount);
			}
			else
			{
				Main.Warning("Could not get simPort sand.AMOUNT");
			}

			SubscribeToBellPort(simFlow, trainCar.GetComponent<LocoControlsReader>());
			SubscribeToBellPort(simFlow, trainCar.loadedExternalInteractables?.GetComponent<LocoControlsReader>());
			if (_unsubscribeBellPort == null)
			{
				SubscribeToBellControl(trainCar);
			}

			trainCar.InteriorLoaded += onInteriorLoaded;
			trainCar.InteriorAboutToBeUnloaded += onInteriorAboutToBeUnloaded;
			trainCar.ExternalInteractableLoaded += onExternalInteractableLoaded;
			trainCar.ExternalInteractableAboutToBeUnloaded += onExternalInteractableAboutToBeUnloaded;
			unsubscribeActions.Add(() =>
			{
				trainCar.InteriorLoaded -= onInteriorLoaded;
				trainCar.InteriorAboutToBeUnloaded -= onInteriorAboutToBeUnloaded;
				trainCar.ExternalInteractableLoaded -= onExternalInteractableLoaded;
				trainCar.ExternalInteractableAboutToBeUnloaded -= onExternalInteractableAboutToBeUnloaded;
				UnsubscribeBellPort();
			});

			return () =>
			{
				foreach (var unsubscribeAction in unsubscribeActions)
					unsubscribeAction();
			};
		}

		void Update()
		{
			_elapsed += Time.deltaTime;
			if (_elapsed < 1.0f)
				return;

			_elapsed = 0f;

			if (_trainCar != null)
			{
				CheckSpeedIncreased(_trainCar);
			}

			CheckUnlockedGarages();
			CheckPlayerPosition();
		}

		private void CheckSpeedIncreased(TrainCar trainCar)
		{
			float currentSpeed = trainCar.GetVelocity().magnitude * 3.6f;
			if (currentSpeed > _previousSpeed)
			{
				SpeedIncreased?.Invoke(currentSpeed);
			}
			_previousSpeed = currentSpeed;
		}

		private void CheckHornHorn(float value)
		{
			bool wasActive = IsHornActive(_previousHorn);
			bool isActive = IsHornActive(value);
			if (!isActive && wasActive)
			{
				HonkEnded?.Invoke();
			}
			if (isActive && !wasActive)
			{
				HonkStarted?.Invoke();
			}
			_previousHorn = value;
		}

		private void OnExternalInteractableLoaded(GameObject loadedExternalInteractables)
		{
			if (_trainCar?.SimController == null)
			{
				Main.Warning("External interactable loaded but train car SimController is missing");
				return;
			}

			SubscribeToBellPort(_trainCar.SimController.simFlow, loadedExternalInteractables?.GetComponent<LocoControlsReader>());
			if (_unsubscribeBellPort == null && _trainCar != null)
			{
				SubscribeToBellControl(_trainCar);
			}
		}

		private void OnInteriorLoaded(GameObject loadedInterior)
		{
			if (_trainCar?.SimController == null)
			{
				Main.Warning("Interior loaded but train car SimController is missing");
				return;
			}

			SubscribeToBellPort(_trainCar.SimController.simFlow, loadedInterior?.GetComponent<LocoControlsReader>());
			if (_unsubscribeBellPort == null && _trainCar != null)
			{
				SubscribeToBellControl(_trainCar);
			}
		}

		private void SubscribeToBellPort(LocoSim.Implementations.SimulationFlow simFlow, LocoControlsReader? locoControlsReader)
		{
			if (locoControlsReader == null || locoControlsReader.bell == null)
			{
				return;
			}

			InteractablePortFeeder? bellPortFeeder = locoControlsReader.bell.GetComponent<InteractablePortFeeder>();
			if (bellPortFeeder == null || string.IsNullOrEmpty(bellPortFeeder.portId) || bellPortFeeder.portId == _subscribedBellPortId)
			{
				return;
			}

			if (!simFlow.TryGetPort(bellPortFeeder.portId, out var simPortBell))
			{
				Main.Warning($"Could not get simPort {bellPortFeeder.portId}");
				return;
			}

			UnsubscribeBellPort();
			_subscribedBellPortId = bellPortFeeder.portId;
			_unsubscribeBellPort = () => simPortBell.ValueUpdatedInternally -= CheckBellValue;
			simPortBell.ValueUpdatedInternally += CheckBellValue;
			_previousBell = simPortBell.Value;
		}

		private void SubscribeToBellControl(TrainCar trainCar)
		{
			InteriorControlsManager? controlsManager =
				trainCar.loadedInterior?.GetComponent<InteriorControlsManager>()
				?? trainCar.loadedInterior?.GetComponentInChildren<InteriorControlsManager>()
				?? trainCar.interior?.GetComponentInChildren<InteriorControlsManager>();
			if (controlsManager == null)
			{
				return;
			}

			if (!controlsManager.TryGetControl(InteriorControlsManager.ControlType.Bell, out var bellReference) || bellReference.controlImplBase == null)
			{
				return;
			}

			UnsubscribeBellPort();
			_subscribedBellPortId = "control_fallback";
			_unsubscribeBellPort = () => bellReference.controlImplBase.ValueChanged -= CheckBellValueChanged;
			bellReference.controlImplBase.ValueChanged += CheckBellValueChanged;
			_previousBell = bellReference.controlImplBase.Value;
		}

		private void UnsubscribeBellPort()
		{
			if (_unsubscribeBellPort == null)
			{
				return;
			}

			_unsubscribeBellPort.Invoke();
			_unsubscribeBellPort = null;
			_subscribedBellPortId = null;
			CheckBellValue(0f);
		}

		private void CheckBellValueChanged(ValueChangedEventArgs value)
		{
			CheckBellValue(value.newValue);
		}

		private void CheckBellValue(float value)
		{
			if (value <= 0.1f && _previousBell > 0.1f)
			{
				BellEnded?.Invoke();
			}
			if (value > 0.1f && _previousBell <= 0.1f)
			{
				BellStarted?.Invoke();
			}
			_previousBell = value;
		}

		private void ResetAudioControlStates()
		{
			_previousHorn = _hornNeutralAt0 ? 0f : 0.5f;
			_previousBell = 0f;
		}

		private bool IsHornActive(float value)
		{
			if (_hornNeutralAt0)
			{
				return value > 0.1f;
			}

			return Mathf.Abs(value - 0.5f) > 0.1f;
		}

		private void EndActiveAudioControls(string reason)
		{
			if (IsHornActive(_previousHorn))
			{
				HonkEnded?.Invoke();
			}

			if (_previousBell > 0.1f)
			{
				BellEnded?.Invoke();
			}
		}

		private void CheckSandAmount(float value)
		{
			;
		}

		private void CheckUnlockedGarages()
		{
			var unlockablesManager = SingletonBehaviour<UnlockablesManager>.Instance;
			if (unlockablesManager == null)
			{
				return;
			}

			int unlockedGaragesCount = unlockablesManager.UnlockedGarages.Count;
			if (unlockedGaragesCount == _previousUnlockedGarages)
			{
				return;
			}

			_previousUnlockedGarages = unlockedGaragesCount;
			UnlockedGaragesChanged?.Invoke(unlockedGaragesCount);
		}

		private void CheckPlayerPosition()
		{
			if (PlayerManager.PlayerTransform == null)
			{
				return;
			}

			Vector3 playerPosition = PlayerManager.PlayerTransform.position;
			//Main.Debug($"Player position: {playerPosition.x}, {playerPosition.y}, {playerPosition.z}");
			Actions.InvokePlayerPositionChanged(playerPosition);
		}
	}
}
