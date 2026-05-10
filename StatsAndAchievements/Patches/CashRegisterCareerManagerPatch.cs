using DV.CashRegister;
using DV.ServicePenalty.UI;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(CashRegisterCareerManager))]
internal static class CashRegisterCareerManagerPatch
{
	[HarmonyPatch(nameof(CashRegisterCareerManager.Buy))]
	[HarmonyPrefix]
	private static void BuyPrefix(CashRegisterCareerManager __instance, out float __state)
	{
		__state = (float)__instance.GetTotalCost();
	}

	[HarmonyPatch(nameof(CashRegisterCareerManager.Buy))]
	[HarmonyPostfix]
	private static void BuyPostfix(CashRegisterCareerManager __instance, float __state, bool __result)
	{
		if (!__result || __state <= 0f)
		{
			return;
		}

		if (__instance.GetComponentInParent<CareerManagerFeePayingScreen>() != null)
		{
			Events.Actions.InvokeMoneySpent(__state, Events.MoneySpentSource.CareerManagerFees);
			return;
		}

		if (__instance.GetComponentInParent<CareerManagerLicensePayingScreen>() != null)
		{
			Events.Actions.InvokeMoneySpent(__state, Events.MoneySpentSource.CareerManagerLicense);
		}
	}
}
