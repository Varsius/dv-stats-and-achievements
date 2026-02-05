using DV.ThingTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(LicenseManager))]
internal class LicenseManagerPatch
{
	[HarmonyPatch(nameof(LicenseManager.AcquireGeneralLicense))]
	[HarmonyPostfix]
	public static void AcquireGeneralLicensePatch(GeneralLicenseType_v2 license)
	{
		Main.Log($"Acquire General License {license.name}");
		// Main.LicenseAcquired?.Invoke(license.name);
	}

	[HarmonyPatch(nameof(LicenseManager.AcquireJobLicense))]
	[HarmonyPostfix]
	public static void AcquireJobLicensePatch(GeneralLicenseType_v2 license)
	{
		Main.Log($"Acquire Job License {license.name}");
	}
}
