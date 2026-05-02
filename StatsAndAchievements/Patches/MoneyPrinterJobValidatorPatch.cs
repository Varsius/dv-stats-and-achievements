using HarmonyLib;
using DV.Logic.Job;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(MoneyPrinterJobValidator))]
internal static class MoneyPrinterJobValidatorPatch
{
	[HarmonyPatch(nameof(MoneyPrinterJobValidator.PrintPayment))]
	[HarmonyPrefix]
	private static bool PrintPaymentPrefix(Job job)
	{
		Events.Actions.InvokeJobCompletion(job);
		return true;
	}
}

