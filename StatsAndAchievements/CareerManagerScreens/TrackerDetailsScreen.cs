using DV.ServicePenalty.UI;
using StatsAndAchievements.Trackers;
using System.Collections.Generic;

namespace StatsAndAchievements.CareerManagerScreens;

public class TrackerDetailsScreen : IModularScreen
{
	private readonly ModularScreenHost _host;
	private readonly StatTracker _tracker;

	public IModularScreen? Parent { get; }
	public ModularScreenHost? Host => _host;
	public IModularScreen.ShowScreen? Show { get; }
	public IModularScreen.HideScreen? Hide { get; }
	public IModularScreen.ScreenInput? Input { get; }

	public TrackerDetailsScreen(IModularScreen parent, ModularScreenHost host, StatTracker tracker)
	{
		Parent = parent;
		_host = host;
		_tracker = tracker;
		Show = OnShow;
		Hide = OnHide;
		Input = OnInput;
	}

	private void OnShow(IModularScreen? previous)
	{
		if (_host.Title == null)
		{
			_host.Exit();
			return;
		}

		_host.Title.text = _tracker.Title;

		var options = new List<(
			LinesScrollerScreen.OptionParser?,
			LinesScrollerScreen.OptionParser?,
			LinesScrollerScreen.CanEnter?
		)>();

		foreach (StatTracker.DetailEntry detail in _tracker.Details())
		{
			options.Add((
				tmPro => { tmPro.text = detail.Title; },
				tmPro => { tmPro.text = detail.Value; },
				null
			));
		}

		_host.Scroller?.SetOptions(options);
	}

	private void OnHide(IModularScreen? next)
	{
	}

	private void OnInput(InputAction action)
	{
		switch (action)
		{
			case InputAction.Up:
				_host.Scroller?.Up();
				break;
			case InputAction.Down:
				_host.Scroller?.Down();
				break;
			case InputAction.Cancel:
				if (Parent == null)
				{
					_host.Exit();
				}
				else
				{
					_host.SwitchToScreen(Parent);
				}
				break;
		}
	}
}
