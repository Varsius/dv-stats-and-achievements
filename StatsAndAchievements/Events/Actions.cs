using System;
using DV.Logic.Job;
using DV.ThingTypes;
using UnityEngine;

namespace StatsAndAchievements.Events
{
	public enum MoneySpentSource
	{
		ItemShop,
		ManualService,
		CareerManagerFees,
		CareerManagerLicense,
		FastTravel,
		CommsRadioRerail,
		CommsRadioCarRemoval,
		CommsRadioUtilityVehicleSummon
	}

	public enum CommsRadioActionType
	{
		Rerail,
		CarRemoval,
		UtilityVehicleSummon
	}

	public static class Actions
	{
		public static event Action<JobLicenseType_v2>? JobLicenseAcquired;
		public static event Action<GeneralLicenseType_v2>? GeneralLicenseAcquired;
		public static event Action<Job>? JobCompletion;
		public static event Action? PaintJobApplied;
		public static event Action<Vector3>? PlayerPositionChanged;
		public static event Action<string>? ItemAcquired;
		public static event Action<float, MoneySpentSource>? MoneySpent;
		public static event Action<float>? CargoDamageIncurred;
		public static event Action<float>? VehicleDamageIncurred;
		public static event Action<ResourceType, float>? ServiceResourceBought;
		public static event Action? PlayerDerailed;
		public static event Action<float>? TurntableRotated;
		public static event Action<bool>? FastTravelCompleted;
		public static event Action<CommsRadioActionType>? CommsRadioActionPerformed;

		public static void InvokeJobLicenseAcquired(JobLicenseType_v2 license)
		{
			JobLicenseAcquired?.Invoke(license);
		}

		public static void InvokeGeneralLicenseAcquired(GeneralLicenseType_v2 license)
		{
			GeneralLicenseAcquired?.Invoke(license);
		}

		public static void InvokeJobCompletion(Job job)
		{
			JobCompletion?.Invoke(job);
		}

		public static void InvokePaintJobApplied()
		{
			PaintJobApplied?.Invoke();
		}

		public static void InvokePlayerPositionChanged(Vector3 playerPosition)
		{
			PlayerPositionChanged?.Invoke(playerPosition);
		}

		public static void InvokeItemAcquired(string itemName)
		{
			ItemAcquired?.Invoke(itemName);
		}

		public static void InvokeMoneySpent(float amount, MoneySpentSource source)
		{
			MoneySpent?.Invoke(amount, source);
		}

		public static void InvokeCargoDamageIncurred(float amount)
		{
			CargoDamageIncurred?.Invoke(amount);
		}

		public static void InvokeVehicleDamageIncurred(float amount)
		{
			VehicleDamageIncurred?.Invoke(amount);
		}

		public static void InvokeServiceResourceBought(ResourceType resourceType, float amount)
		{
			ServiceResourceBought?.Invoke(resourceType, amount);
		}

		public static void InvokePlayerDerailed()
		{
			PlayerDerailed?.Invoke();
		}

		public static void InvokeTurntableRotated(float degrees)
		{
			TurntableRotated?.Invoke(degrees);
		}

		public static void InvokeFastTravelCompleted(bool withLoco)
		{
			FastTravelCompleted?.Invoke(withLoco);
		}

		public static void InvokeCommsRadioActionPerformed(CommsRadioActionType actionType)
		{
			CommsRadioActionPerformed?.Invoke(actionType);
		}
	}
}
