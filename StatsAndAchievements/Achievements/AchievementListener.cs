using DV.InventorySystem;
using DV.JObjectExtstensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DV.Logic.Job;

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

		public bool IsUnlocked()
		{
			bool? unlocked = Main.saaSaveData.GetBool($"{Id}_unlocked");
			return unlocked.HasValue && unlocked.Value;
		}

		private readonly List<Action> unsubscribeActions = new List<Action>();

		internal void Attach(AchievementManager manager)
		{
			_manager = manager;

			SubscribeToEvents();
		}

		internal void Detach()
		{
			foreach (var unsubscribeAction in unsubscribeActions)
				unsubscribeAction();

			unsubscribeActions.Clear();
		}

		protected void TriggerUnlock()
		{
			if (IsUnlocked()) return;

			Main.saaSaveData.SetBool($"{Id}_unlocked", true);

			_manager.NotifyUnlocked(Id);
		}

		protected virtual void SubscribeToEvents()
		{
		}

		protected void Subscribe(Action subscribe, Action unsubscribe)
		{
			subscribe();
			unsubscribeActions.Add(unsubscribe);
		}

		protected void SubscribeToSpeedIncreased(Action<float> listener) =>
			Subscribe(
				() => Events.EventWatch.SpeedIncreased += listener,
				() => Events.EventWatch.SpeedIncreased -= listener
			);

		protected void SubscribeToHonked(Action listener) =>
			Subscribe(
				() => Events.EventWatch.Honked += listener,
				() => Events.EventWatch.Honked -= listener
			);

		protected void SubscribeToHonkStarted(Action listener) =>
			Subscribe(
				() => Events.EventWatch.HonkStarted += listener,
				() => Events.EventWatch.HonkStarted -= listener
			);

		protected void SubscribeToHonkEnded(Action listener) =>
			Subscribe(
				() => Events.EventWatch.HonkEnded += listener,
				() => Events.EventWatch.HonkEnded -= listener
			);

		protected void SubscribeToLicenseAcquired(Action<string> listener) =>
			Subscribe(
				() => Events.Actions.LicenseAcquired += listener,
				() => Events.Actions.LicenseAcquired -= listener
			);

		protected void SubscribeToJobCompletion(Action<Job> listener) =>
			Subscribe(
				() => Events.Actions.JobCompletion += listener,
				() => Events.Actions.JobCompletion -= listener
			);

		// Common hooks
		protected virtual void OnSpeedIncreased(float speed) { }
		protected virtual void OnHonked() { }
		protected virtual void OnHonkStarted() { }
		protected virtual void OnHonkEnded() { }
		protected virtual void OnMoneyChanged(double previousAmount, double currentAmount) { }
		protected virtual void OnLicenseAcquired(string licenseName) { }
		protected virtual void OnJobCompletion(Job job) { }
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
