using DV.Shops;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(ScanItemCashRegisterModule))]
internal static class ScanItemCashRegisterModulePatch
{
	[HarmonyPatch(nameof(ScanItemCashRegisterModule.GetBoughtResource))]
	[HarmonyPrefix]
	private static void GetBoughtResourcePrefix(ScanItemCashRegisterModule __instance, out float __state)
	{
		__state = __instance.Data.unitsToBuy > 0f ? __instance.Data.TotalPrice : 0f;
	}

	[HarmonyPatch(nameof(ScanItemCashRegisterModule.GetBoughtResource))]
	[HarmonyPostfix]
	private static void GetBoughtResourcePostfix(float __state)
	{
		if (__state > 0f)
		{
			Events.Actions.InvokeMoneySpent(__state);
		}
	}
}
