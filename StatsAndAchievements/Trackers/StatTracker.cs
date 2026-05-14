using System;
using System.Collections.Generic;
using DV.Logic.Job;
using DV.ThingTypes;
using UnityEngine;

namespace StatsAndAchievements.Trackers;

public abstract class StatTracker
{
	public readonly struct DetailEntry(string title, string value)
	{
		public string Title { get; } = title;
		public string Value { get; } = value;
	}

	private readonly List<Action> _unsubscribeActions = new();

	public event Action? Changed;

	public abstract string Id { get; }
	public abstract string Title { get; }
	public abstract string Value();
	public virtual IReadOnlyList<DetailEntry> Details() => [];

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

	protected SavedInt SavedInt(string name, int defaultValue = 0)
	{
		return new SavedInt(name, defaultValue);
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

	protected void SubscribeToBellStarted(Action listener) =>
		Subscribe(
			() => Events.EventWatch.BellStarted += listener,
			() => Events.EventWatch.BellStarted -= listener
		);

	protected void SubscribeToBellEnded(Action listener) =>
		Subscribe(
			() => Events.EventWatch.BellEnded += listener,
			() => Events.EventWatch.BellEnded -= listener
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

	protected void SubscribeToMoneySpent(Action<float, Events.MoneySpentSource> listener) =>
		Subscribe(
			() => Events.Actions.MoneySpent += listener,
			() => Events.Actions.MoneySpent -= listener
		);

	protected void SubscribeToCargoDamageIncurred(Action<float> listener) =>
		Subscribe(
			() => Events.Actions.CargoDamageIncurred += listener,
			() => Events.Actions.CargoDamageIncurred -= listener
		);

	protected void SubscribeToVehicleDamageIncurred(Action<float> listener) =>
		Subscribe(
			() => Events.Actions.VehicleDamageIncurred += listener,
			() => Events.Actions.VehicleDamageIncurred -= listener
		);

	protected void SubscribeToServiceResourceBought(Action<ResourceType, float> listener) =>
		Subscribe(
			() => Events.Actions.ServiceResourceBought += listener,
			() => Events.Actions.ServiceResourceBought -= listener
		);

	protected void SubscribeToPlayerDerailed(Action listener) =>
		Subscribe(
			() => Events.Actions.PlayerDerailed += listener,
			() => Events.Actions.PlayerDerailed -= listener
		);

	protected void SubscribeToTurntableRotated(Action<float> listener) =>
		Subscribe(
			() => Events.Actions.TurntableRotated += listener,
			() => Events.Actions.TurntableRotated -= listener
		);

	protected void SubscribeToPlayerTeleportStarted(Action listener) =>
		Subscribe(
			() => PlayerManager.PlayerTeleportStarted += listener,
			() => PlayerManager.PlayerTeleportStarted -= listener
		);

	protected void SubscribeToPlayerTeleportFinished(Action listener) =>
		Subscribe(
			() => PlayerManager.PlayerTeleportFinished += listener,
			() => PlayerManager.PlayerTeleportFinished -= listener
		);

	protected void SubscribeToFastTravelCompleted(Action<bool> listener) =>
		Subscribe(
			() => Events.Actions.FastTravelCompleted += listener,
			() => Events.Actions.FastTravelCompleted -= listener
		);

	protected void SubscribeToCommsRadioActionPerformed(Action<Events.CommsRadioActionType> listener) =>
		Subscribe(
			() => Events.Actions.CommsRadioActionPerformed += listener,
			() => Events.Actions.CommsRadioActionPerformed -= listener
		);
}
