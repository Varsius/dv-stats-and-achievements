using System;
using System.Collections.Generic;
using DV.Logic.Job;
using DV.ThingTypes;
using UnityEngine;

namespace StatsAndAchievements.Trackers;

public abstract class StatTracker
{
	private readonly List<Action> _unsubscribeActions = new();

	public event Action? Changed;

	public abstract string Id { get; }
	public abstract string Title { get; }
	public abstract string Value();

	internal void Attach()
	{
		SubscribeToEvents();
	}

	internal void Detach()
	{
		foreach (Action unsubscribeAction in _unsubscribeActions)
		{
			unsubscribeAction();
		}

		_unsubscribeActions.Clear();
	}

	protected virtual void SubscribeToEvents()
	{
	}

	protected void NotifyChanged()
	{
		Changed?.Invoke();
	}

	protected SavedBool SavedBool(string name, bool defaultValue = false)
	{
		return new SavedBool(name, defaultValue);
	}

	protected SavedFloat SavedFloat(string name, float defaultValue = 0.0f)
	{
		return new SavedFloat(name, defaultValue);
	}

	protected void Subscribe(Action subscribe, Action unsubscribe)
	{
		subscribe();
		_unsubscribeActions.Add(unsubscribe);
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
