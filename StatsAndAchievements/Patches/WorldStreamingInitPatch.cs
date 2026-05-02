using System.Collections;
using HarmonyLib;
using StatsAndAchievements.Achievements;

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

        // TODO: the mod should contain config options to enable/disable each category
        // TODO: move the details on which mods should be loaded somewhere else
        Main.achievementManager = new AchievementManager([
            new AchievementCategory("career", "Career", [
                new AllRounderAchievementListener(),
                new VisitAllStationsAchievementListener(),
                new DriveEveryVehicleAchievementListener(),
                new FirstHazmatJobCompletionAchievementListener(),
                new FirstMilitaryJobCompletionAchievementListener(),
                new FirstFragileJobCompletionAchievementListener(),
                new TheEndAchievementListener(),
                new TheEngineerAchievementListener(),
                new GraphicDesignIsMyPassionAchievementListener(),
				// TODO: fix. new GarageSaleEnthusiastAchievementListener()
			]),
            new AchievementCategory("advanced", "Advanced", [
                new SpeedDemonAchievementListener(),
                new MaximumHonkAchievementListener()
            ]),
            new AchievementCategory("milestones", "Milestones", [
                new ScroogeMcDuckAchievementListener()
            ]),
            new AchievementCategory("secret", "Secret", [
                new HomeSweetHomeAchievementListener()
            ])
			// TODO: "Steam Engine Connoisseur" category (see ACHIEVEMENTS.md)
		]);
    }
}
