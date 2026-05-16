using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(WorldStreamingInit))]
internal static class WorldStreamingInitPatch
{
	[HarmonyPatch(nameof(WorldStreamingInit.LoadingRoutine))]
	[HarmonyPostfix]
	private static IEnumerator LoadingRoutinePostfix(IEnumerator __result)
	{
		float loadingStartedAt = Time.realtimeSinceStartup;

		while (__result.MoveNext())
		{
			yield return __result.Current;
		}

		float loadingDurationSeconds = Time.realtimeSinceStartup - loadingStartedAt;
		if (loadingDurationSeconds > 0f)
		{
			SavedFloat loadingScreenTimeSeconds = new SavedFloat("loading_screen_time_seconds");
			loadingScreenTimeSeconds.Value += loadingDurationSeconds;
		}

		if (Main.statTrackerManager != null)
		{
			Main.statTrackerManager.Detach();
		}

		if (Main.achievementManager != null)
		{
			Main.achievementManager.Detach();
		}

		Main.RebuildStatTrackerManager();
		Main.RebuildAchievementManager();
	}
}
