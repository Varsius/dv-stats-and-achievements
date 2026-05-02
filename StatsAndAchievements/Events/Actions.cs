using System;
using DV.Logic.Job;
using UnityEngine;

namespace StatsAndAchievements.Events
{
	public static class Actions
	{
		public static event Action<string>? LicenseAcquired;
		public static event Action<Job>? JobCompletion;
		public static event Action? PaintJobApplied;
		public static event Action<Vector3>? PlayerPositionChanged;

		public static void InvokeLicenseAcquired(string licenseName)
		{
			LicenseAcquired?.Invoke(licenseName);
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
	}
}
