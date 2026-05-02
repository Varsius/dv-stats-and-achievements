using System.Collections;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(WorldStreamingInit))]
internal static class WorldStreamingInitPatch
{
    [HarmonyPatch(nameof(WorldStreamingInit.LoadingRoutine))]
    [HarmonyPostfix]
    private static IEnumerator LoadingRoutinePostfix(IEnumerator __result)
    {
        while (__result.MoveNext())
        {
            yield return __result.Current;
        }

        if (Main.achievementManager != null)
        {
            Main.achievementManager.Detach();
        }

        Main.RebuildAchievementManager();
    }
}
