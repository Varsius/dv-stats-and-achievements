using DV.ThingTypes;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

