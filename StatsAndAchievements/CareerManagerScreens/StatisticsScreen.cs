using DV.ServicePenalty.UI;
using System.Collections.Generic;
using StatsAndAchievements.Trackers;

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

		if (Main.statTrackerManager == null)
		{
			Scroller?.SetOptions(options);
			return;
		}

		foreach (StatTracker tracker in Main.statTrackerManager.Trackers)
		{
			options.Add((
				tmPro =>
				{
					tmPro.text = tracker.Title;
				},
				tmPro =>
				{
					tmPro.text = tracker.Value();
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
