using System.Linq;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(TrainCar))]
internal static class TrainCarDerailmentPatch
{
	[HarmonyPatch(nameof(TrainCar.Derail))]
	[HarmonyPrefix]
	private static void DerailPrefix(TrainCar __instance, out bool __state)
	{
		TrainCar? playerCar = PlayerManager.Car;
		Trainset? playerTrainset = playerCar?.trainset;

		__state = playerTrainset != null
			&& __instance.trainset == playerTrainset
			&& !__instance.derailed
			&& !playerTrainset.cars.Any(car => car != null && car.derailed);
	}

	[HarmonyPatch(nameof(TrainCar.Derail))]
	[HarmonyPostfix]
	private static void DerailPostfix(TrainCar __instance, bool __state)
	{
		if (!__state || !__instance.derailed)
		{
			return;
		}

		Events.Actions.InvokePlayerDerailed();
	}
}
