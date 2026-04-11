using DV.ServicePenalty.UI;
using StatsAndAchievements.Achievements;

namespace StatsAndAchievements.CareerManagerScreens;

public class AchievementDescriptionScreen : IModularScreen
{
	private readonly ModularScreenHost _host;
	private readonly AchievementListener _achievement;

	public IModularScreen? Parent { get; }
	public ModularScreenHost? Host => _host;
	public IModularScreen.ShowScreen? Show { get; }
	public IModularScreen.HideScreen? Hide { get; }
	public IModularScreen.ScreenInput? Input { get; }

	public AchievementDescriptionScreen(IModularScreen parent, ModularScreenHost host, AchievementListener achievement)
	{
		Parent = parent;
		_host = host;
		_achievement = achievement;
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

		_host.Title.text = _achievement.Title;
		if (_host.Subtitle != null)
			_host.Subtitle.text = GetStatusText();

		_host.Paragraphs.ParagraphB.text = _achievement.Description.Trim();
	}

	private void OnHide(IModularScreen? next)
	{
	}

	private void OnInput(InputAction action)
	{
		switch (action)
		{
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

	private string GetStatusText()
	{
		if (_achievement.IsUnlocked())
			return "Unlocked";

		if (_achievement.Type == AchievementType.Progress)
			return $"Progress: {((ProgressAchievementListener)_achievement).Progress()}";

		return "Locked";
	}
}
