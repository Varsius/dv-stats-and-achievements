using DV.ServicePenalty.UI;
using HarmonyLib;
using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;
using StatsAndAchievements.Achievements;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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
	public static AchievementManager achievementManager = null!;
	public static Settings settings = null!;
	public static JObject saaSaveData = new JObject();
	private static bool Load(UnityModManager.ModEntry modEntry)
	{
		ModEntry = modEntry;

		try
		{
			settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

			_harmony = new Harmony(modEntry.Info.Id);
			_harmony.PatchAll(Assembly.GetExecutingAssembly());

			watchGO = new GameObject("SAAEventWatch");
			UnityEngine.Object.DontDestroyOnLoad(watchGO);
			watchGO.AddComponent<Events.EventWatch>();

			CareerManagerAPI.CareerManagerAPI.CareerManagerAwake += (tracker, locationName, station, trainCar) =>
			{
				tracker.TryAddToMainScreen<CareerManagerScreens.AchievementCategoryScreen>("Achievements", null,
					after: CareerManagerLocalization.STATS);
				// TODO: ideally this should extend/replace the original "Stats" screen
				tracker.TryAddToMainScreen<CareerManagerScreens.StatisticsScreen>("Advanced Stats", null,
					after: CareerManagerLocalization.STATS);
			};

			modEntry.OnGUI = DrawGUI;
			modEntry.OnSaveGUI = SaveGUI;
			modEntry.OnUnload = Unload;

			Main.RebuildAchievementManager();

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
		if (achievementManager != null)
			achievementManager.Detach();

		_harmony?.UnpatchAll(modEntry.Info.Id);
		if (watchGO != null)
		{
			GameObject.Destroy(watchGO);
		}
		return true;
	}

	private static void DrawGUI(UnityModManager.ModEntry modEntry)
	{
		settings.Draw(modEntry);
	}

	private static void SaveGUI(UnityModManager.ModEntry modEntry)
	{
		settings.Save(modEntry);
	}

	public static void RebuildAchievementManager()
	{
		if (achievementManager != null)
		{
			achievementManager.Detach();
		}

		achievementManager = new AchievementManager(BuildAchievementCategories());
	}

	public static IReadOnlyList<AchievementCategory> BuildAchievementCategories()
	{
		List<AchievementCategory> categories = new List<AchievementCategory>();

		if (settings.EnableCareerAchievements)
		{
			categories.Add(CreateCategory("career", "Career", [
				typeof(AllRounderAchievementListener),
				typeof(VisitAllStationsAchievementListener),
				typeof(CompleteTheMuseumAchievementListener),
				typeof(DriveEveryVehicleAchievementListener),
				typeof(FirstHazmatJobCompletionAchievementListener),
				typeof(FirstMilitaryJobCompletionAchievementListener),
				typeof(FirstFragileJobCompletionAchievementListener),
				typeof(TheEndAchievementListener),
				typeof(TheEngineerAchievementListener),
				typeof(GraphicDesignIsMyPassionAchievementListener),
				typeof(GarageSaleEnthusiastAchievementListener)
			]));
		}

		if (settings.EnableAdvancedAchievements)
		{
			categories.Add(CreateCategory("advanced", "Advanced", [
				typeof(SpeedDemonAchievementListener),
				typeof(MaximumHonkAchievementListener)
			]));
		}

		if (settings.EnableMilestoneAchievements)
		{
			categories.Add(CreateCategory("milestones", "Milestones", [
				typeof(ScroogeMcDuckAchievementListener)
			]));
		}

		if (settings.EnableSecretAchievements)
		{
			categories.Add(CreateCategory("secret", "Secret", [
				typeof(HomeSweetHomeAchievementListener)
			]));
		}

		// TODO: "Steam Engine Connoisseur" category (see ACHIEVEMENTS.md)
		return categories;
	}

	private static AchievementCategory CreateCategory(string id, string title, IEnumerable<Type> achievementTypes)
	{
		List<AchievementListener> achievements = new List<AchievementListener>();

		foreach (Type achievementType in achievementTypes)
		{
			try
			{
				if (Activator.CreateInstance(achievementType) is AchievementListener achievement)
				{
					achievements.Add(achievement);
					continue;
				}

				Warning($"Failed to create achievement listener {achievementType.FullName}: created instance is not an AchievementListener.");
			}
			catch (Exception ex)
			{
				ModEntry.Logger.LogException($"Failed to create achievement listener {achievementType.FullName}:", ex);
			}
		}

		return new AchievementCategory(id, title, achievements);
	}
}
