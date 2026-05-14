using System;
using DV;
using DV.InventorySystem;
using DV.Utils;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

internal readonly struct CommsRadioMoneySpentState
{
	public CommsRadioMoneySpentState(float amount, double previousMoney)
	{
		Amount = amount;
		PreviousMoney = previousMoney;
	}

	public float Amount { get; }
	public double PreviousMoney { get; }
}

[HarmonyPatch(typeof(RerailController))]
internal static class RerailControllerMoneySpentPatch
{
	[HarmonyPatch(nameof(RerailController.OnUse))]
	[HarmonyPrefix]
	private static void OnUsePrefix(RerailController __instance, float ___rerailPrice, out CommsRadioMoneySpentState __state)
	{
		__state = CommsRadioMoneySpentHelper.CaptureState(__instance.CurrentState == RerailController.State.ConfirmRerail ? ___rerailPrice : 0f);
	}

	[HarmonyPatch(nameof(RerailController.OnUse))]
	[HarmonyPostfix]
	private static void OnUsePostfix(CommsRadioMoneySpentState __state)
	{
		CommsRadioMoneySpentHelper.InvokeIfMoneySpent(__state, Events.MoneySpentSource.CommsRadioRerail);
	}
}

[HarmonyPatch(typeof(CommsRadioCarDeleter))]
internal static class CommsRadioCarDeleterMoneySpentPatch
{
	[HarmonyPatch(nameof(CommsRadioCarDeleter.OnUse))]
	[HarmonyPrefix]
	private static void OnUsePrefix(CommsRadioCarDeleter __instance, float ___removePrice, out CommsRadioMoneySpentState __state)
	{
		__state = CommsRadioMoneySpentHelper.CaptureState(__instance.CurrentState == CommsRadioCarDeleter.State.ConfirmDelete ? ___removePrice : 0f);
	}

	[HarmonyPatch(nameof(CommsRadioCarDeleter.OnUse))]
	[HarmonyPostfix]
	private static void OnUsePostfix(CommsRadioMoneySpentState __state)
	{
		CommsRadioMoneySpentHelper.InvokeIfMoneySpent(__state, Events.MoneySpentSource.CommsRadioCarRemoval);
	}
}

[HarmonyPatch(typeof(CommsRadioCrewVehicle))]
internal static class CommsRadioCrewVehicleMoneySpentPatch
{
	[HarmonyPatch(nameof(CommsRadioCrewVehicle.OnUse))]
	[HarmonyPrefix]
	private static void OnUsePrefix(CommsRadioCrewVehicle __instance, out CommsRadioMoneySpentState __state)
	{
		float amount = __instance.CurrentState == CommsRadioCrewVehicle.State.ConfirmSummon
			? Traverse.Create(__instance).Property("SummonPrice").GetValue<float>()
			: 0f;
		__state = CommsRadioMoneySpentHelper.CaptureState(amount);
	}

	[HarmonyPatch(nameof(CommsRadioCrewVehicle.OnUse))]
	[HarmonyPostfix]
	private static void OnUsePostfix(CommsRadioMoneySpentState __state)
	{
		CommsRadioMoneySpentHelper.InvokeIfMoneySpent(__state, Events.MoneySpentSource.CommsRadioUtilityVehicleSummon);
	}
}

internal static class CommsRadioMoneySpentHelper
{
	internal static CommsRadioMoneySpentState CaptureState(float amount)
	{
		double previousMoney = SingletonBehaviour<Inventory>.Instance?.PlayerMoney ?? 0d;
		return new CommsRadioMoneySpentState(amount, previousMoney);
	}

	internal static void InvokeIfMoneySpent(CommsRadioMoneySpentState state, Events.MoneySpentSource source)
	{
		if (state.Amount <= 0f)
		{
			return;
		}

		double currentMoney = SingletonBehaviour<Inventory>.Instance?.PlayerMoney ?? state.PreviousMoney;
		if (Math.Abs((state.PreviousMoney - currentMoney) - state.Amount) > 0.01d)
		{
			return;
		}

		Events.Actions.InvokeMoneySpent(state.Amount, source);
	}
}
