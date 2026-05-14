using System;
using DV;
using DV.InventorySystem;
using DV.Utils;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

// TODO: this vibe-coded pile works but I do not like the way it is written
// AI: remove shouldTrackAction and prefer clearity over duplication avoidance
internal readonly struct CommsRadioMoneySpentState
{
	public CommsRadioMoneySpentState(bool shouldTrackAction, float amount, double previousMoney)
	{
		ShouldTrackAction = shouldTrackAction;
		Amount = amount;
		PreviousMoney = previousMoney;
	}

	public bool ShouldTrackAction { get; }
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
		bool shouldTrackAction = __instance.CurrentState == RerailController.State.ConfirmRerail;
		__state = CommsRadioMoneySpentHelper.CaptureState(shouldTrackAction, shouldTrackAction ? ___rerailPrice : 0f);
	}

	[HarmonyPatch(nameof(RerailController.OnUse))]
	[HarmonyPostfix]
	private static void OnUsePostfix(CommsRadioMoneySpentState __state)
	{
		CommsRadioMoneySpentHelper.InvokeIfMoneySpent(__state, Events.MoneySpentSource.CommsRadioRerail, Events.CommsRadioActionType.Rerail);
	}
}

[HarmonyPatch(typeof(CommsRadioCarDeleter))]
internal static class CommsRadioCarDeleterMoneySpentPatch
{
	[HarmonyPatch(nameof(CommsRadioCarDeleter.OnUse))]
	[HarmonyPrefix]
	private static void OnUsePrefix(CommsRadioCarDeleter __instance, float ___removePrice, out CommsRadioMoneySpentState __state)
	{
		bool shouldTrackAction = __instance.CurrentState == CommsRadioCarDeleter.State.ConfirmDelete;
		__state = CommsRadioMoneySpentHelper.CaptureState(shouldTrackAction, shouldTrackAction ? ___removePrice : 0f);
	}

	[HarmonyPatch(nameof(CommsRadioCarDeleter.OnUse))]
	[HarmonyPostfix]
	private static void OnUsePostfix(CommsRadioMoneySpentState __state)
	{
		CommsRadioMoneySpentHelper.InvokeIfMoneySpent(__state, Events.MoneySpentSource.CommsRadioCarRemoval, Events.CommsRadioActionType.CarRemoval);
	}
}

[HarmonyPatch(typeof(CommsRadioCrewVehicle))]
internal static class CommsRadioCrewVehicleMoneySpentPatch
{
	[HarmonyPatch(nameof(CommsRadioCrewVehicle.OnUse))]
	[HarmonyPrefix]
	private static void OnUsePrefix(CommsRadioCrewVehicle __instance, out CommsRadioMoneySpentState __state)
	{
		bool shouldTrackAction = __instance.CurrentState == CommsRadioCrewVehicle.State.ConfirmSummon;
		float amount = shouldTrackAction
			? Traverse.Create(__instance).Property("SummonPrice").GetValue<float>()
			: 0f;
		__state = CommsRadioMoneySpentHelper.CaptureState(shouldTrackAction, amount);
	}

	[HarmonyPatch(nameof(CommsRadioCrewVehicle.OnUse))]
	[HarmonyPostfix]
	private static void OnUsePostfix(CommsRadioMoneySpentState __state)
	{
		CommsRadioMoneySpentHelper.InvokeIfMoneySpent(__state, Events.MoneySpentSource.CommsRadioUtilityVehicleSummon, Events.CommsRadioActionType.UtilityVehicleSummon);
	}
}

internal static class CommsRadioMoneySpentHelper
{
	internal static CommsRadioMoneySpentState CaptureState(bool shouldTrackAction, float amount)
	{
		double previousMoney = SingletonBehaviour<Inventory>.Instance?.PlayerMoney ?? 0d;
		return new CommsRadioMoneySpentState(shouldTrackAction, amount, previousMoney);
	}

	internal static void InvokeIfMoneySpent(CommsRadioMoneySpentState state, Events.MoneySpentSource source, Events.CommsRadioActionType actionType)
	{
		if (!state.ShouldTrackAction)
		{
			return;
		}

		if (state.Amount <= 0f)
		{
			Events.Actions.InvokeCommsRadioActionPerformed(actionType);
			return;
		}

		double currentMoney = SingletonBehaviour<Inventory>.Instance?.PlayerMoney ?? state.PreviousMoney;
		if (Math.Abs((state.PreviousMoney - currentMoney) - state.Amount) > 0.01d)
		{
			return;
		}

		Events.Actions.InvokeCommsRadioActionPerformed(actionType);
		Events.Actions.InvokeMoneySpent(state.Amount, source);
	}
}
