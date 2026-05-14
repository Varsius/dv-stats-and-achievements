using System.Collections.Generic;
using DV;
using DV.Damage;
using DV.ThingTypes;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

internal readonly struct VehicleDamageState
{
	public VehicleDamageState(float previousHealth)
	{
		PreviousHealth = previousHealth;
	}

	public float PreviousHealth { get; }
}

[HarmonyPatch(typeof(DamageController))]
internal static class DamageControllerVehicleDamagePatch
{
	internal static readonly Dictionary<TrainDamage, ResourceType> ResourceTypesByDamage = [];

	[HarmonyPatch(nameof(DamageController.InitializeTrainCarScripts))]
	[HarmonyPostfix]
	private static void InitializeTrainCarScriptsPostfix(DamageController __instance)
	{
		if (__instance.wheels != null)
		{
			ResourceTypesByDamage[__instance.wheels] = ResourceType.Wheels_DMG;
		}

		if (__instance.mechanicalPT != null)
		{
			ResourceTypesByDamage[__instance.mechanicalPT] = ResourceType.MechanicalPowertrain_DMG;
		}

		if (__instance.electricalPT != null)
		{
			ResourceTypesByDamage[__instance.electricalPT] = ResourceType.ElectricalPowertrain_DMG;
		}
	}
}

[HarmonyPatch(typeof(CarDamageModel))]
internal static class CarDamageModelVehicleDamagePatch
{
	[HarmonyPatch(nameof(CarDamageModel.DamageCar))]
	[HarmonyPrefix]
	private static void DamageCarPrefix(CarDamageModel __instance, out VehicleDamageState __state)
	{
		__state = new VehicleDamageState(__instance.EffectiveHealthPercentage100Notation);
	}

	[HarmonyPatch(nameof(CarDamageModel.DamageCar))]
	[HarmonyPostfix]
	private static void DamageCarPostfix(CarDamageModel __instance, VehicleDamageState __state)
	{
		float effectiveHealthLoss = (__state.PreviousHealth - __instance.EffectiveHealthPercentage100Notation).To1Decimal();
		if (effectiveHealthLoss <= 0f)
		{
			return;
		}

		float unitPrice = ResourceTypes.GetFullUnitPriceOfResource(ResourceType.Car_DMG, carLivery: __instance.trainCar.carLivery, gameParams: Globals.G.GameParams.ResourcesParams);
		float totalPrice = (effectiveHealthLoss * unitPrice).To2Decimals();
		if (totalPrice <= 0f)
		{
			return;
		}

		Events.Actions.InvokeVehicleDamageIncurred(totalPrice);
	}
}

[HarmonyPatch(typeof(TrainDamage))]
internal static class TrainDamageVehicleDamagePatch
{
	[HarmonyPatch(nameof(TrainDamage.ApplyDamage))]
	[HarmonyPrefix]
	private static void ApplyDamagePrefix(TrainDamage __instance, out VehicleDamageState __state)
	{
		__state = new VehicleDamageState(__instance.HealthPercentage100Notation);
	}

	[HarmonyPatch(nameof(TrainDamage.ApplyDamage))]
	[HarmonyPostfix]
	private static void ApplyDamagePostfix(TrainDamage __instance, VehicleDamageState __state)
	{
		if (!DamageControllerVehicleDamagePatch.ResourceTypesByDamage.TryGetValue(__instance, out ResourceType resourceType))
		{
			return;
		}

		float healthLoss = (__state.PreviousHealth - __instance.HealthPercentage100Notation).To1Decimal();
		if (healthLoss <= 0f)
		{
			return;
		}

		float unitPrice = ResourceTypes.GetFullUnitPriceOfResource(resourceType, gameParams: Globals.G.GameParams.ResourcesParams);
		float totalPrice = (healthLoss * unitPrice).To2Decimals();
		if (totalPrice <= 0f)
		{
			return;
		}

		Events.Actions.InvokeVehicleDamageIncurred(totalPrice);
	}
}
