using DV.Customization;
using System;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace StatsAndAchievements
{
	internal class Watch : MonoBehaviour
	{
		private float _elapsed;
		private float _previousSpeed;
		private static TrainCar _trainCar;
		private Action? _unsubscribeFromAllTrainCarEvents;

		public static event Action<float> SpeedIncreased;
		public static event Action Honked;

		void OnEnable()
		{
			if (PlayerManager.Car != null)
			{
				OnCarChanged(PlayerManager.Car);
			}
			PlayerManager.CarChanged += OnCarChanged;
		}

		private void OnCarChanged(TrainCar newCar)
		{
			_unsubscribeFromAllTrainCarEvents?.Invoke();
			_unsubscribeFromAllTrainCarEvents = null;

			if (newCar == null)
			{
				return;
			}
			_trainCar = newCar;
			_unsubscribeFromAllTrainCarEvents = SubscribeToTrainCarEvents(newCar);
		}

		private Action SubscribeToTrainCarEvents(TrainCar trainCar)
		{
			var simFlow = trainCar.SimController.simFlow;
			Action? unsubscribeHornHorn = null;
			Action? unsubscribeSandAmount = null;

			if (simFlow.TryGetPort("horn.HORN", out var simPortHornHorn))
			{
				simPortHornHorn.ValueUpdatedInternally += CheckHornHorn;
				unsubscribeHornHorn = () => simPortHornHorn.ValueUpdatedInternally -= CheckHornHorn;
			}

			if (simFlow.TryGetPort("sand.AMOUNT", out var simPortSandAmount))
			{
				simPortSandAmount.ValueUpdatedInternally += CheckSandAmount;
				unsubscribeHornHorn = () => simPortSandAmount.ValueUpdatedInternally -= CheckSandAmount;
			}

			return () =>
			{
				unsubscribeHornHorn?.Invoke();
				unsubscribeSandAmount?.Invoke();
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
			float currentSpeed = _trainCar.GetVelocity().magnitude;
			if (currentSpeed > _previousSpeed)
			{
				Main.Log($"Speed increased: {currentSpeed}");
				SpeedIncreased?.Invoke(currentSpeed);
			}
			_previousSpeed = currentSpeed;
		}

		private void CheckHornHorn(float value)
		{
			;
		}

		private void CheckSandAmount(float value)
		{
			;
		}
	}
}
