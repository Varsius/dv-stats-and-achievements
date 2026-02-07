using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsAndAchievements.Events
{
	public static class Actions
	{
		public static event Action<string> LicenseAcquired;

		public static void InvokeLicenseAcquired(string licenseName)
		{
			LicenseAcquired?.Invoke(licenseName);
		}
	}
}
