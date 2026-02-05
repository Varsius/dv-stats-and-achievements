using DV.JObjectExtstensions;
using DV.Localization;
using DV.ServicePenalty.UI;
using DV.Teleporters;
using DV.ThingTypes;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using StatsAndAchievements.Achievements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static UnityModManagerNet.UnityModManager.Repository;

namespace StatsAndAchievements.CareerManagerScreens;

public class StatisticsScreen : ModularScreenHost
{
	public StatisticsScreen()
	{
		Show = OnShow;
		Hide = OnHide;
		Input = OnInput;
		Clear += OnClear;
	}



	private void OnShow(IModularScreen? previous)
	{
		if (Title == null)
		{
			Exit();
			return;
		}
		Title.text = "Statistics";

		var options = new List<(
			LinesScrollerScreen.OptionParser?,
			LinesScrollerScreen.OptionParser?,
			LinesScrollerScreen.CanEnter?
		)>();

		foreach (KeyValuePair<string, AchievementListener> entry in Main.achievementManager._listeners)
		{
			AchievementListener listener = entry.Value;
			if (listener.Type == Achievements.AchievementType.Condition)
			{
				continue;
			}
			ProgressAchievementListener progressListener  = (ProgressAchievementListener)listener;
			options.Add((
				tmPro =>
				{
					tmPro.text = progressListener.ValueName();
				},
				tmPro =>
				{
					tmPro.text = progressListener.Value();
				},
				null
			));
		}

		Scroller?.SetOptions(options);
	}
	private void OnInput(InputAction action)
	{
		switch (action)
		{
			case InputAction.Up:
				Scroller?.Up();
				break;
			case InputAction.Down:
				Scroller?.Down();
				break;
			case InputAction.PrintInfo:
				break;
			case InputAction.Cancel:
				Exit();
				break;
		}
	}
}
