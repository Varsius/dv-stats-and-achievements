using DV.Logic.Job;
using HarmonyLib;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(JobsManager))]
internal static class JobsManagerPatch
{
	[HarmonyPatch(nameof(JobsManager.AbandonJob))]
	[HarmonyPostfix]
	private static void AbandonJobPostfix(Job job)
	{
		Events.Actions.InvokeJobCancellation(job);
	}
}
