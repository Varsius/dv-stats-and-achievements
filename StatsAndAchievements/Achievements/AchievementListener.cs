using DV.JObjectExtstensions;
using System;
using System.Collections.Generic;
using DV.Logic.Job;
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

			_manager!.NotifyUnlocked(Id);
		}

		protected virtual void SubscribeToEvents() { }

		protected SavedBool SavedBool(string name, bool defaultValue = false)
		{
			return new SavedBool($"{Id}_{name}", defaultValue);
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

		protected void SubscribeToItemAcquired(Action<string> listener) =>
			Subscribe(
				() => Events.Actions.ItemAcquired += listener,
				() => Events.Actions.ItemAcquired -= listener
			);
	}

	public sealed class SavedBool
	{
		private readonly string _key;
		private readonly bool _defaultValue;
		private bool _hasLoaded;
		private bool _value;

		public SavedBool(string key, bool defaultValue = false)
		{
			_key = key;
			_defaultValue = defaultValue;
		}

		public bool Value
		{
			get
			{
				EnsureLoaded();
				return _hasLoaded ? _value : _defaultValue;
			}
			set
			{
				_value = value;
				_hasLoaded = true;
				Main.saaSaveData.SetBool(_key, value);
			}
		}

		public void SetTrue()
		{
			Value = true;
		}

		public static implicit operator bool(SavedBool savedBool) => savedBool.Value;

		private void EnsureLoaded()
		{
			if (_hasLoaded || !Main.hasLoadedSaaSaveData)
			{
				return;
			}

			_value = Main.saaSaveData.GetBool(_key) ?? _defaultValue;
			_hasLoaded = true;
		}
	}

	public sealed class SavedFloat
	{
		private readonly string _key;
		private readonly float _defaultValue;
		private bool _hasLoaded;
		private float _value;

		public SavedFloat(string key, float defaultValue = 0.0f)
		{
			_key = key;
			_defaultValue = defaultValue;
		}

		public float Value
		{
			get
			{
				EnsureLoaded();
				return _hasLoaded ? _value : _defaultValue;
			}
			set
			{
				_value = value;
				_hasLoaded = true;
				Main.saaSaveData.SetFloat(_key, value);
			}
		}

		public static implicit operator float(SavedFloat savedFloat) => savedFloat.Value;

		private void EnsureLoaded()
		{
			if (_hasLoaded || !Main.hasLoadedSaaSaveData)
			{
				return;
			}

			_value = Main.saaSaveData.GetFloat(_key) ?? _defaultValue;
			_hasLoaded = true;
		}
	}

	public abstract class ProgressAchievementListener : AchievementListener
	{
		public abstract string Value();
		public abstract string ValueName();
		public abstract string Target();
		public virtual string Progress()
		{
			return $"{Value()}/{Target()}";
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
