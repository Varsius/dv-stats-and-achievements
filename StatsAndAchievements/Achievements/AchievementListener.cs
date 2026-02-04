using DV.InventorySystem;
using DV.JObjectExtstensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsAndAchievements.Achievements
{
	public abstract class AchievementListener
	{
		private AchievementManager _manager;
		public abstract string Id { get; }

		// TODO: look this up in save data
		public bool IsUnlocked() {
			bool? unlocked = Main.saaSaveData.GetBool($"{Id}_unlocked");
			return unlocked.HasValue && unlocked.Value;
		}

		protected string Progress = "0/10";

		private List<Action> unsubscribeActions = new List<Action>();

		~AchievementListener()
		{
			Main.Log($"Destructor {Id}");
			foreach (var unsubscribeAction in unsubscribeActions)
				unsubscribeAction();
		}

		internal void Attach(AchievementManager manager)
		{
			_manager = manager;

			SubscribeToEvents();
		}

		protected void TriggerUnlock()
		{
			if (IsUnlocked()) return;

			Main.saaSaveData.SetBool($"{Id}_unlocked", true);

			_manager.NotifyUnlocked(Id);
		}

		protected virtual void SubscribeToEvents()
		{
			// TODO: for the MVP subscribe to all events
			// With this, all hooks that an achievement does not use will just translate to an empty function being called.
			// If this turns out to hurt the performance, implement event subscription in each achievement individually.

			// TODO: write a meta function that combines subscribing and adding to the unsubscribeActions list

			Events.EventWatch.SpeedIncreased += OnSpeedIncreased;
			unsubscribeActions.Add(() => Events.EventWatch.SpeedIncreased -= OnSpeedIncreased);

			Events.EventWatch.Honked += OnHonked;
			unsubscribeActions.Add(() => Events.EventWatch.Honked -= OnHonked);

			Events.EventWatch.HonkStarted += OnHonkStarted;
			unsubscribeActions.Add(() => Events.EventWatch.HonkStarted -= OnHonkStarted);

			Events.EventWatch.HonkEnded += OnHonkEnded;
			unsubscribeActions.Add(() => Events.EventWatch.HonkEnded -= OnHonkEnded);


			//Inventory.Instance.MoneyChanged += OnMoneyChanged;
			//unsubscribeActions.Add(() => Inventory.Instance.MoneyChanged -= OnMoneyChanged);
		}

		// Common hooks
		protected virtual void OnSpeedIncreased(float speed) { }
		protected virtual void OnHonked() { }
		protected virtual void OnHonkStarted() { }
		protected virtual void OnHonkEnded() { }
		protected virtual void OnMoneyChanged(double previousAmount, double currentAmoung) { }
	}
}
