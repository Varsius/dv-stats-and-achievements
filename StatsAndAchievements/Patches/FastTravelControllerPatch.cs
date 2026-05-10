using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(FastTravelController))]
internal static class FastTravelControllerPatch
{
	[HarmonyPatch("OnFastTravelRequested")]
	[HarmonyPostfix]
	private static void OnFastTravelRequestedPostfix(bool withLoco)
	{
		if (!FastTravelController.IsFastTravelling)
		{
			return;
		}

		Events.Actions.InvokeFastTravelCompleted(withLoco);
	}
}
