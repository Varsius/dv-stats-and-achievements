using System;
using System.Collections.Generic;
using System.Linq;
using DV.UI;
using DV.Utils;

namespace StatsAndAchievements.Achievements
{
	public class AchievementManager
	{
		public IReadOnlyList<AchievementCategory> Categories { get; }
		public readonly Dictionary<string, AchievementListener> _listeners = new();

		// Other components (like the UI) can subscribe to this,
		// in order to get notified on each newly unlocked achievement
		// TODO: is this needed at all?
		public event Action<string>? Unlocked;

		public AchievementManager(IEnumerable<AchievementCategory> categories)
		{
			Categories = categories.ToList();

			foreach (var category in Categories)
			{
				foreach (var l in category.Achievements)
				{
					if (l is null) continue;
					if (_listeners.ContainsKey(l.Id))
						throw new InvalidOperationException($"Duplicate achievement id: {l.Id}");

					l.Attach(this);
					_listeners.Add(l.Id, l);
				}
			}
		}

		internal void NotifyUnlocked(string achievementId)
		{
			SingletonBehaviour<ACanvasController<CanvasController.ElementType>>.Instance.NotificationManager.ShowNotification($"Achievement Unlocked:\n{_listeners[achievementId].Title}", localize: false, duration: 10f);
			Main.Log($"Achievement unlocked: {_listeners[achievementId].Title}");
			Unlocked?.Invoke(achievementId);
		}

		internal void NotifyProgress(string achievementId, string progress)
		{
			SingletonBehaviour<ACanvasController<CanvasController.ElementType>>.Instance.NotificationManager.ShowNotification($"{achievementId}: {progress}", localize: false, duration: 5f);
			Main.Log($"Achievement progress: {achievementId} - {progress}");
		}

		internal void Detach()
		{
			foreach (var listener in _listeners.Values)
				listener.Detach();
		}
	}
}
