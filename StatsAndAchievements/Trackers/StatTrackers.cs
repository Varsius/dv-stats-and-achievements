using System;
using System.Collections.Generic;
using System.Linq;
using DV.Logic.Job;
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

	public override string Id => "money_spent";
	public override string Title => "Money Spent";

	public MoneySpentStatTracker()
	{
		_itemShopMoneySpent = SavedFloat("money_spent_item_shop");
		_manualServiceMoneySpent = SavedFloat("money_spent_manual_service");
		_careerManagerFeesMoneySpent = SavedFloat("money_spent_career_manager_fees");
		_careerManagerLicensesMoneySpent = SavedFloat("money_spent_career_manager_licenses");
		_fastTravelMoneySpent = SavedFloat("money_spent_fast_travel");
	}

	public override string Value() => $"${(int)TotalValue}";
	public float TotalValue => ItemShopValue + ManualServiceValue + CareerManagerFeesValue + CareerManagerLicensesValue + FastTravelValue;
	public float ItemShopValue => _itemShopMoneySpent.Value;
	public float ManualServiceValue => _manualServiceMoneySpent.Value;
	public float CareerManagerFeesValue => _careerManagerFeesMoneySpent.Value;
	public float CareerManagerLicensesValue => _careerManagerLicensesMoneySpent.Value;
	public float FastTravelValue => _fastTravelMoneySpent.Value;
	// TODO: Track comms radio payments.

	public override IReadOnlyList<DetailEntry> Details() =>
	[
		new DetailEntry("Total", FormatMoney(TotalValue)),
		new DetailEntry("Item Shop", FormatMoney(ItemShopValue)),
		new DetailEntry("Manual Service", FormatMoney(ManualServiceValue)),
		new DetailEntry("Career Manager Fees", FormatMoney(CareerManagerFeesValue)),
		new DetailEntry("Career Manager Licenses", FormatMoney(CareerManagerLicensesValue)),
		new DetailEntry("Fast Travel", FormatMoney(FastTravelValue))
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

// TODO: this should track the distances traveled with each loco instead.
// Then, the achievement can check that each distance > 0.
// Suggestions: convert this to a DistanceWithLocoStatTracker and make each individual loco a detail
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
			.Select(vehicle => new DetailEntry(vehicle.Value, _operatedVehicles.Contains(vehicle.Key) ? "Operated" : "Not Operated"))
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
		// TODO: rethink the display names, maybe use Bob, Dave, etc.
		{ Garage.Bob, "BE2 Microshunter" },
		{ Garage.Caboose, "Caboose" },
		{ Garage.DM1U, "DM1U" },
		{ Garage.DE6_Slug, "DE6 Slug" }
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
