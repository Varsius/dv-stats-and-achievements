using System;
using System.Collections.Generic;
using DV.Logic.Job;
using StatsAndAchievements.Trackers;
using UnityEngine;
using DV.ThingTypes;

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
		private SavedBool? _unlocked;
		public abstract string Id { get; }
		public abstract string Title { get; }
		public abstract string Description { get; }
		public abstract AchievementType Type { get; }

		public virtual string GetDisplayTitle() => Title;
		public virtual string GetDisplayDescription() => Description;

		public bool IsUnlocked() => Unlocked.Value;

		private SavedBool Unlocked => _unlocked ??= SavedBool("unlocked");

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

			Unlocked.SetTrue();

			NotifyUnlock();
		}

		protected void NotifyUnlock()
		{
			_manager!.NotifyUnlocked(Id);
		}

		protected virtual void SubscribeToEvents() { }

		protected SavedBool SavedBool(string name, bool defaultValue = false)
		{
			return new SavedBool($"{Id}_{name}", defaultValue);
		}

		protected static int CountTrue(params SavedBool[] values)
		{
			int count = 0;
			foreach (var value in values)
			{
				if (value) count++;
			}

			return count;
		}

		protected SavedFloat SavedFloat(string name, float defaultValue = 0.0f)
		{
			return new SavedFloat($"{Id}_{name}", defaultValue);
		}

		protected void Subscribe(Action subscribe, Action unsubscribe)
		{
			subscribe();
			unsubscribeActions.Add(unsubscribe);
		}

		protected void SubscribeToTrackerChanged(StatTracker tracker, Action listener) =>
			Subscribe(
				() => tracker.Changed += listener,
				() => tracker.Changed -= listener
			);

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

		protected void SubscribeToGeneralLicenseAcquired(Action<GeneralLicenseType_v2> listener) =>
			Subscribe(
				() => Events.Actions.GeneralLicenseAcquired += listener,
				() => Events.Actions.GeneralLicenseAcquired -= listener
			);

		protected void SubscribeToJobLicenseAcquired(Action<JobLicenseType_v2> listener) =>
			Subscribe(
				() => Events.Actions.JobLicenseAcquired += listener,
				() => Events.Actions.JobLicenseAcquired -= listener
			);

		protected void SubscribeToCarChanged(Action<TrainCar?> listener) =>
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

		protected void SubscribeToItemAcquired(Action<string> listener) =>
			Subscribe(
				() => Events.Actions.ItemAcquired += listener,
				() => Events.Actions.ItemAcquired -= listener
			);

		protected void SubscribeToTurntableRotated(Action<float> listener) =>
			Subscribe(
				() => Events.Actions.TurntableRotated += listener,
				() => Events.Actions.TurntableRotated -= listener
			);
	}

	public abstract class ProgressAchievementListener : AchievementListener
	{
		public abstract string Value();
		public abstract string Target();
		public virtual string Progress()
		{
			return $"{Value()}/{Target()}";
		}
		public override AchievementType Type => AchievementType.Progress;
	}

	public abstract class MilestoneAchievementListener : ProgressAchievementListener
	{
		protected abstract string BaseTitle { get; }
		protected abstract string MaxRankDescription { get; }
		protected abstract float[] Milestones { get; }
		protected abstract float CurrentValue { get; }

		public override string Title => $"{BaseTitle} {ToRoman(DisplayRank())}";
		public override string Description => CurrentRank() >= Milestones.Length
			? MaxRankDescription
			: GetMilestoneDescription(CurrentTarget());
		public override string Target() => FormatValue(CurrentTarget());
		public override string Progress() => $"{FormatValue(GetProgressValue())}/{FormatValue(CurrentTarget())}";

		protected abstract string GetMilestoneDescription(float target);
		protected abstract string FormatValue(float value);

		protected int CurrentRank()
		{
			return CalculateRank(CurrentValue);
		}

		protected int DisplayRank()
		{
			int currentRank = CurrentRank();
			if (currentRank >= Milestones.Length)
			{
				return Milestones.Length;
			}

			return currentRank + 1;
		}

		protected int CalculateRank(float value)
		{
			int rank = 0;

			foreach (float threshold in Milestones)
			{
				if (value >= threshold)
				{
					rank++;
				}
			}

			return rank;
		}

		protected float CurrentTarget()
		{
			int rank = CurrentRank();
			if (rank >= Milestones.Length)
			{
				return Milestones[Milestones.Length - 1];
			}

			return Milestones[rank];
		}

		protected float GetProgressValue()
		{
			return Mathf.Min(CurrentValue, CurrentTarget());
		}

		protected void NotifyMilestoneProgress(float previousValue, float currentValue)
		{
			int previousRank = CalculateRank(previousValue);
			int newRank = CalculateRank(currentValue);
			if (newRank <= previousRank)
			{
				return;
			}

			if (previousRank == 0)
			{
				TriggerUnlock();
				return;
			}

			NotifyUnlock();
		}

		protected static string ToRoman(int number)
		{
			return number switch
			{
				1 => "I",
				2 => "II",
				3 => "III",
				4 => "IV",
				5 => "V",
				_ => number.ToString()
			};
		}
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
