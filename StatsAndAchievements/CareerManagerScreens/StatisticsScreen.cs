using DV.ServicePenalty.UI;
using System.Collections.Generic;
using System.Linq;
using StatsAndAchievements.Trackers;

namespace StatsAndAchievements.CareerManagerScreens;

public class StatisticsScreen : ModularScreenHost
{
	private IReadOnlyList<StatTracker> _trackers = [];
	private int? _returnSelectedIndex;

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
			_trackers = [];
			Scroller?.SetOptions(options);
			return;
		}

		_trackers = Main.statTrackerManager.Trackers
			.Where(tracker => tracker.ShowInCareerManagerScreen)
			.ToList();

		foreach (StatTracker tracker in _trackers)
		{
			bool hasDetails = tracker.Details().Count > 0;
			options.Add((
				tmPro =>
				{
					tmPro.text = hasDetails ? $"{tracker.Title}…" : tracker.Title;
				},
				tmPro =>
				{
					tmPro.text = tracker.Value();
				},
				null
			));
		}

		Scroller?.SetOptions(options);

		if (previous is TrackerDetailsScreen && _returnSelectedIndex.HasValue && Scroller != null)
		{
			int selectedIndex = _returnSelectedIndex.Value;
			if (selectedIndex >= 0 && selectedIndex < _trackers.Count)
			{
				Scroller.SelectedIndex = selectedIndex;
			}
		}

		if (previous is not TrackerDetailsScreen)
		{
			_returnSelectedIndex = null;
		}
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
			case InputAction.Confirm:
				if (Scroller == null || Scroller.SelectedIndex < 0 || Scroller.SelectedIndex >= _trackers.Count)
				{
					return;
				}

				StatTracker tracker = _trackers[Scroller.SelectedIndex];
				if (tracker.Details().Count > 0)
				{
					_returnSelectedIndex = Scroller.SelectedIndex;
					SwitchToScreen(new TrackerDetailsScreen(this, this, tracker));
				}
				break;
			case InputAction.PrintInfo:
				break;
			case InputAction.Cancel:
				Exit();
				break;
		}
	}
}
