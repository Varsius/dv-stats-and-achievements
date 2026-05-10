using System.Collections.Generic;
namespace StatsAndAchievements.Achievements
{
	public sealed class AchievementCategory
	{
		public string Id { get; }
		public string Title { get; }
		public IReadOnlyList<AchievementListener> Achievements { get; }

		public AchievementCategory(string id, string title, IEnumerable<AchievementListener> achievements)
		{
			Id = id;
			Title = title;
			Achievements = new List<AchievementListener>(achievements);
		}
	}
}
