using DV.CabControls;
using DV.HUD;
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
		private ControlImplBase? _bellControl;
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
			ResetAudioControlStates();
			_unsubscribeFromTrainCarEvents?.Invoke();
		}

		private void OnCarChanged(TrainCar? newCar)
		{
			ResetAudioControlStates();
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
			var simFlow = trainCar.SimController.simFlow;
			var unsubscribeActions = new List<Action>();
			Action<GameObject> onExternalInteractableLoaded = OnExternalInteractableLoaded;
			Action<GameObject> onExternalInteractableAboutToBeUnloaded = _ => UnsubscribeBellControl();

			if (simFlow.TryGetPort("horn.HORN", out var simPortHornHorn))
			{
				simPortHornHorn.ValueUpdatedInternally += CheckHornHorn;
				unsubscribeActions.Add(() => simPortHornHorn.ValueUpdatedInternally -= CheckHornHorn);
			}
			else
			{
				Main.Warning("Could not get simPort horn.Horn");
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

			SubscribeToBellControl(trainCar.GetComponent<LocoControlsReader>());
			SubscribeToBellControl(trainCar.loadedExternalInteractables?.GetComponent<LocoControlsReader>());

			trainCar.ExternalInteractableLoaded += onExternalInteractableLoaded;
			trainCar.ExternalInteractableAboutToBeUnloaded += onExternalInteractableAboutToBeUnloaded;
			unsubscribeActions.Add(() =>
			{
				trainCar.ExternalInteractableLoaded -= onExternalInteractableLoaded;
				trainCar.ExternalInteractableAboutToBeUnloaded -= onExternalInteractableAboutToBeUnloaded;
				UnsubscribeBellControl();
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
			if (value <= 0.1f && _previousHorn > 0.1f)
			{
				Main.Log("Honk end");
				HonkEnded?.Invoke();
			}
			if (value > 0.1f && _previousHorn <= 0.1f)
			{
				Main.Log("Honk start");
				HonkStarted?.Invoke();
			}
			_previousHorn = value;
		}

		private void OnExternalInteractableLoaded(GameObject loadedExternalInteractables)
		{
			SubscribeToBellControl(loadedExternalInteractables?.GetComponent<LocoControlsReader>());
		}

		private void SubscribeToBellControl(LocoControlsReader? locoControlsReader)
		{
			if (locoControlsReader == null || locoControlsReader.bell == null)
			{
				return;
			}

			ControlImplBase? bellControl = locoControlsReader.bell.GetComponent<ControlImplBase>();
			if (bellControl == null || bellControl == _bellControl)
			{
				return;
			}

			UnsubscribeBellControl();
			_bellControl = bellControl;
			_bellControl.ValueChanged += CheckBellValueChanged;
			CheckBellValue(_bellControl.Value);
		}

		private void UnsubscribeBellControl()
		{
			if (_bellControl == null)
			{
				return;
			}

			_bellControl.ValueChanged -= CheckBellValueChanged;
			_bellControl = null;
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
				Main.Log("Bell end");
				BellEnded?.Invoke();
			}
			if (value > 0.1f && _previousBell <= 0.1f)
			{
				Main.Log("Bell start");
				BellStarted?.Invoke();
			}
			_previousBell = value;
		}

		private void ResetAudioControlStates()
		{
			CheckHornHorn(0f);
			CheckBellValue(0f);
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

			Main.Debug($"Unlocked garages ({unlockedGaragesCount}): {string.Join(", ", unlockablesManager.UnlockedGarages.Select(garage => garage.ToString()))}");
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
