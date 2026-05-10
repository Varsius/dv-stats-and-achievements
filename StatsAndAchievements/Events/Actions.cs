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
		FastTravel
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
		public static event Action<bool>? FastTravelCompleted;

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

		public static void InvokeFastTravelCompleted(bool withLoco)
		{
			FastTravelCompleted?.Invoke(withLoco);
		}
	}
}
