using System.Collections.Generic;
using System.Linq;

namespace StatsAndAchievements.Achievements
{
	public sealed class AchievementCategory
	{
		public string Id { get; }
		public string Title { get; }
		public int SortOrder { get; }
		public IReadOnlyList<AchievementListener> Achievements { get; }

		public AchievementCategory(string id, string title, int sortOrder, IEnumerable<AchievementListener> achievements)
		{
			Id = id;
			Title = title;
			SortOrder = sortOrder;
			Achievements = achievements
				.OrderBy(achievement => achievement.SortOrder)
				.ToList();
		}
	}
}
