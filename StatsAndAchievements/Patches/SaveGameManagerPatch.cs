using HarmonyLib;
using Newtonsoft.Json.Linq;

namespace StatsAndAchievements.Patches;

[HarmonyPatch(typeof(SaveGameManager))]
internal class SaveGameManagerPatch
{
	private const string SAA_SAVE_DATA_KEY = "statsandachievements";

	[HarmonyPatch(nameof(SaveGameManager.UpdateInternalData))]
	[HarmonyPrefix]
	public static bool UpdateInternalDataPatch(SaveGameManager __instance)
	{
		Main.Log("Writing to save");
		__instance.data.SetJObject(SAA_SAVE_DATA_KEY, Main.saaSaveData);
		return true;
	}

	[HarmonyPatch(nameof(SaveGameManager.FindStartGameData))]
	[HarmonyPostfix]
	public static void FindStartGameDataPatch(SaveGameManager __instance)
	{
		Main.ResetAchievementManager();
		Main.Log("Attempting load from save");
		JObject saveData = __instance.data.GetJObject(SAA_SAVE_DATA_KEY);

		if (saveData != null)
		{
			Main.Log("Loaded save data");
			Main.saaSaveData = saveData;
		}
		else
		{
			Main.Log("No save data found, creating new");
			Main.saaSaveData = new JObject();
		}
	}
}
