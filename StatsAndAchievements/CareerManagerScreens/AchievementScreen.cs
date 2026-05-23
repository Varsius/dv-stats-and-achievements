using DV.ServicePenalty.UI;
using StatsAndAchievements.Achievements;
using System.Collections.Generic;

namespace StatsAndAchievements.CareerManagerScreens;

public class AchievementScreen : IModularScreen
{
	private readonly ModularScreenHost _host;
	private readonly AchievementCategory _category;
	private IReadOnlyList<AchievementListener> _achievements = [];

	public IModularScreen? Parent { get; }
	public ModularScreenHost? Host => _host;
	public IModularScreen.ShowScreen? Show { get; }
	public IModularScreen.HideScreen? Hide { get; }
	public IModularScreen.ScreenInput? Input { get; }

	public AchievementScreen(IModularScreen parent, ModularScreenHost host, AchievementCategory category)
	{
		Parent = parent;
		_host = host;
		_category = category;
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
		_host.Title.text = _category.Title;
		_achievements = _category.Achievements;

		var options = new List<(
			LinesScrollerScreen.OptionParser?,
			LinesScrollerScreen.OptionParser?,
			LinesScrollerScreen.CanEnter?
		)>();

		foreach (AchievementListener listener in _achievements)
		{
			options.Add((
				tmPro =>
				{
					if (listener.IsUnlocked())
					{
						tmPro.text = $"★{listener.GetDisplayTitle()}";
					}
					else
					{
						tmPro.text = listener.GetDisplayTitle();
					}
				},
				tmPro =>
				{
					if (listener.IsUnlocked())
					{
						tmPro.text = "Unlocked";
					}
					else if (listener.Type == Achievements.AchievementType.Progress)
					{
						tmPro.text = $"{((ProgressAchievementListener)listener).Progress()}";
					}
					else
					{
						tmPro.text = "";
					}
				},
				null
			));
		}

		if (_category.Id != "career")
		{
			options.Add((
				tmPro => { tmPro.text = "Coming Soon"; },
				tmPro => { tmPro.text = string.Empty; },
				() => false
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
			case InputAction.Confirm:
				if (_host.Scroller == null || _host.Scroller.SelectedIndex < 0 || _host.Scroller.SelectedIndex >= _achievements.Count)
					return;

				_host.SwitchToScreen(new AchievementDescriptionScreen(this, _host, _achievements[_host.Scroller.SelectedIndex]));
				break;
			case InputAction.PrintInfo:
				// TODO: print "license info"-like object that shows the achievement description (and progress)
				break;
			case InputAction.Cancel:
				if (Parent == null)
					_host.Exit();
				else
					_host.SwitchToScreen(Parent);
				break;
		}
	}
}
