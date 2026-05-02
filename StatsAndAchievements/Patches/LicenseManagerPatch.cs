using DV.ThingTypes;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(LicenseManager))]
internal static class LicenseManagerPatch
{
	[HarmonyPatch(nameof(LicenseManager.AcquireGeneralLicense), new[] { typeof(GeneralLicenseType_v2) })]
	[HarmonyPostfix]
	private static void AcquireGeneralLicensePostfix(GeneralLicenseType_v2 license)
	{
		Events.Actions.InvokeLicenseAcquired(license.name);
	}

	[HarmonyPatch(nameof(LicenseManager.AcquireJobLicense), new[] { typeof(JobLicenseType_v2) })]
	[HarmonyPostfix]
	private static void AcquireJobLicensePostfix(JobLicenseType_v2 newLicense)
	{
		Events.Actions.InvokeLicenseAcquired(newLicense.name);
	}
}

