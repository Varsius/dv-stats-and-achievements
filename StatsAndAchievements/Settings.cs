using System;
using System.Xml.Serialization;
using UnityModManagerNet;

namespace StatsAndAchievements;

public class Settings : UnityModManager.ModSettings, IDrawable
{
	[Draw("Enable Career achievements")]
	public bool EnableCareerAchievements = true;

	[Draw("Enable Advanced achievements")]
	public bool EnableAdvancedAchievements = true;

	[Draw("Enable Milestone achievements")]
	public bool EnableMilestoneAchievements = true;

	[Draw("Enable Secret achievements")]
	public bool EnableSecretAchievements = true;

	[XmlIgnore]
	public Action<Settings>? OnSettingsSaved;

	[XmlIgnore]
	public Action<Settings>? OnSettingsChanged;

	public override void Save(UnityModManager.ModEntry modEntry)
	{
		Save(this, modEntry);
		OnSettingsSaved?.Invoke(this);
	}

	public void OnChange()
	{
		OnSettingsChanged?.Invoke(this);
	}
}
