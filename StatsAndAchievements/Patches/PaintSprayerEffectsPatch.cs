using DV.Items;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(PaintSprayerEffects))]
internal static class PaintSprayerEffectsPatch
{
	[HarmonyPatch(nameof(PaintSprayerEffects.OnSpent))]
	[HarmonyPostfix]
	private static void OnSpentPostfix()
	{
		Events.Actions.InvokePaintJobApplied();
	}
}
