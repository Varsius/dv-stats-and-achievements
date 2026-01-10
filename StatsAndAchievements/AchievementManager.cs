using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsAndAchievements
{
	public class AchievementManager
	{
		private readonly Dictionary<string, AchievementListener> _listeners = new();

		// Other components (like the UI) can subscribe to this,
		// in order to get notified on each newly unlocked achievement
		public event Action<string> Unlocked;

		public AchievementManager(IEnumerable<AchievementListener> listeners)
		{
			foreach (var l in listeners)
			{
				if (l is null) continue;
				if (_listeners.ContainsKey(l.Id))
					throw new InvalidOperationException($"Duplicate achievement id: {l.Id}");

				l.Attach(this);
				_listeners.Add(l.Id, l);
			}
		}

		internal void NotifyUnlocked(string achievementId)
		{
			Main.Log($"Achievement unlocked: {achievementId}");
			Unlocked?.Invoke(achievementId);
		}
	}

	public abstract class AchievementListener
	{
		private AchievementManager _manager;
		public abstract string Id { get; }

		// protected bool IsUnlocked => false; // TODO: look this up in save data

		protected bool IsUnlocked = false;

		internal void Attach(AchievementManager manager)
		{
			_manager = manager;

			SubscribeToEvents();
		}

		protected void TriggerUnlock()
		{
			if (IsUnlocked) return;

			// TODO: Store in save data

			// TODO: remove
			IsUnlocked = true;
			_manager.NotifyUnlocked(Id);
		}

		protected virtual void SubscribeToEvents()
		{
			// TODO: for the MVP subscribe to all events
			// With this, all hooks that an achievement does not use will just translate to an empty function being called.
			// If this turns out to hurt the performance, implement event subscription in each achievement individually.

			Watch.SpeedIncreased += OnSpeedIncreased;

			/*
			 var simFlow = trainCar.SimController.simFlow;

			if (simFlow.TryGetPort("horn.HORN", out var port))
			{
				port.ValueUpdatedInternally += (newValue) => Log($"Value: {newValue}"); // eg. 0.561
			}
			 */
		}

		// TODO: Unsubscribe

		// Common hooks
		protected virtual void OnSpeedIncreased(float speed) { }
	}
}
