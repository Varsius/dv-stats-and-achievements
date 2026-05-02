using DV.ThingTypes;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(LicenseManager))]
internal static class LicenseManagerPatch
{
	[HarmonyPatch(nameof(LicenseManager.AcquireGeneralLicense), [typeof(GeneralLicenseType_v2)])]
	[HarmonyPostfix]
	private static void AcquireGeneralLicensePostfix(GeneralLicenseType_v2 license)
	{
		Events.Actions.InvokeGeneralLicenseAcquired(license);
	}

	[HarmonyPatch(nameof(LicenseManager.AcquireJobLicense), [typeof(JobLicenseType_v2)])]
	[HarmonyPostfix]
	private static void AcquireJobLicensePostfix(JobLicenseType_v2 newLicense)
	{
		Events.Actions.InvokeJobLicenseAcquired(newLicense);
	}
}

