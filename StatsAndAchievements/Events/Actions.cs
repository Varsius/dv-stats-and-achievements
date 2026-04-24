using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DV.Logic.Job;

namespace StatsAndAchievements.Events
{
	public static class Actions
	{
		public static event Action<string> LicenseAcquired;
		public static event Action<Job> JobCompletion;
		public static event Action PaintJobApplied;

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
	}
}
