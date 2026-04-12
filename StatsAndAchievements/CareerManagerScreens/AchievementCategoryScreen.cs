using DV.ServicePenalty.UI;
using StatsAndAchievements.Achievements;
using System.Collections.Generic;
using System.Linq;

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

		Title.text = "Achievements"; // TODO: include overall percentage of unlocked achievements in all categories
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
				tmPro => { tmPro.text = GetCategoryProgressText(category); },
				null
			));
		}

		Scroller?.SetOptions(options);
	}

	private static string GetCategoryProgressText(AchievementCategory category)
	{
		int achievementCount = category.Achievements.Count;
		if (achievementCount == 0)
		{
			return "0%";
		}

		int unlockedCount = category.Achievements.Count(achievement => achievement.IsUnlocked());
		int unlockedPercentage = unlockedCount * 100 / achievementCount;
		return $"{unlockedPercentage}%";
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
