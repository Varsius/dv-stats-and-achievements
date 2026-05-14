using DV;
using DV.Damage;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

internal readonly struct CargoDamageState
{
	public CargoDamageState(CargoType cargoType, float previousEffectiveHealth)
	{
		CargoType = cargoType;
		PreviousEffectiveHealth = previousEffectiveHealth;
	}

	public CargoType CargoType { get; }
	public float PreviousEffectiveHealth { get; }
}

[HarmonyPatch(typeof(CargoDamageModel))]
internal static class CargoDamageModelPatch
{
	[HarmonyPatch("ApplyDamageToCargo")]
	[HarmonyPrefix]
	private static void ApplyDamageToCargoPrefix(CargoDamageModel __instance, out CargoDamageState __state)
	{
		__state = new CargoDamageState(__instance.cargoType, __instance.EffectiveHealthPercentage100Notation);
	}

	[HarmonyPatch("ApplyDamageToCargo")]
	[HarmonyPostfix]
	private static void ApplyDamageToCargoPostfix(CargoDamageModel __instance, CargoDamageState __state)
	{
		if (__state.CargoType <= CargoType.None)
		{
			return;
		}

		float effectiveHealthLoss = (__state.PreviousEffectiveHealth - __instance.EffectiveHealthPercentage100Notation).To1Decimal();
		if (effectiveHealthLoss <= 0f)
		{
			return;
		}

		float unitPrice = ResourceTypes.GetFullUnitPriceOfResource(ResourceType.Cargo_DMG, cargoType: __state.CargoType.ToV2(), gameParams: Globals.G.GameParams.ResourcesParams);
		float totalPrice = (effectiveHealthLoss * unitPrice).To2Decimals();
		if (totalPrice <= 0f)
		{
			return;
		}

		Events.Actions.InvokeCargoDamageIncurred(totalPrice);
	}
}
