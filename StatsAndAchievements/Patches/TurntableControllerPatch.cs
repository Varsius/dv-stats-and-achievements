using UnityEngine;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

internal readonly struct TurntableRotationState
{
	public TurntableRotationState(float angleBefore)
	{
		AngleBefore = angleBefore;
	}

	public float AngleBefore { get; }
}

[HarmonyPatch(typeof(TurntableController))]
internal static class TurntableControllerPatch
{
	[HarmonyPatch("FixedUpdate")]
	[HarmonyPrefix]
	private static void FixedUpdatePrefix(TurntableController __instance, out TurntableRotationState __state)
	{
		__state = new TurntableRotationState(__instance.turntable.currentYRotation);
	}

	[HarmonyPatch("FixedUpdate")]
	[HarmonyPostfix]
	private static void FixedUpdatePostfix(TurntableController __instance, TurntableRotationState __state)
	{
		float degreesRotated = Mathf.Abs(Mathf.DeltaAngle(__state.AngleBefore, __instance.turntable.currentYRotation));
		if (degreesRotated <= 0f)
		{
			return;
		}

		Events.Actions.InvokeTurntableRotated(degreesRotated);
	}
}
