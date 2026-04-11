using DV.ServicePenalty.UI;
using StatsAndAchievements.Achievements;
using System.Collections.Generic;

namespace StatsAndAchievements.CareerManagerScreens;

public class AchievementCategoryScreen : ModularScreenHost
{
	private IReadOnlyList<AchievementCategory> _categories = [];

	public AchievementCategoryScreen()
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

		Title.text = "Achievements";
		_categories = Main.achievementManager.Categories;

		var options = new List<(
			LinesScrollerScreen.OptionParser?,
			LinesScrollerScreen.OptionParser?,
			LinesScrollerScreen.CanEnter?
		)>();

		foreach (var category in _categories)
		{
			options.Add((
				tmPro => { tmPro.text = category.Title; },
				tmPro => { tmPro.text = $"{category.Achievements.Count} achievements"; },
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
				if (Scroller == null || Scroller.SelectedIndex < 0 || Scroller.SelectedIndex >= _categories.Count)
					return;

				SwitchToScreen(new AchievementScreen(this, this, _categories[Scroller.SelectedIndex]));
				break;
			case InputAction.Cancel:
				Exit();
				break;
		}
	}
}
