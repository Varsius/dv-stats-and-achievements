using DV.Customization;
using System;
using System.Collections.Generic;
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
		private static TrainCar _trainCar;
		private Action? _unsubscribeFromTrainCarEvents;

		public static event Action<float> SpeedIncreased;
		public static event Action Honked;
		public static event Action HonkStarted;
		public static event Action HonkEnded;

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
			_unsubscribeFromTrainCarEvents?.Invoke();
		}

		private void OnCarChanged(TrainCar newCar)
		{
			_unsubscribeFromTrainCarEvents?.Invoke();
			_unsubscribeFromTrainCarEvents = null;

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

			if (simFlow.TryGetPort("horn.HORN", out var simPortHornHorn))
			{
				simPortHornHorn.ValueUpdatedInternally += CheckHornHorn;
				unsubscribeActions.Add(() => simPortHornHorn.ValueUpdatedInternally -= CheckHornHorn);
			}

			if (simFlow.TryGetPort("sand.AMOUNT", out var simPortSandAmount))
			{
				simPortSandAmount.ValueUpdatedInternally += CheckSandAmount;
				unsubscribeActions.Add(() => simPortSandAmount.ValueUpdatedInternally -= CheckSandAmount);
			}

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
		}

		private void CheckSpeedIncreased(TrainCar trainCar)
		{
			float currentSpeed = _trainCar.GetVelocity().magnitude * 3.6f;
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

		private void CheckSandAmount(float value)
		{
			;
		}
	}
}
