using System.Collections.Generic;
using DV.ThingTypes;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(LocoResourceModule))]
internal static class LocoResourceModulePatch
{
	[HarmonyPatch(nameof(LocoResourceModule.GetBoughtResource))]
	[HarmonyPrefix]
	private static void GetBoughtResourcePrefix(LocoResourceModule __instance, List<CashRegisterModule.CashRegisterModuleData> ___resourceData, out float __state)
	{
		__state = 0f;

		if (!IsTrackedServiceResource(__instance.resourceType))
		{
			return;
		}

		foreach (CashRegisterModule.CashRegisterModuleData resourceData in ___resourceData)
		{
			if (resourceData.unitsToBuy > __instance.BuyMinLimit)
			{
				__state += resourceData.unitsToBuy;
			}
		}
	}

	[HarmonyPatch(nameof(LocoResourceModule.GetBoughtResource))]
	[HarmonyPostfix]
	private static void GetBoughtResourcePostfix(LocoResourceModule __instance, float __state)
	{
		if (__state <= 0f || !IsTrackedServiceResource(__instance.resourceType))
		{
			return;
		}

		Events.Actions.InvokeServiceResourceBought(__instance.resourceType, __state);
	}

	private static bool IsTrackedServiceResource(ResourceType resourceType)
	{
		switch (resourceType)
		{
			case ResourceType.Fuel:
			case ResourceType.Sand:
			case ResourceType.Oil:
			case ResourceType.Water:
			case ResourceType.Coal:
			case ResourceType.ElectricCharge:
				return true;
			default:
				return false;
		}
	}
}
