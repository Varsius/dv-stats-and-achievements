using dnlib;
using DV.Customization;
using DV.ServicePenalty.UI;
using DV.Simulation;
using HarmonyLib;
using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;
using CareerManagerAPI;
using StatsAndAchievements.Events;
using StatsAndAchievements.Achievements;
using Newtonsoft.Json.Linq;

namespace StatsAndAchievements;

[EnableReloading]
public class Main
{
	public static UnityModManager.ModEntry ModEntry { get; private set; } = null!;


	/// Log a message only when compiled with debug profile.
	[Conditional("DEBUG")]
	public static void Debug(string msg) => ModEntry.Logger.Log("[Debug] " + msg);
	public static void Log(string msg) => ModEntry.Logger.Log(msg);
	public static void Warning(string msg) => ModEntry.Logger.Warning(msg);
	public static void Error(string msg) => ModEntry.Logger.Error(msg);

	private static Harmony? _harmony;
	private static GameObject? watchGO;
	public static AchievementManager achievementManager;
	public static JObject saaSaveData = new JObject();
	private static bool Load(UnityModManager.ModEntry modEntry)
	{
		ModEntry = modEntry;

		try
		{
			_harmony = new Harmony(modEntry.Info.Id);
			_harmony.PatchAll(Assembly.GetExecutingAssembly());

			watchGO = new GameObject("SAAEventWatch");
			UnityEngine.Object.DontDestroyOnLoad(watchGO);
			watchGO.AddComponent<Events.EventWatch>();

			achievementManager = new AchievementManager([
				new AchievementCategory("career", "Career", [
					new AllRounderAchievementListener(),
					new TheEndAchievementListener()
				]),
				new AchievementCategory("advanced", "Advanced", [
					new SpeedDemonAchievementListener(),
					new MaximumHonkAchievementListener()
				]),
				new AchievementCategory("milestones", "Milestones", [
					new ScroogeMcDuckAchievementListener()
				])
			]);

			CareerManagerAPI.CareerManagerAPI.CareerManagerAwake += (tracker, locationName, station, trainCar) => 
			{
				tracker.TryAddToMainScreen<CareerManagerScreens.AchievementCategoryScreen>("Achievements", null,
					after: CareerManagerLocalization.STATS);
				tracker.TryAddToMainScreen<CareerManagerScreens.StatisticsScreen>("Statistics", null,
					after: CareerManagerLocalization.STATS);
			};

			modEntry.OnUnload = Unload;
			return true;
		}
		catch (Exception ex)
		{
			modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", ex);
			_harmony?.UnpatchAll(modEntry.Info.Id);
			return false;
		}
	}

	private static bool Unload(UnityModManager.ModEntry modEntry)
	{
		_harmony?.UnpatchAll(modEntry.Info.Id);
		if (watchGO != null)
		{
			GameObject.Destroy(watchGO);
		}
		return true;
	}
}
