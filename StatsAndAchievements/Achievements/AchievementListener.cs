using DV.InventorySystem;
using DV.JObjectExtstensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsAndAchievements.Achievements
{

	public enum AchievementType
	{
		Progress,
		Condition
	}
	public abstract class AchievementListener
	{
		private AchievementManager _manager;
		public abstract string Id { get; }
		public abstract string Title { get; }
		public abstract string Description { get; }
		public abstract AchievementType Type { get; }

		// TODO: look this up in save data
		public bool IsUnlocked() {
			bool? unlocked = Main.saaSaveData.GetBool($"{Id}_unlocked");
			return unlocked.HasValue && unlocked.Value;
		}

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

			Events.Actions.LicenseAcquired += OnLicenseAcquired;
			unsubscribeActions.Add(() => Events.Actions.LicenseAcquired -= OnLicenseAcquired);


			//Inventory.Instance.MoneyChanged += OnMoneyChanged;
			//unsubscribeActions.Add(() => Inventory.Instance.MoneyChanged -= OnMoneyChanged);
		}

		// Common hooks
		protected virtual void OnSpeedIncreased(float speed) { }
		protected virtual void OnHonked() { }
		protected virtual void OnHonkStarted() { }
		protected virtual void OnHonkEnded() { }
		protected virtual void OnMoneyChanged(double previousAmount, double currentAmount) { }
		protected virtual void OnLicenseAcquired(string licenseName) { }
	}

	public abstract class ProgressAchievementListener : AchievementListener
	{
		public abstract string Value();
		public abstract string ValueName();
		public abstract string Target();
		public virtual string Progress()
		{
			return $"{this.Value()}/{this.Target()}";
		}
		public override AchievementType Type => AchievementType.Progress;
	}

	public abstract class ConditionAchievementListener : AchievementListener
	{
		public override AchievementType Type => AchievementType.Condition;
	}
}
