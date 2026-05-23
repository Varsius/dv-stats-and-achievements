using System;
using System.Collections.Generic;
using System.Linq;
using DV.Logic.Job;
using DV.Simulation.Cars;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using UnityEngine;

namespace StatsAndAchievements.Trackers;

public sealed class SpeedStatTracker : StatTracker
{
	private readonly SavedFloat _maxSpeed;

	public override string Id => "highest_speed";
	public override string Title => "Highest Speed";

	public SpeedStatTracker()
	{
		_maxSpeed = SavedFloat("highest_speed");
	}

	public override string Value() => $"{(int)_maxSpeed.Value} km/h";
	public float CurrentSpeed => _maxSpeed.Value;

	protected override void SubscribeToEvents()
	{
		SubscribeToSpeedIncreased(OnSpeedIncreased);
	}

	private void OnSpeedIncreased(float speed)
	{
		if (speed <= _maxSpeed.Value)
		{
			return;
		}

		_maxSpeed.Value = speed;
		NotifyChanged();
	}
}

public sealed class LicenseProgressStatTracker : StatTracker
{
	public override string Id => "licenses_acquired";
	public override string Title => "Licenses Acquired";

	public override string Value() => $"{AcquiredLicensesCount()}";
	public int CurrentCount => AcquiredLicensesCount();

	protected override void SubscribeToEvents()
	{
		SubscribeToGeneralLicenseAcquired(_ => NotifyChanged());
		SubscribeToJobLicenseAcquired(_ => NotifyChanged());
	}

	private static int AcquiredLicensesCount()
	{
		LicenseManager licenseManager = SingletonBehaviour<LicenseManager>.Instance;
		return licenseManager.GetNumberOfAcquiredGeneralLicenses() + licenseManager.GetNumberOfAcquiredJobLicenses();
	}
}

public sealed class SteamEngineRequirementsStatTracker : StatTracker
{
	private readonly SavedBool _hasAcquiredSteamLicense;
	private readonly SavedBool _hasAcquiredShovel;
	private readonly SavedBool _hasAcquiredLighter;
	private readonly SavedBool _hasAcquiredOil;

	public override string Id => "steamer_items_collected";
	public override string Title => "Steamer Items Collected";
	public override bool ShowInCareerManagerScreen => false;

	public SteamEngineRequirementsStatTracker()
	{
		_hasAcquiredSteamLicense = SavedBool("steamer_license_acquired");
		_hasAcquiredShovel = SavedBool("steamer_shovel_acquired");
		_hasAcquiredLighter = SavedBool("steamer_lighter_acquired");
		_hasAcquiredOil = SavedBool("steamer_oiler_acquired");
	}

	public override string Value() => $"{CurrentCount}";
	public int CurrentCount => CountTrue(_hasAcquiredSteamLicense, _hasAcquiredShovel, _hasAcquiredLighter, _hasAcquiredOil);
	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Steam License", BoolState(_hasAcquiredSteamLicense.Value)),
		new DetailEntry("Shovel", BoolState(_hasAcquiredShovel.Value)),
		new DetailEntry("Lighter", BoolState(_hasAcquiredLighter.Value)),
		new DetailEntry("Oiler", BoolState(_hasAcquiredOil.Value))
	];

	protected override void SubscribeToEvents()
	{
		SubscribeToGeneralLicenseAcquired(OnGeneralLicenseAcquired);
		SubscribeToItemAcquired(OnItemAcquired);
	}

	private void OnGeneralLicenseAcquired(GeneralLicenseType_v2 license)
	{
		var steamer = GeneralLicenseType.S060 | GeneralLicenseType.SH282;
		bool isSteamer = (license.v1 & steamer) != 0;
		if (!isSteamer || _hasAcquiredSteamLicense.Value)
		{
			return;
		}

		_hasAcquiredSteamLicense.SetTrue();
		NotifyChanged();
	}

	private void OnItemAcquired(string itemName)
	{
		bool changed = false;
		string normalizedName = itemName.ToLower();

		if (normalizedName.Contains("shovel") && !_hasAcquiredShovel.Value)
		{
			_hasAcquiredShovel.SetTrue();
			changed = true;
		}

		if (normalizedName.Contains("lighter") && !_hasAcquiredLighter.Value)
		{
			_hasAcquiredLighter.SetTrue();
			changed = true;
		}

		if (normalizedName.Contains("oiler") && !_hasAcquiredOil.Value)
		{
			_hasAcquiredOil.SetTrue();
			changed = true;
		}

		if (changed)
		{
			NotifyChanged();
		}
	}

	private static int CountTrue(params SavedBool[] values)
	{
		int count = 0;
		foreach (SavedBool value in values)
		{
			if (value)
			{
				count++;
			}
		}

		return count;
	}

	private static string BoolState(bool value)
	{
		return value ? "Acquired" : "Missing";
	}
}

public sealed class MoneyEarnedStatTracker : StatTracker
{
	private readonly SavedFloat _moneyEarned;

	public override string Id => "money_earned";
	public override string Title => "Money Earned";

	public MoneyEarnedStatTracker()
	{
		_moneyEarned = SavedFloat("career_money_earned");
	}

	public override string Value() => $"${(int)_moneyEarned.Value}";
	public float CurrentValue => _moneyEarned.Value;

	protected override void SubscribeToEvents()
	{
		SubscribeToJobCompletion(OnJobCompletion);
	}

	private void OnJobCompletion(Job job)
	{
		_moneyEarned.Value += job.GetWageForTheJob();
		NotifyChanged();
	}
}

public sealed class TeleportsStatTracker : StatTracker
{
	private readonly SavedInt _teleports;

	public override string Id => "teleports";
	public override string Title => "Teleports";

	public TeleportsStatTracker()
	{
		_teleports = SavedInt("teleports");
	}

	public override string Value() => $"{CurrentCount}";
	public int CurrentCount => _teleports.Value;

	protected override void SubscribeToEvents()
	{
		SubscribeToPlayerTeleportFinished(OnPlayerTeleportFinished);
	}

	private void OnPlayerTeleportFinished()
	{
		if (FastTravelController.IsFastTravelling)
		{
			return;
		}

		_teleports.Value++;
		NotifyChanged();
	}
}

public sealed class FastTravelsStatTracker : StatTracker
{
	private readonly SavedInt _fastTravelsWithoutLoco;
	private readonly SavedInt _fastTravelsWithLoco;

	public override string Id => "fast_travels";
	public override string Title => "Fast Travels";

	public FastTravelsStatTracker()
	{
		_fastTravelsWithoutLoco = SavedInt("fast_travels_without_loco");
		_fastTravelsWithLoco = SavedInt("fast_travels_with_loco");
	}

	public override string Value() => $"{CurrentCount}";
	public int CurrentCount => WithoutLocoCount + WithLocoCount;
	public int WithoutLocoCount => _fastTravelsWithoutLoco.Value;
	public int WithLocoCount => _fastTravelsWithLoco.Value;
	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Total", $"{CurrentCount}"),
		new DetailEntry("Without Loco", $"{WithoutLocoCount}"),
		new DetailEntry("With Loco", $"{WithLocoCount}")
	];

	protected override void SubscribeToEvents()
	{
		SubscribeToFastTravelCompleted(OnFastTravelCompleted);
	}

	private void OnFastTravelCompleted(bool withLoco)
	{
		if (withLoco)
		{
			_fastTravelsWithLoco.Value++;
		}
		else
		{
			_fastTravelsWithoutLoco.Value++;
		}

		NotifyChanged();
	}
}

public sealed class DistanceTravelledStatTracker : StatTracker
{
	private const float MaxWalkingDistancePerSample = 20f;

	private readonly SavedFloat _walkingDistanceTravelled;
	private readonly SavedFloat _teleportDistanceTravelled;
	private Transform? _walkingReferenceParent;
	private Vector3 _lastWalkingPosition;
	private bool _hasLastWalkingPosition;
	private Vector3 _teleportStartPosition;
	private bool _hasTeleportStartPosition;

	public override string Id => "distance_travelled";
	public override string Title => "Distance Travelled";

	public DistanceTravelledStatTracker()
	{
		_walkingDistanceTravelled = SavedFloat("distance_travelled_walking");
		_teleportDistanceTravelled = SavedFloat("distance_travelled_teleport");
	}

	public override string Value() => FormatDistance(TotalDistanceTravelled);
	public float TotalDistanceTravelled => WalkingDistanceTravelled + TeleportDistanceTravelled;
	public float WalkingDistanceTravelled => _walkingDistanceTravelled.Value;
	public float TeleportDistanceTravelled => _teleportDistanceTravelled.Value;
	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Total", FormatDistance(TotalDistanceTravelled)),
		new DetailEntry("Walking", FormatDistance(WalkingDistanceTravelled)),
		new DetailEntry("Teleport", FormatDistance(TeleportDistanceTravelled))
	];

	protected override void SubscribeToEvents()
	{
		SubscribeToPlayerPositionChanged(OnPlayerPositionChanged);
		SubscribeToPlayerTeleportStarted(OnPlayerTeleportStarted);
		SubscribeToPlayerTeleportFinished(OnPlayerTeleportFinished);
	}

	private void OnPlayerPositionChanged(Vector3 _)
	{
		if (FastTravelController.IsFastTravelling || _hasTeleportStartPosition)
		{
			_hasLastWalkingPosition = false;
			_walkingReferenceParent = null;
			return;
		}

		Transform? playerTransform = PlayerManager.PlayerTransform;
		if (playerTransform == null)
		{
			_hasLastWalkingPosition = false;
			_walkingReferenceParent = null;
			return;
		}

		Transform? currentParent = playerTransform.parent;
		Vector3 currentPosition = currentParent != null ? playerTransform.localPosition : playerTransform.position;
		if (_walkingReferenceParent != currentParent)
		{
			_walkingReferenceParent = currentParent;
			_lastWalkingPosition = currentPosition;
			_hasLastWalkingPosition = true;
			return;
		}

		if (!_hasLastWalkingPosition)
		{
			_lastWalkingPosition = currentPosition;
			_hasLastWalkingPosition = true;
			return;
		}

		float walkingDistance = Vector3.Distance(_lastWalkingPosition, currentPosition);
		_lastWalkingPosition = currentPosition;
		if (walkingDistance <= 0f || walkingDistance > MaxWalkingDistancePerSample)
		{
			return;
		}

		_walkingDistanceTravelled.Value += walkingDistance;
		NotifyChanged();
	}

	private void OnPlayerTeleportStarted()
	{
		_hasLastWalkingPosition = false;
		_walkingReferenceParent = null;
		if (FastTravelController.IsFastTravelling)
		{
			_hasTeleportStartPosition = false;
			return;
		}

		Transform? playerTransform = PlayerManager.PlayerTransform;
		if (playerTransform == null)
		{
			_hasTeleportStartPosition = false;
			return;
		}

		_teleportStartPosition = playerTransform.position;
		_hasTeleportStartPosition = true;
	}

	private void OnPlayerTeleportFinished()
	{
		_hasLastWalkingPosition = false;
		_walkingReferenceParent = null;
		if (FastTravelController.IsFastTravelling)
		{
			_hasTeleportStartPosition = false;
			return;
		}

		if (!_hasTeleportStartPosition)
		{
			return;
		}

		Transform? playerTransform = PlayerManager.PlayerTransform;
		if (playerTransform == null)
		{
			_hasTeleportStartPosition = false;
			return;
		}

		float teleportDistance = Vector3.Distance(_teleportStartPosition, playerTransform.position);
		_hasTeleportStartPosition = false;
		if (teleportDistance <= 0f)
		{
			return;
		}

		_teleportDistanceTravelled.Value += teleportDistance;
		NotifyChanged();
	}

	private static string FormatDistance(float distanceInMeters)
	{
		if (distanceInMeters < 1000f)
		{
			return $"{(int)distanceInMeters} m";
		}

		return $"{distanceInMeters / 1000f:0.0} km";
	}
}

public sealed class MoneySpentStatTracker : StatTracker
{
	private readonly SavedFloat _itemShopMoneySpent;
	private readonly SavedFloat _manualServiceMoneySpent;
	private readonly SavedFloat _careerManagerFeesMoneySpent;
	private readonly SavedFloat _careerManagerLicensesMoneySpent;
	private readonly SavedFloat _fastTravelMoneySpent;
	private readonly SavedFloat _commsRadioRerailMoneySpent;
	private readonly SavedFloat _commsRadioCarRemovalMoneySpent;
	private readonly SavedFloat _commsRadioUtilityVehicleSummonMoneySpent;

	public override string Id => "money_spent";
	public override string Title => "Money Spent";

	public MoneySpentStatTracker()
	{
		_itemShopMoneySpent = SavedFloat("money_spent_item_shop");
		_manualServiceMoneySpent = SavedFloat("money_spent_manual_service");
		_careerManagerFeesMoneySpent = SavedFloat("money_spent_career_manager_fees");
		_careerManagerLicensesMoneySpent = SavedFloat("money_spent_career_manager_licenses");
		_fastTravelMoneySpent = SavedFloat("money_spent_fast_travel");
		_commsRadioRerailMoneySpent = SavedFloat("money_spent_comms_radio_rerail");
		_commsRadioCarRemovalMoneySpent = SavedFloat("money_spent_comms_radio_car_removal");
		_commsRadioUtilityVehicleSummonMoneySpent = SavedFloat("money_spent_comms_radio_utility_vehicle_summon");
	}

	public override string Value() => $"${(int)TotalValue}";
	public float TotalValue => ItemShopValue + ManualServiceValue + CareerManagerFeesValue + CareerManagerLicensesValue + FastTravelValue + CommsRadioRerailValue + CommsRadioCarRemovalValue + CommsRadioUtilityVehicleSummonValue;
	public float ItemShopValue => _itemShopMoneySpent.Value;
	public float ManualServiceValue => _manualServiceMoneySpent.Value;
	public float CareerManagerFeesValue => _careerManagerFeesMoneySpent.Value;
	public float CareerManagerLicensesValue => _careerManagerLicensesMoneySpent.Value;
	public float FastTravelValue => _fastTravelMoneySpent.Value;
	public float CommsRadioRerailValue => _commsRadioRerailMoneySpent.Value;
	public float CommsRadioCarRemovalValue => _commsRadioCarRemovalMoneySpent.Value;
	public float CommsRadioUtilityVehicleSummonValue => _commsRadioUtilityVehicleSummonMoneySpent.Value;

	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Total", FormatMoney(TotalValue)),
		new DetailEntry("Item Shop", FormatMoney(ItemShopValue)),
		new DetailEntry("Manual Service", FormatMoney(ManualServiceValue)),
		new DetailEntry("Fees", FormatMoney(CareerManagerFeesValue)),
		new DetailEntry("Licenses", FormatMoney(CareerManagerLicensesValue)),
		new DetailEntry("Fast Travel", FormatMoney(FastTravelValue)),
		new DetailEntry("Rerail", FormatMoney(CommsRadioRerailValue)),
		new DetailEntry("Car Removal", FormatMoney(CommsRadioCarRemovalValue)),
		new DetailEntry("Utility Vehicle Summon", FormatMoney(CommsRadioUtilityVehicleSummonValue))
	];

	protected override void SubscribeToEvents()
	{
		SubscribeToMoneySpent(OnMoneySpent);
		SubscribeToPitStopCheckout(OnManualServiceCheckout);
	}

	private void OnMoneySpent(float amount, Events.MoneySpentSource source)
	{
		if (amount <= 0f)
		{
			return;
		}

		switch (source)
		{
			case Events.MoneySpentSource.ItemShop:
				_itemShopMoneySpent.Value += amount;
				break;
			case Events.MoneySpentSource.CareerManagerFees:
				_careerManagerFeesMoneySpent.Value += amount;
				break;
			case Events.MoneySpentSource.CareerManagerLicense:
				_careerManagerLicensesMoneySpent.Value += amount;
				break;
			case Events.MoneySpentSource.FastTravel:
				_fastTravelMoneySpent.Value += amount;
				break;
			case Events.MoneySpentSource.CommsRadioRerail:
				_commsRadioRerailMoneySpent.Value += amount;
				break;
			case Events.MoneySpentSource.CommsRadioCarRemoval:
				_commsRadioCarRemovalMoneySpent.Value += amount;
				break;
			case Events.MoneySpentSource.CommsRadioUtilityVehicleSummon:
				_commsRadioUtilityVehicleSummonMoneySpent.Value += amount;
				break;
			default:
				return;
		}

		NotifyChanged();
	}

	private void OnManualServiceCheckout(float amount, bool hasPaid)
	{
		if (!hasPaid || amount <= 0f)
		{
			return;
		}

		_manualServiceMoneySpent.Value += amount;
		NotifyChanged();
	}

	private static string FormatMoney(float amount)
	{
		return $"${(int)amount}";
	}
}

public sealed class DamageCausedStatTracker : StatTracker
{
	private readonly SavedFloat _cargoDamageTotal;
	private readonly SavedFloat _vehicleDamageTotal;

	public override string Id => "damage_caused";
	public override string Title => "Damage Caused";

	public DamageCausedStatTracker()
	{
		_cargoDamageTotal = SavedFloat("cargo_damage_total");
		_vehicleDamageTotal = SavedFloat("vehicle_damage_total");
	}

	public override string Value() => FormatMoney(TotalValue);
	public float TotalValue => CargoDamageValue + VehicleDamageValue;
	public float CargoDamageValue => _cargoDamageTotal.Value;
	public float VehicleDamageValue => _vehicleDamageTotal.Value;

	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Total", FormatMoney(TotalValue)),
		new DetailEntry("Cargo Damage", FormatMoney(CargoDamageValue)),
		new DetailEntry("Vehicle Damage", FormatMoney(VehicleDamageValue))
	];

	protected override void SubscribeToEvents()
	{
		SubscribeToCargoDamageIncurred(OnCargoDamageIncurred);
		SubscribeToVehicleDamageIncurred(OnVehicleDamageIncurred);
	}

	private void OnCargoDamageIncurred(float amount)
	{
		if (amount <= 0f)
		{
			return;
		}

		_cargoDamageTotal.Value += amount;
		NotifyChanged();
	}

	private void OnVehicleDamageIncurred(float amount)
	{
		if (amount <= 0f)
		{
			return;
		}

		_vehicleDamageTotal.Value += amount;
		NotifyChanged();
	}

	private static string FormatMoney(float amount)
	{
		return $"${(int)amount}";
	}
}

public sealed class ServiceResourcesStatTracker : StatTracker
{
	private readonly SavedFloat _fuelAmount;
	private readonly SavedFloat _sandAmount;
	private readonly SavedFloat _oilAmount;
	private readonly SavedFloat _waterAmount;
	private readonly SavedFloat _coalAmount;
	private readonly SavedFloat _electricChargeAmount;

	public override string Id => "service_resources";
	public override string Title => "Service Resources";

	public ServiceResourcesStatTracker()
	{
		_fuelAmount = SavedFloat("service_resource_fuel_amount");
		_sandAmount = SavedFloat("service_resource_sand_amount");
		_oilAmount = SavedFloat("service_resource_oil_amount");
		_waterAmount = SavedFloat("service_resource_water_amount");
		_coalAmount = SavedFloat("service_resource_coal_amount");
		_electricChargeAmount = SavedFloat("service_resource_electric_charge_amount");
	}

	public override string Value() => string.Empty;
	public float FuelAmount => _fuelAmount.Value;
	public float SandAmount => _sandAmount.Value;
	public float OilAmount => _oilAmount.Value;
	public float WaterAmount => _waterAmount.Value;
	public float CoalAmount => _coalAmount.Value;
	public float ElectricChargeAmount => _electricChargeAmount.Value;

	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Fuel", FormatAmount(FuelAmount, "L")),
		new DetailEntry("Sand", FormatAmount(SandAmount, "kg")),
		new DetailEntry("Oil", FormatAmount(OilAmount, "L")),
		new DetailEntry("Water", FormatAmount(WaterAmount, "L")),
		new DetailEntry("Coal", FormatAmount(CoalAmount, "kg")),
		new DetailEntry("Electric Charge", FormatAmount(ElectricChargeAmount, "kWh"))
	];

	protected override void SubscribeToEvents()
	{
		SubscribeToServiceResourceBought(OnServiceResourceBought);
	}

	private void OnServiceResourceBought(ResourceType resourceType, float amount)
	{
		if (amount <= 0f)
		{
			return;
		}

		switch (resourceType)
		{
			case ResourceType.Fuel:
				_fuelAmount.Value += amount;
				break;
			case ResourceType.Sand:
				_sandAmount.Value += amount;
				break;
			case ResourceType.Oil:
				_oilAmount.Value += amount;
				break;
			case ResourceType.Water:
				_waterAmount.Value += amount;
				break;
			case ResourceType.Coal:
				_coalAmount.Value += amount;
				break;
			case ResourceType.ElectricCharge:
				_electricChargeAmount.Value += amount;
				break;
			default:
				return;
		}

		NotifyChanged();
	}

	private static string FormatAmount(float amount, string unit)
	{
		string formattedAmount = amount % 1f == 0f ? $"{(int)amount}" : $"{amount:0.0}";
		return $"{formattedAmount} {unit}";
	}
}

public sealed class DerailmentsStatTracker : StatTracker
{
	private readonly SavedInt _derailmentCount;

	public override string Id => "derailments";
	public override string Title => "Derailments";

	public DerailmentsStatTracker()
	{
		_derailmentCount = SavedInt("derailment_count");
	}

	public override string Value() => $"{CurrentCount}";
	public int CurrentCount => _derailmentCount.Value;

	protected override void SubscribeToEvents()
	{
		SubscribeToPlayerDerailed(OnPlayerDerailed);
	}

	private void OnPlayerDerailed()
	{
		_derailmentCount.Value++;
		NotifyChanged();
	}
}

public sealed class CommsRadioOperationsStatTracker : StatTracker
{
	private readonly SavedInt _rerailCount;
	private readonly SavedInt _carRemovalCount;
	private readonly SavedInt _utilityVehicleSummonCount;

	public override string Id => "comms_radio_operations";
	public override string Title => "Comms Radio Operations";

	public CommsRadioOperationsStatTracker()
	{
		_rerailCount = SavedInt("comms_radio_operation_rerail_count");
		_carRemovalCount = SavedInt("comms_radio_operation_car_removal_count");
		_utilityVehicleSummonCount = SavedInt("comms_radio_operation_utility_vehicle_summon_count");
	}

	public override string Value() => $"{TotalCount}";
	public int TotalCount => RerailCount + CarRemovalCount + UtilityVehicleSummonCount;
	public int RerailCount => _rerailCount.Value;
	public int CarRemovalCount => _carRemovalCount.Value;
	public int UtilityVehicleSummonCount => _utilityVehicleSummonCount.Value;

	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Total", $"{TotalCount}"),
		new DetailEntry("Rerail", $"{RerailCount}"),
		new DetailEntry("Car Removal", $"{CarRemovalCount}"),
		new DetailEntry("Utility Vehicle Summon", $"{UtilityVehicleSummonCount}")
	];

	protected override void SubscribeToEvents()
	{
		SubscribeToCommsRadioActionPerformed(OnCommsRadioActionPerformed);
	}

	private void OnCommsRadioActionPerformed(Events.CommsRadioActionType actionType)
	{
		switch (actionType)
		{
			case Events.CommsRadioActionType.Rerail:
				_rerailCount.Value++;
				break;
			case Events.CommsRadioActionType.CarRemoval:
				_carRemovalCount.Value++;
				break;
			case Events.CommsRadioActionType.UtilityVehicleSummon:
				_utilityVehicleSummonCount.Value++;
				break;
			default:
				return;
		}

		NotifyChanged();
	}
}

public sealed class HornAndBellTimeStatTracker : StatTracker
{
	private readonly SavedFloat _hornTimeSeconds;
	private readonly SavedFloat _bellTimeSeconds;
	private float _hornStartedAt = -1f;
	private float _bellStartedAt = -1f;
	private float _currentHornSessionCommittedSeconds;
	private float _currentBellSessionCommittedSeconds;

	public override string Id => "horn_bell_time";
	public override string Title => "Horn / Bell Time";

	public HornAndBellTimeStatTracker()
	{
		_hornTimeSeconds = SavedFloat("horn_time_seconds");
		_bellTimeSeconds = SavedFloat("bell_time_seconds");
	}

	public override string Value() => FormatDuration(TotalTimeSeconds);
	public float TotalTimeSeconds => HornTimeSeconds + BellTimeSeconds;
	public float HornTimeSeconds => _hornTimeSeconds.Value + CurrentActiveElapsedSeconds(_hornStartedAt);
	public float BellTimeSeconds => _bellTimeSeconds.Value + CurrentActiveElapsedSeconds(_bellStartedAt);
	public float CurrentHornSessionDuration => CurrentSessionDuration(_hornStartedAt, _currentHornSessionCommittedSeconds);
	public float CurrentBellSessionDuration => CurrentSessionDuration(_bellStartedAt, _currentBellSessionCommittedSeconds);

	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Total", FormatDuration(TotalTimeSeconds)),
		new DetailEntry("Horn", FormatDuration(HornTimeSeconds)),
		new DetailEntry("Bell", FormatDuration(BellTimeSeconds))
	];

	protected override void SubscribeToEvents()
	{
		SubscribeToHonkStarted(OnHonkStarted);
		SubscribeToHonkEnded(OnHonkEnded);
		SubscribeToBellStarted(OnBellStarted);
		SubscribeToBellEnded(OnBellEnded);
		// TODO: think about replacing with the event watch events
		SubscribeToPlayerPositionChanged(OnHeartbeat);
	}

	private void OnHonkStarted()
	{
		if (_hornStartedAt >= 0f)
		{
			return;
		}

		_hornStartedAt = Time.time;
		_currentHornSessionCommittedSeconds = 0f;
		NotifyChanged();
	}

	private void OnHonkEnded()
	{
		FlushHornTime(keepActive: false);
		_currentHornSessionCommittedSeconds = 0f;
		NotifyChanged();
	}

	private void OnBellStarted()
	{
		if (_bellStartedAt >= 0f)
		{
			return;
		}

		_bellStartedAt = Time.time;
		_currentBellSessionCommittedSeconds = 0f;
		NotifyChanged();
	}

	private void OnBellEnded()
	{
		FlushBellTime(keepActive: false);
		_currentBellSessionCommittedSeconds = 0f;
		NotifyChanged();
	}

	private void OnHeartbeat(Vector3 _)
	{
		bool changed = FlushHornTime(keepActive: true) | FlushBellTime(keepActive: true);
		if (changed)
		{
			NotifyChanged();
		}
	}

	private bool FlushHornTime(bool keepActive)
	{
		return FlushActiveTime(_hornTimeSeconds, ref _hornStartedAt, ref _currentHornSessionCommittedSeconds, keepActive);
	}

	private bool FlushBellTime(bool keepActive)
	{
		return FlushActiveTime(_bellTimeSeconds, ref _bellStartedAt, ref _currentBellSessionCommittedSeconds, keepActive);
	}

	private static bool FlushActiveTime(SavedFloat totalTime, ref float startedAt, ref float committedSessionTime, bool keepActive)
	{
		if (startedAt < 0f)
		{
			return false;
		}

		float elapsed = Time.time - startedAt;
		if (elapsed > 0f)
		{
			totalTime.Value += elapsed;
			committedSessionTime += elapsed;
		}

		startedAt = keepActive ? Time.time : -1f;
		return elapsed > 0f;
	}

	private static float CurrentActiveElapsedSeconds(float startedAt)
	{
		return startedAt >= 0f ? Mathf.Max(0f, Time.time - startedAt) : 0f;
	}

	private static float CurrentSessionDuration(float startedAt, float committedSessionTime)
	{
		return committedSessionTime + CurrentActiveElapsedSeconds(startedAt);
	}

	private static string FormatDuration(float seconds)
	{
		if (seconds < 60f)
		{
			return $"{(int)seconds} s";
		}

		TimeSpan duration = TimeSpan.FromSeconds(seconds);
		if (duration.TotalHours >= 1d)
		{
			return $"{(int)duration.TotalHours}:{duration.Minutes:00}:{duration.Seconds:00} h";
		}

		return $"{duration.Minutes}:{duration.Seconds:00} min";
	}
}

public sealed class LoadingScreenTimeStatTracker : StatTracker
{
	private readonly SavedFloat _loadingScreenTimeSeconds;

	public override string Id => "loading_screen_time";
	public override string Title => "Loading Screen Time";

	public LoadingScreenTimeStatTracker()
	{
		_loadingScreenTimeSeconds = SavedFloat("loading_screen_time_seconds");
	}

	public override string Value() => FormatDuration(_loadingScreenTimeSeconds.Value);

	private static string FormatDuration(float seconds)
	{
		if (seconds < 60f)
		{
			return $"{(int)seconds} s";
		}

		TimeSpan duration = TimeSpan.FromSeconds(seconds);
		if (duration.TotalHours >= 1d)
		{
			return $"{(int)duration.TotalHours}:{duration.Minutes:00}:{duration.Seconds:00} h";
		}

		return $"{duration.Minutes}:{duration.Seconds:00} min";
	}
}

public sealed class LocoEnginesStartedStatTracker : StatTracker
{
	private readonly SavedInt _enginesStartedCount;
	private BaseControlsOverrider? _currentControlsOverrider;

	public override string Id => "loco_engines_started";
	public override string Title => "Loco Engines Started";

	public LocoEnginesStartedStatTracker()
	{
		_enginesStartedCount = SavedInt("loco_engines_started_count");
	}

	public override string Value() => $"{CurrentCount}";
	public int CurrentCount => _enginesStartedCount.Value;

	protected override void SubscribeToEvents()
	{
		SubscribeToCarChanged(OnCarChanged);
		Subscribe(() => { }, UnsubscribeFromCurrentReader);

		if (PlayerManager.Car != null)
		{
			OnCarChanged(PlayerManager.Car);
		}
	}

	private void OnCarChanged(TrainCar? trainCar)
	{
		UnsubscribeFromCurrentReader();

		if (trainCar == null || !trainCar.IsLoco)
		{
			return;
		}

		BaseControlsOverrider? controlsOverrider = trainCar.SimController?.controlsOverrider;
		if (controlsOverrider?.EngineOnReader == null)
		{
			return;
		}

		_currentControlsOverrider = controlsOverrider;
		_currentControlsOverrider.EngineOnReader.StateChanged += OnEngineStateChanged;
	}

	private void OnEngineStateChanged(bool isOn)
	{
		if (!isOn)
		{
			return;
		}

		_enginesStartedCount.Value++;
		NotifyChanged();
	}

	private void UnsubscribeFromCurrentReader()
	{
		if (_currentControlsOverrider?.EngineOnReader == null)
		{
			_currentControlsOverrider = null;
			return;
		}

		_currentControlsOverrider.EngineOnReader.StateChanged -= OnEngineStateChanged;
		_currentControlsOverrider = null;
	}
}

public sealed class TurntableRotationStatTracker : StatTracker
{
	private readonly SavedFloat _degreesRotated;

	public override string Id => "turntable_rotation";
	public override string Title => "Turntable Rotation";

	public TurntableRotationStatTracker()
	{
		_degreesRotated = SavedFloat("turntable_rotation_degrees");
	}

	public override string Value() => $"{(int)TotalDegrees}°";
	public float TotalDegrees => _degreesRotated.Value;

	protected override void SubscribeToEvents()
	{
		SubscribeToTurntableRotated(OnTurntableRotated);
	}

	private void OnTurntableRotated(float degrees)
	{
		if (degrees <= 0f)
		{
			return;
		}

		_degreesRotated.Value += degrees;
		NotifyChanged();
	}
}

public sealed class VisitedStationsStatTracker : StatTracker
{
	private readonly Dictionary<string, StationController> _stations = new();
	private readonly HashSet<string> _visitedStations = new();

	public override string Id => "visited_stations";
	public override string Title => "Stations Visited";

	public VisitedStationsStatTracker()
	{
		foreach (StationController stationController in StationController.allStations.Where(station => station != null).ToList())
		{
			string stationName = stationController.stationInfo.YardID.Trim();
			_stations[stationName] = stationController;

			if (VisitedStation(stationName).Value)
			{
				_visitedStations.Add(stationName);
			}
		}
	}

	public override string Value() => $"{CurrentCount}";
	public int CurrentCount => _stations.Keys.Count(stationName => _visitedStations.Contains(stationName));
	public override IReadOnlyList<DetailEntry> Details() =>
		_stations.Keys
			.OrderBy(stationName => stationName)
			.Select(stationName => new DetailEntry(stationName, _visitedStations.Contains(stationName) ? "Visited" : "Not Visited"))
			.ToList();

	protected override void SubscribeToEvents()
	{
		SubscribeToPlayerPositionChanged(OnPlayerPositionChanged);
	}

	private void OnPlayerPositionChanged(Vector3 playerPosition)
	{
		if (_stations.Count == 0)
		{
			return;
		}

		foreach (KeyValuePair<string, StationController> station in _stations)
		{
			float distanceSquared = station.Value.stationRange.PlayerSqrDistanceFromStationOffice;
			if (!station.Value.stationRange.IsPlayerInRangeForBookletGeneration(distanceSquared) || !_visitedStations.Add(station.Key))
			{
				continue;
			}

			VisitedStation(station.Key).SetTrue();
			NotifyChanged();
			break;
		}
	}

	public List<string> GetRemainingStationNames()
	{
		return _stations.Keys
			.Where(stationName => !_visitedStations.Contains(stationName))
			.OrderBy(stationName => stationName)
			.ToList();
	}

	private SavedBool VisitedStation(string stationName) => SavedBool($"station_visited_{stationName}");
}

public sealed class OperatedVehiclesStatTracker : StatTracker
{
	private static readonly Dictionary<TrainCarType, string> TrackableVehicles = new()
	{
		{ TrainCarType.LocoShunter, "DE2 Shunter" },
		{ TrainCarType.LocoSteamHeavy, "282 Steam Locomotive" },
		{ TrainCarType.LocoS060, "S060 Steam Locomotive" },
		{ TrainCarType.LocoRailbus, "Railbus" },
		{ TrainCarType.LocoDM1U, "DM1U" },
		{ TrainCarType.LocoDiesel, "DE6" },
		{ TrainCarType.LocoDH4, "DH4" },
		{ TrainCarType.LocoDM3, "DM3" },
		{ TrainCarType.LocoMicroshunter, "Microshunter" }
	};

	private static readonly Dictionary<TrainCarType, GeneralLicenseType?> RequiredLicenses = new()
	{
		{ TrainCarType.LocoShunter, GeneralLicenseType.DE2 },
		{ TrainCarType.LocoSteamHeavy, GeneralLicenseType.SH282 },
		{ TrainCarType.LocoS060, GeneralLicenseType.S060 },
		{ TrainCarType.LocoRailbus, null },
		{ TrainCarType.LocoDM1U, null },
		{ TrainCarType.LocoDiesel, GeneralLicenseType.DE6 },
		{ TrainCarType.LocoDH4, GeneralLicenseType.DH4 },
		{ TrainCarType.LocoDM3, GeneralLicenseType.DM3 },
		{ TrainCarType.LocoMicroshunter, null }
	};

	private readonly HashSet<TrainCarType> _operatedVehicles = new();

	public override string Id => "operated_vehicles";
	public override string Title => "Vehicles Operated";

	public OperatedVehiclesStatTracker()
	{
		foreach (TrainCarType trainCarType in TrackableVehicles.Keys)
		{
			if (OperatedVehicle(trainCarType).Value)
			{
				_operatedVehicles.Add(trainCarType);
			}
		}
	}

	public override string Value() => $"{CurrentCount}";
	public int CurrentCount => TrackableVehicles.Keys.Count(trainCarType => _operatedVehicles.Contains(trainCarType));
	public override IReadOnlyList<DetailEntry> Details() =>
		TrackableVehicles
			.OrderBy(vehicle => vehicle.Value)
			.Select(vehicle => new DetailEntry(vehicle.Value, _operatedVehicles.Contains(vehicle.Key) ? "Yes" : "No"))
			.ToList();

	protected override void SubscribeToEvents()
	{
		SubscribeToCarChanged(OnCarChanged);

		if (PlayerManager.Car != null)
		{
			OnCarChanged(PlayerManager.Car);
		}
	}

	private void OnCarChanged(TrainCar? trainCar)
	{
		if (trainCar == null)
		{
			return;
		}

		TrainCarType trainCarType = trainCar.carType;
		if (!TrackableVehicles.ContainsKey(trainCarType))
		{
			return;
		}

		if (!HasRequiredLicense(trainCarType))
		{
			Main.Debug($"Drive every vehicle: {TrackableVehicles[trainCarType]} did not count because the required license is missing");
			return;
		}

		if (!_operatedVehicles.Add(trainCarType))
		{
			return;
		}

		OperatedVehicle(trainCarType).SetTrue();
		NotifyChanged();
	}

	public List<string> GetRemainingVehicleNames()
	{
		return TrackableVehicles
			.Where(vehicle => !_operatedVehicles.Contains(vehicle.Key))
			.Select(vehicle => vehicle.Value)
			.OrderBy(vehicleName => vehicleName)
			.ToList();
	}

	private static bool HasRequiredLicense(TrainCarType trainCarType)
	{
		LicenseManager? licenseManager = SingletonBehaviour<LicenseManager>.Instance;
		if (licenseManager == null)
		{
			return false;
		}

		GeneralLicenseType? requiredLicense = RequiredLicenses[trainCarType];
		if (!requiredLicense.HasValue)
		{
			return true;
		}

		return licenseManager.IsGeneralLicenseAcquired(requiredLicense.Value.ToV2());
	}

	private SavedBool OperatedVehicle(TrainCarType trainCarType) => SavedBool($"vehicle_operated_{trainCarType}");
}

public sealed class DistanceDrivenStatTracker : StatTracker
{
	private static readonly Dictionary<TrainCarType, string> TrackableVehicles = new()
	{
		{ TrainCarType.LocoShunter, "DE2 Shunter" },
		{ TrainCarType.LocoSteamHeavy, "282 Steam Locomotive" },
		{ TrainCarType.LocoS060, "S060 Steam Locomotive" },
		{ TrainCarType.LocoRailbus, "Railbus" },
		{ TrainCarType.LocoDM1U, "DM1U" },
		{ TrainCarType.LocoDiesel, "DE6" },
		{ TrainCarType.LocoDH4, "DH4" },
		{ TrainCarType.LocoDM3, "DM3" },
		{ TrainCarType.LocoMicroshunter, "Microshunter" }
	};

	private readonly Dictionary<TrainCarType, SavedFloat> _forwardDistanceByVehicle = new();
	private readonly Dictionary<TrainCarType, SavedFloat> _backwardDistanceByVehicle = new();
	private float _lastSampleTime = -1f;

	public override string Id => "distance_driven";
	public override string Title => "Distance Driven";

	public DistanceDrivenStatTracker()
	{
		foreach (TrainCarType trainCarType in TrackableVehicles.Keys)
		{
			_forwardDistanceByVehicle[trainCarType] = SavedFloat($"distance_driven_{trainCarType}_forward");
			_backwardDistanceByVehicle[trainCarType] = SavedFloat($"distance_driven_{trainCarType}_backward");
		}
	}

	public override string Value() => FormatDistance(TotalDistance);
	public float TotalDistance => TrackableVehicles.Keys.Sum(GetVehicleTotalDistance);
	public float TotalForwardDistance => TrackableVehicles.Keys.Sum(GetForwardDistance);
	public float TotalBackwardDistance => TrackableVehicles.Keys.Sum(GetBackwardDistance);
	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Total", FormatDistance(TotalDistance)),
		new DetailEntry("Forward", FormatDistance(TotalForwardDistance)),
		new DetailEntry("Backward", FormatDistance(TotalBackwardDistance)),
		..
		TrackableVehicles
			.OrderBy(vehicle => vehicle.Value)
			.Select(vehicle => new DetailEntry(vehicle.Value, FormatDistance(GetVehicleTotalDistance(vehicle.Key))))
	];

	protected override void SubscribeToEvents()
	{
		SubscribeToPlayerPositionChanged(OnHeartbeat);
		SubscribeToCarChanged(_ => ResetSampling());
	}

	private void OnHeartbeat(Vector3 _)
	{
		float currentTime = Time.time;
		if (_lastSampleTime < 0f)
		{
			_lastSampleTime = currentTime;
			return;
		}

		float deltaTime = currentTime - _lastSampleTime;
		_lastSampleTime = currentTime;
		if (deltaTime <= 0f || FastTravelController.IsFastTravelling)
		{
			return;
		}

		TrainCar? trainCar = PlayerManager.Car;
		if (trainCar == null || !TrackableVehicles.ContainsKey(trainCar.carType))
		{
			return;
		}

		float signedSpeed = Vector3.Dot(trainCar.transform.forward, trainCar.GetVelocity());
		float distance = Mathf.Abs(signedSpeed) * deltaTime;
		if (distance <= 0f)
		{
			return;
		}

		if (signedSpeed >= 0f)
		{
			_forwardDistanceByVehicle[trainCar.carType].Value += distance;
		}
		else
		{
			_backwardDistanceByVehicle[trainCar.carType].Value += distance;
		}

		NotifyChanged();
	}

	private void ResetSampling()
	{
		_lastSampleTime = -1f;
	}

	private float GetVehicleTotalDistance(TrainCarType trainCarType)
	{
		return GetForwardDistance(trainCarType) + GetBackwardDistance(trainCarType);
	}

	private float GetForwardDistance(TrainCarType trainCarType)
	{
		return _forwardDistanceByVehicle[trainCarType].Value;
	}

	private float GetBackwardDistance(TrainCarType trainCarType)
	{
		return _backwardDistanceByVehicle[trainCarType].Value;
	}

	private static string FormatDistance(float distanceInMeters)
	{
		if (distanceInMeters < 1000f)
		{
			return $"{(int)distanceInMeters} m";
		}

		return $"{distanceInMeters / 1000f:0.0} km";
	}
}

public sealed class JunctionsSwitchedStatTracker : StatTracker
{
	private readonly SavedInt _junctionsSwitched;
	private Junction[] _junctions = Array.Empty<Junction>();

	public override string Id => "junctions_switched";
	public override string Title => "Junctions Switched";

	public JunctionsSwitchedStatTracker()
	{
		_junctionsSwitched = SavedInt("junctions_switched");
	}

	public override string Value() => $"{CurrentCount}";
	public int CurrentCount => _junctionsSwitched.Value;

	protected override void SubscribeToEvents()
	{
		RailTrackRegistryBase? registry = SingletonBehaviour<RailTrackRegistryBase>.Instance;
		if (registry == null)
		{
			Main.Warning("Junctions Switched: RailTrackRegistryBase instance was not available.");
			return;
		}

		_junctions = registry.OrderedJunctions ?? Array.Empty<Junction>();
		Subscribe(
			SubscribeToJunctions,
			UnsubscribeFromJunctions
		);
	}

	private void SubscribeToJunctions()
	{
		foreach (Junction junction in _junctions)
		{
			if (junction != null)
			{
				junction.Switched += OnJunctionSwitched;
			}
		}
	}

	private void UnsubscribeFromJunctions()
	{
		foreach (Junction junction in _junctions)
		{
			if (junction != null)
			{
				junction.Switched -= OnJunctionSwitched;
			}
		}
	}

	private void OnJunctionSwitched(Junction.SwitchMode mode, int selectedBranch)
	{
		_junctionsSwitched.Value++;
		NotifyChanged();
	}
}

public sealed class UnlockedGaragesStatTracker : StatTracker
{
	private static readonly Dictionary<Garage, string> TrackableGarages = new()
	{
		{ Garage.Bob, "Old Bob's Garage" },
		{ Garage.Caboose, "Reginald's Garage" },
		{ Garage.DM1U, "Olaf's Garage" },
		{ Garage.DE6_Slug, "Steve's Garage" }
	};

	private readonly HashSet<Garage> _unlockedGarages = new();

	public override string Id => "unlocked_garages";
	public override string Title => "Garages Unlocked";

	public override string Value() => $"{_unlockedGarages.Count}";
	public int CurrentCount => _unlockedGarages.Count;
	public override IReadOnlyList<DetailEntry> Details() =>
		TrackableGarages
			.OrderBy(garage => garage.Value)
			.Select(garage => new DetailEntry(garage.Value, _unlockedGarages.Contains(garage.Key) ? "Unlocked" : "Locked"))
			.ToList();

	protected override void SubscribeToEvents()
	{
		LicenseManager? licenseManager = SingletonBehaviour<LicenseManager>.Instance;
		if (licenseManager == null)
		{
			Main.Warning("Garage Sale Enthusiast: LicenseManager instance was not available.");
			return;
		}

		Subscribe(
			() => licenseManager.GarageUnlocked += OnGarageUnlocked,
			() => licenseManager.GarageUnlocked -= OnGarageUnlocked
		);

		RefreshUnlockedGarages(licenseManager);
	}

	private void OnGarageUnlocked(GarageType_v2 unlockedGarage)
	{
		LicenseManager? licenseManager = SingletonBehaviour<LicenseManager>.Instance;
		if (licenseManager == null || !TrackableGarages.Keys.Any(garage => garage.ToV2() == unlockedGarage))
		{
			return;
		}

		RefreshUnlockedGarages(licenseManager);
	}

	private void RefreshUnlockedGarages(LicenseManager licenseManager)
	{
		int previousCount = _unlockedGarages.Count;
		_unlockedGarages.Clear();

		foreach (Garage garage in TrackableGarages.Keys)
		{
			if (licenseManager.GetUnlockedGarages().Contains(garage.ToV2()))
			{
				_unlockedGarages.Add(garage);
			}
		}

		if (_unlockedGarages.Count != previousCount)
		{
			NotifyChanged();
		}
	}

	public List<string> GetRemainingGarageNames()
	{
		return TrackableGarages
			.Where(garage => !_unlockedGarages.Contains(garage.Key))
			.Select(garage => garage.Value)
			.OrderBy(garageName => garageName)
			.ToList();
	}
}

public sealed class CompletedJobsStatTracker : StatTracker
{
	private readonly SavedInt _transportCount;
	private readonly SavedInt _emptyHaulCount;
	private readonly SavedInt _shuntingLoadCount;
	private readonly SavedInt _shuntingUnloadCount;

	public override string Id => "completed_jobs";
	public override string Title => "Jobs Completed";

	public CompletedJobsStatTracker()
	{
		_transportCount = SavedInt("job_type_transport_completed_count");
		_emptyHaulCount = SavedInt("job_type_empty_haul_completed_count");
		_shuntingLoadCount = SavedInt("job_type_shunting_load_completed_count");
		_shuntingUnloadCount = SavedInt("job_type_shunting_unload_completed_count");
	}

	public override string Value() => $"{TotalCompletedJobs}";
	public int TotalCompletedJobs => TransportCount + EmptyHaulCount + ShuntingLoadCount + ShuntingUnloadCount;
	public int TransportCount => _transportCount.Value;
	public int EmptyHaulCount => _emptyHaulCount.Value;
	public int ShuntingLoadCount => _shuntingLoadCount.Value;
	public int ShuntingUnloadCount => _shuntingUnloadCount.Value;
	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Total", $"{TotalCompletedJobs}"),
		new DetailEntry("Transport", $"{TransportCount}"),
		new DetailEntry("Empty Haul", $"{EmptyHaulCount}"),
		new DetailEntry("Shunting Load", $"{ShuntingLoadCount}"),
		new DetailEntry("Shunting Unload", $"{ShuntingUnloadCount}")
	];

	protected override void SubscribeToEvents()
	{
		SubscribeToJobCompletion(OnJobCompletion);
	}

	private void OnJobCompletion(Job job)
	{
		switch (job.jobType)
		{
			case JobType.Transport:
				_transportCount.Value++;
				break;
			case JobType.EmptyHaul:
				_emptyHaulCount.Value++;
				break;
			case JobType.ShuntingLoad:
				_shuntingLoadCount.Value++;
				break;
			case JobType.ShuntingUnload:
				_shuntingUnloadCount.Value++;
				break;
			default:
				return;
		}

		NotifyChanged();
	}
}

public sealed class CancelledJobsStatTracker : StatTracker
{
	private readonly SavedInt _cancelledJobsCount;

	public override string Id => "cancelled_jobs";
	public override string Title => "Jobs Cancelled";

	public CancelledJobsStatTracker()
	{
		_cancelledJobsCount = SavedInt("cancelled_jobs_count");
	}

	public override string Value() => $"{CurrentCount}";
	public int CurrentCount => _cancelledJobsCount.Value;

	protected override void SubscribeToEvents()
	{
		SubscribeToJobCancellation(OnJobCancelled);
	}

	private void OnJobCancelled(Job _)
	{
		_cancelledJobsCount.Value++;
		NotifyChanged();
	}
}
