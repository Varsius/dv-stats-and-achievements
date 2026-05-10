using DV.Teleporters;
using DV.UI;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(FastTravelController))]
internal static class FastTravelControllerPatch
{
	[HarmonyPatch("OnFastTravelRequested")]
	[HarmonyPrefix]
	private static void OnFastTravelRequestedPrefix(bool withLoco, FastTravelDestination ___lastMarkerClicked, out float __state)
	{
		FastTravelData fastTravelData = FastTravelController.ExtractFastTravelData(___lastMarkerClicked, PlayerManager.Car);
		__state = withLoco ? fastTravelData.fastTravelWithLocoPrice : fastTravelData.fastTravelPrice;
	}

	[HarmonyPatch("OnFastTravelRequested")]
	[HarmonyPostfix]
	private static void OnFastTravelRequestedPostfix(bool withLoco, float __state)
	{
		if (!FastTravelController.IsFastTravelling)
		{
			return;
		}

		if (__state > 0f)
		{
			Events.Actions.InvokeMoneySpent(__state, Events.MoneySpentSource.FastTravel);
		}

		Events.Actions.InvokeFastTravelCompleted(withLoco);
	}
}
