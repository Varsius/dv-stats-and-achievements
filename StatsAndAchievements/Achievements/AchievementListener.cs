using DV.JObjectExtstensions;
using System;
using System.Collections.Generic;
using DV.Logic.Job;
using UnityEngine;

namespace StatsAndAchievements.Achievements
{

	public enum AchievementType
	{
		Progress,
		Condition
	}
	public abstract class AchievementListener
	{
		private AchievementManager? _manager;
		public abstract string Id { get; }
		public abstract string Title { get; }
		public abstract string Description { get; }
		public abstract AchievementType Type { get; }

		public virtual string GetDisplayTitle() => Title;
		public virtual string GetDisplayDescription() => Description;

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

			_manager!.NotifyUnlocked(Id);
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

		protected void SubscribeToUnlockedGaragesChanged(Action<int> listener) =>
			Subscribe(
				() => Events.EventWatch.UnlockedGaragesChanged += listener,
				() => Events.EventWatch.UnlockedGaragesChanged -= listener
			);

		protected void SubscribeToLicenseAcquired(Action<string> listener) =>
			Subscribe(
				() => Events.Actions.LicenseAcquired += listener,
				() => Events.Actions.LicenseAcquired -= listener
			);

		protected void SubscribeToCarChanged(Action<TrainCar> listener) =>
			Subscribe(
				() => PlayerManager.CarChanged += listener,
				() => PlayerManager.CarChanged -= listener
			);

		protected void SubscribeToJobCompletion(Action<Job> listener) =>
			Subscribe(
				() => Events.Actions.JobCompletion += listener,
				() => Events.Actions.JobCompletion -= listener
			);

		protected void SubscribeToPaintJobApplied(Action listener) =>
			Subscribe(
				() => Events.Actions.PaintJobApplied += listener,
				() => Events.Actions.PaintJobApplied -= listener
			);

		protected void SubscribeToPlayerPositionChanged(Action<Vector3> listener) =>
			Subscribe(
				() => Events.Actions.PlayerPositionChanged += listener,
				() => Events.Actions.PlayerPositionChanged -= listener
			);

		protected void SubscribeToPitStopCheckout(Action<float, bool> listener) =>
			Subscribe(
				() => LocoResourceModule.LocoResourceBoughtGlobalEvent += listener,
				() => LocoResourceModule.LocoResourceBoughtGlobalEvent -= listener
			);

		// Common hooks
		protected virtual void OnSpeedIncreased(float speed) { }
		protected virtual void OnHonked() { }
		protected virtual void OnHonkStarted() { }
		protected virtual void OnHonkEnded() { }
		protected virtual void OnUnlockedGaragesChanged(int unlockedGaragesCount) { }
		protected virtual void OnMoneyChanged(double previousAmount, double currentAmount) { }
		protected virtual void OnLicenseAcquired(string licenseName) { }
		protected virtual void OnJobCompletion(Job job) { }
		protected virtual void OnPaintJobApplied() { }
		protected virtual void OnPlayerPositionChanged(Vector3 playerPosition) { }
		protected virtual void OnPitStopCheckout(float amount, bool hasPaid) { }
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

	public abstract class SecretConditionAchievementListener : ConditionAchievementListener
	{
		protected virtual string HiddenTitle => "???";
		protected virtual string HiddenDescription => "Details for this achievement will be revealed once unlocked";

		public override string GetDisplayTitle() => IsUnlocked() ? Title : HiddenTitle;
		public override string GetDisplayDescription() => IsUnlocked() ? Description : HiddenDescription;
	}
}
