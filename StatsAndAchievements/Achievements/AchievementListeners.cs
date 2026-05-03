using DV.JObjectExtstensions;
using DV.Logic.Job;
using DV.LocoRestoration;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StatsAndAchievements.Achievements
{
	public sealed class SpeedDemonAchievementListener : ProgressAchievementListener
	{
		public override string Id => "speed_demon";
		public override string Title => "Speed Demon";
		public override string Description => @"
			Reach a speed of 100km/h with any locomotive
			";

		public override string Value() => $"{(int)_maxSpeed.Value} km/h";
		public override string ValueName() => "Highest Speed";
		public override string Target() => "100 km/h";
		public override string Progress() => $"{(int)_maxSpeed.Value}/100 kmh";

		private readonly SavedFloat _maxSpeed;

		public SpeedDemonAchievementListener()
		{
			_maxSpeed = SavedFloat("max_speed");
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToSpeedIncreased(OnSpeedIncreased);
		}

		void OnSpeedIncreased(float speed)
		{
			if (speed <= _maxSpeed.Value)
			{
				return;
			}

			_maxSpeed.Value = speed;

			if (speed > 100.0f)
			{
				TriggerUnlock();
			}
		}
	}

	// TODO: trigger while honking and not only when the honking ends
	public sealed class MaximumHonkAchievementListener : ConditionAchievementListener
	{
		public override string Id => "maximum_honk";
		public override string Title => "Maximum Honk";
		public override string Description => @"
			Use the horn for a honk of at least 5 seconds
			";

		private float _honkStartedAt = 0.0f;

		protected override void SubscribeToEvents()
		{
			SubscribeToHonkStarted(OnHonkStarted);
			SubscribeToHonkEnded(OnHonkEnded);
		}

		void OnHonkStarted()
		{
			if (IsUnlocked()) return;

			_honkStartedAt = Time.time;
		}

		void OnHonkEnded()
		{
			if (IsUnlocked()) return;

			if (Time.time - _honkStartedAt > 5f)
			{
				TriggerUnlock();
			}
		}
	}

	// This achievement is implemented as a conditional achievement, since the total amount of licenses
	// is already tracked in the vanilla stats menu
	public sealed class TheEndAchievementListener : ConditionAchievementListener
	{
		public override string Id => "the_end";
		public override string Title => "The End?";
		public override string Description => @"
			Acquire all licenses
			";

		protected override void SubscribeToEvents()
		{
			SubscribeToGeneralLicenseAcquired(_ => OnLicenseAcquired());
			SubscribeToJobLicenseAcquired(_ => OnLicenseAcquired());
		}

		void OnLicenseAcquired()
		{
			if (IsUnlocked()) return;

			int acquiredLicensesCount = SingletonBehaviour<LicenseManager>.Instance.GetNumberOfAcquiredGeneralLicenses() + SingletonBehaviour<LicenseManager>.Instance.GetNumberOfAcquiredJobLicenses();
			int allLicensesCount = SingletonBehaviour<LicenseManager>.Instance.AllLicensesCount;

			if (acquiredLicensesCount >= allLicensesCount)
			{
				TriggerUnlock();
			}
		}
	}

	// This achievement is implemented as a conditional achievement, since the total amount of licenses
	// is already tracked in the vanilla stats menu
	public sealed class SteamEngineRequirementsAchievementListener : ProgressAchievementListener
	{
		public override string Id => "steam_engine_requirements";
		public override string Title => "Fully equipped";
		public override string Description => @"
			Acquire everything you need to drive a steamer, including the license, a shovel, a lighter and oil.
		";

		public override string Value() => $"{ProgressCount()}";
		public override string ValueName() => "Job Types Completed";
		public override string Target() => $"{TargetCount}";

		private readonly SavedBool _hasAcquiredSteamLicense;
		private readonly SavedBool _hasAcquiredShovel;
		private readonly SavedBool _hasAcquiredLighter;
		private readonly SavedBool _hasAcquiredOil;

		private int TargetCount => 4;
		private int ProgressCount()
		{
			int progressCount = 0;
			if (_hasAcquiredSteamLicense) progressCount++;
			if (_hasAcquiredShovel) progressCount++;
			if (_hasAcquiredLighter) progressCount++;
			if (_hasAcquiredOil) progressCount++;

			return progressCount;
		}

		public SteamEngineRequirementsAchievementListener()
		{
			_hasAcquiredSteamLicense = SavedBool("has_acquired_steam_license");
			_hasAcquiredShovel = SavedBool("has_acquired_shovel");
			_hasAcquiredLighter = SavedBool("has_acquired_lighter");
			_hasAcquiredOil = SavedBool("has_acquired_oiler");
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToGeneralLicenseAcquired(OnGeneralLicenseAcquired);
			SubscribeToItemAcquired(OnItemAcquired);
		}

		void OnGeneralLicenseAcquired(GeneralLicenseType_v2 license)
		{
			if (IsUnlocked()) return;

			var steamer = GeneralLicenseType.S060 | GeneralLicenseType.SH282;
			bool isSteamer = (license.v1 & steamer) != 0;
			if (isSteamer)
			{
				_hasAcquiredSteamLicense.SetTrue();
			}

			if (ProgressCount() >= TargetCount) TriggerUnlock();
		}

		private void OnItemAcquired(string itemName)
		{
			if (IsUnlocked()) return;

			if (itemName.ToLower().Contains("shovel"))
			{
				_hasAcquiredShovel.SetTrue();
			}

			if (itemName.ToLower().Contains("lighter"))
			{
				_hasAcquiredLighter.SetTrue();
			}

			if (itemName.ToLower().Contains("oiler"))
			{
				_hasAcquiredOil.SetTrue();
			}

			if (ProgressCount() >= TargetCount) TriggerUnlock();
		}
	}

	public sealed class ScroogeMcDuckAchievementListener : MilestoneAchievementListener
	{
		public override string Id => "scrooge_mc_duck";
		public override string Value() => $"${(int)_moneyEarned.Value}"; // TODO: thousand separator (see Varsius' KittyCat PR)
		public override string ValueName() => "Money Earned";

		private readonly SavedFloat _moneyEarned;
		protected override string BaseTitle => "Scrooge McDuck";
		protected override string MaxRankDescription => "Earn $10,000,000 throughout your career";
		protected override float[] Milestones => [100_000f, 1_000_000f, 10_000_000f];
		protected override float CurrentValue => _moneyEarned.Value;

		public ScroogeMcDuckAchievementListener()
		{
			_moneyEarned = SavedFloat("money_earned");
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToJobCompletion(OnJobCompletion);
		}

		void OnJobCompletion(Job job)
		{
			float previousMoneyEarned = _moneyEarned.Value;
			_moneyEarned.Value += job.GetWageForTheJob();
			NotifyMilestoneProgress(previousMoneyEarned, _moneyEarned.Value);
		}

		protected override string GetMilestoneDescription(float target)
		{
			return $"Earn ${(int)target} throughout your career";
		}

		protected override string FormatValue(float value)
		{
			return $"${(int)value}";
		}
	}

	public sealed class VisitAllStationsAchievementListener : ProgressAchievementListener
	{
		public override string Id => "visit_all_stations";
		public override string Title => "Traveling Salesman";
		public override string Description
		{
			get
			{
				var remainingStations = GetRemainingStationNames();
				if (remainingStations.Count == 0)
				{
					return "Visit all stations";
				}

				return $"Visit all stations\n\nNot visited yet: {string.Join(", ", remainingStations)}";
			}
		}

		public override string Value() => $"{VisitedStationCount()}";
		public override string ValueName() => "Stations Visited";
		public override string Target() => $"{KnownStationCount()}";

		private readonly Dictionary<string, StationController> _stations = new Dictionary<string, StationController>();
		private readonly HashSet<string> _visitedStations = new HashSet<string>();

		public VisitAllStationsAchievementListener()
		{
			foreach (StationController stationController in StationController.allStations.Where((x) => { return x != null; }).ToList())
			{
				string stationName = stationController.stationInfo.YardID.Trim();
				_stations[stationName] = stationController;

				if (VisitedStation(stationName).Value)
				{
					_visitedStations.Add(stationName);
				}
			}
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToPlayerPositionChanged(OnPlayerPositionChanged);
		}

		void OnPlayerPositionChanged(Vector3 playerPosition)
		{
			if (_stations.Count == 0)
			{
				return;
			}

			float closestDistance = float.MaxValue;

			foreach (KeyValuePair<string, StationController> station in _stations)
			{
				float distanceSquared = station.Value.stationRange.PlayerSqrDistanceFromStationOffice;
				float distance = Mathf.Sqrt(distanceSquared);
				if (distance < closestDistance)
				{
					closestDistance = distance;
				}

				if (station.Value.stationRange.IsPlayerInRangeForBookletGeneration(distanceSquared) && _visitedStations.Add(station.Key))
				{
					VisitedStation(station.Key).SetTrue();
					break;
				}
			}

			if (!IsUnlocked() && _stations.Count > 0 && _visitedStations.Count >= _stations.Count)
			{
				TriggerUnlock();
			}
		}

		private List<string> GetRemainingStationNames()
		{
			return _stations.Keys
				.Where(stationName => !_visitedStations.Contains(stationName))
				.OrderBy(stationName => stationName)
				.ToList();
		}

		private int KnownStationCount()
		{
			return _stations.Count;
		}

		private int VisitedStationCount()
		{
			return _stations.Keys.Count(stationName => _visitedStations.Contains(stationName));
		}

		private SavedBool VisitedStation(string stationName) => SavedBool($"visited_{stationName}");
	}

	public sealed class CompleteTheMuseumAchievementListener : ConditionAchievementListener
	{
		public override string Id => "complete_the_museum";
		public override string Title => "Complete the Museum";
		public override string Description => @"
			Complete all locomotive restorations
			";

		protected override void SubscribeToEvents()
		{
			foreach (LocoRestorationController controller in LocoRestorationController.allLocoRestorationControllers)
			{
				Subscribe(
					() => controller.StateChanged += OnRestorationStateChanged,
					() => controller.StateChanged -= OnRestorationStateChanged
				);
			}

			CheckMuseumCompletion();
		}

		private void OnRestorationStateChanged(LocoRestorationController controller, TrainCarLivery locoLivery, LocoRestorationController.RestorationState restorationState)
		{
			CheckMuseumCompletion();
		}

		private void CheckMuseumCompletion()
		{
			if (IsUnlocked()) return;
			if (LocoRestorationController.allLocoRestorationControllers.Count == 0) return;

			if (LocoRestorationController.allLocoRestorationControllers.All(controller => controller.State == LocoRestorationController.RestorationState.S10_PaintJobDone))
			{
				TriggerUnlock();
			}
		}
	}

	public sealed class DriveEveryVehicleAchievementListener : ProgressAchievementListener
	{
		public override string Id => "drive_every_vehicle";
		public override string Title => "Jack of All Trades";
		public override string Description
		{
			get
			{
				var remainingVehicles = GetRemainingVehicleNames();
				if (remainingVehicles.Count == 0)
				{
					return "Operate every drivable vehicle";
				}

				return $"Operate every drivable vehicle\n\nNot operated yet: {string.Join(", ", remainingVehicles)}";
			}
		}

		public override string Value() => $"{OperatedVehicleCount()}";
		public override string ValueName() => "Vehicles Operated";
		public override string Target() => $"{TrackableVehicles.Count}";

		private static readonly Dictionary<TrainCarType, string> TrackableVehicles = new Dictionary<TrainCarType, string>
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

		private static readonly Dictionary<TrainCarType, GeneralLicenseType?> RequiredLicenses = new Dictionary<TrainCarType, GeneralLicenseType?>
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

		private readonly HashSet<TrainCarType> _operatedVehicles = new HashSet<TrainCarType>();

		public DriveEveryVehicleAchievementListener()
		{
			foreach (TrainCarType trainCarType in TrackableVehicles.Keys)
			{
				if (OperatedVehicle(trainCarType).Value)
				{
					_operatedVehicles.Add(trainCarType);
				}
			}
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToCarChanged(OnCarChanged);

			if (PlayerManager.Car != null)
			{
				OnCarChanged(PlayerManager.Car);
			}
		}

		private void OnCarChanged(TrainCar trainCar)
		{
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

			if (_operatedVehicles.Add(trainCarType))
			{
				OperatedVehicle(trainCarType).SetTrue();
			}

			if (!IsUnlocked() && _operatedVehicles.Count >= TrackableVehicles.Count)
			{
				TriggerUnlock();
			}
		}

		private List<string> GetRemainingVehicleNames()
		{
			return TrackableVehicles
				.Where(vehicle => !_operatedVehicles.Contains(vehicle.Key))
				.Select(vehicle => vehicle.Value)
				.OrderBy(vehicleName => vehicleName)
				.ToList();
		}

		private int OperatedVehicleCount()
		{
			return TrackableVehicles.Keys.Count(trainCarType => _operatedVehicles.Contains(trainCarType));
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

		private SavedBool OperatedVehicle(TrainCarType trainCarType) => SavedBool($"operated_{trainCarType}");
	}

	public sealed class TheEngineerAchievementListener : ConditionAchievementListener
	{
		public override string Id => "the_engineer";
		public override string Title => "The Engineer";
		public override string Description => @"
			Complete your first manual service
			";

		protected override void SubscribeToEvents()
		{
			SubscribeToPitStopCheckout(OnPitStopCheckout);
		}

		void OnPitStopCheckout(float amount, bool hasPaid)
		{
			TriggerUnlock();
		}
	}

	public sealed class GraphicDesignIsMyPassionAchievementListener : ConditionAchievementListener
	{
		public override string Id => "graphic_design_is_my_passion";
		public override string Title => "Graphic Design is my Passion";
		public override string Description => @"
			Do a paint job
			";

		protected override void SubscribeToEvents()
		{
			SubscribeToPaintJobApplied(OnPaintJobApplied);
		}

		void OnPaintJobApplied()
		{
			if (IsUnlocked()) return;

			TriggerUnlock();
		}
	}

	public sealed class GarageSaleEnthusiastAchievementListener : ProgressAchievementListener
	{
		public override string Id => "garage_sale_enthusiast";
		public override string Title => "Garage Sale Enthusiast";
		public override string Description
		{
			get
			{
				List<string> remainingGarages = GetRemainingGarageNames();
				if (remainingGarages.Count == 0)
				{
					return "Buy everything in the garages";
				}

				return $"Buy everything in the garages\n\nNot unlocked yet: {string.Join(", ", remainingGarages)}";
			}
		}

		public override string Value() => $"{_unlockedGarages.Count}";
		public override string ValueName() => "Garages Unlocked";
		public override string Target() => "4";

		// As of may 2026, the `Garage` enum contains entries that are not related to an actual garage.
		// Hard-coding the required garages should only fail, if new garages are introduces or if there is a custom map with different garages.
		// If this should be the case, the `PadlockKeyType` enum might come in handy.
		private static readonly Dictionary<Garage, string> TrackableGarages = new Dictionary<Garage, string>
		{
			{ Garage.Bob, "BE2 Microshunter" },
			{ Garage.Caboose, "Caboose" },
			{ Garage.DM1U, "DM1U" },
			{ Garage.DE6_Slug, "DE6 Slug" }
		};

		private readonly HashSet<Garage> _unlockedGarages = new HashSet<Garage>();

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
			if (licenseManager == null)
			{
				return;
			}

			if (!TrackableGarages.Keys.Any(garage => garage.ToV2() == unlockedGarage))
			{
				return;
			}

			RefreshUnlockedGarages(licenseManager);
		}

		private void RefreshUnlockedGarages(LicenseManager licenseManager)
		{
			_unlockedGarages.Clear();

			foreach (Garage garage in TrackableGarages.Keys)
			{
				if (licenseManager.GetUnlockedGarages().Contains(garage.ToV2()))
				{
					_unlockedGarages.Add(garage);
				}
			}

			if (!IsUnlocked() && _unlockedGarages.Count >= TrackableGarages.Count)
			{
				TriggerUnlock();
			}
		}

		private List<string> GetRemainingGarageNames()
		{
			return TrackableGarages
				.Where(garage => !_unlockedGarages.Contains(garage.Key))
				.Select(garage => garage.Value)
				.OrderBy(garageName => garageName)
				.ToList();
		}
	}

	public sealed class HomeSweetHomeAchievementListener : SecretConditionAchievementListener
	{
		public override string Id => "home_sweet_home";
		public override string Title => "Home Sweet Home";
		public override string Description => @"
			Visit your home garage
			";

		private static readonly Vector3 HomeGaragePosition = new Vector3(768.1255f, 155.95f, 216.2065f);
		private const float UnlockRadius = 5.0f;

		protected override void SubscribeToEvents()
		{
			SubscribeToPlayerPositionChanged(OnPlayerPositionChanged);
		}

		void OnPlayerPositionChanged(Vector3 playerPosition)
		{
			if (IsUnlocked()) return;

			float distanceSquared = (playerPosition - HomeGaragePosition).sqrMagnitude;
			if (distanceSquared <= UnlockRadius * UnlockRadius)
			{
				TriggerUnlock();
			}
		}
	}

	// TODO: When we are able to detect if passenger jobs is installed alongside this mod, then conditionally include regional/express job
	public sealed class AllRounderAchievementListener : ProgressAchievementListener
	{
		public override string Id => "all_rounder";
		public override string Title => "All-Rounder";
		public override string Description => @"
			Complete a job of each type
			";

		public override string Value() => $"{CompletedJobTypeCount()}";
		public override string ValueName() => "Job Types Completed";
		public override string Target() => $"{targetCount}";

		private readonly SavedBool _hasCompletedTransport;
		private readonly SavedBool _hasCompletedEmptyHaul;
		private readonly SavedBool _hasCompletedShuntingLoad;
		private readonly SavedBool _hasCompletedShuntingUnload;

		private int targetCount => 4;

		public AllRounderAchievementListener()
		{
			_hasCompletedTransport = SavedBool("completed_transport");
			_hasCompletedEmptyHaul = SavedBool("completed_empty_haul");
			_hasCompletedShuntingLoad = SavedBool("completed_shunting_load");
			_hasCompletedShuntingUnload = SavedBool("completed_shunting_unload");
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToJobCompletion(OnJobCompletion);
		}

		void OnJobCompletion(Job job)
		{
			if (IsUnlocked()) return;

			switch (job.jobType)
			{
				case JobType.Transport:
					_hasCompletedTransport.SetTrue();
					break;
				case JobType.EmptyHaul:
					_hasCompletedEmptyHaul.SetTrue();
					break;
				case JobType.ShuntingLoad:
					_hasCompletedShuntingLoad.SetTrue();
					break;
				case JobType.ShuntingUnload:
					_hasCompletedShuntingUnload.SetTrue();
					break;
			}

			if (CompletedJobTypeCount() >= targetCount)
			{
				TriggerUnlock();
			}
		}

		private int CompletedJobTypeCount()
		{
			int completedJobTypeCount = 0;

			if (_hasCompletedTransport)
				completedJobTypeCount++;
			if (_hasCompletedEmptyHaul)
				completedJobTypeCount++;
			if (_hasCompletedShuntingLoad)
				completedJobTypeCount++;
			if (_hasCompletedShuntingUnload)
				completedJobTypeCount++;

			return completedJobTypeCount;
		}
	}

	public sealed class FirstHazmatJobCompletionAchievementListener : ConditionAchievementListener
	{
		public override string Id => "first_hazmat_job_completion";
		public override string Title => "No Smoking on Job Site";
		public override string Description => @"
			Complete your first hazmat job
			";

		protected override void SubscribeToEvents()
		{
			SubscribeToJobCompletion(OnJobCompletion);
		}

		void OnJobCompletion(Job job)
		{
			if (IsUnlocked()) return;

			if (JobHasHazmatLicenseRequirement(job))
			{
				TriggerUnlock();
			}
		}

		private static bool JobHasHazmatLicenseRequirement(Job job)
		{
			if (job is null) return false;

			const JobLicenses hazmatLicenses = JobLicenses.Hazmat1 | JobLicenses.Hazmat2 | JobLicenses.Hazmat3;
			return (job.requiredLicenses & hazmatLicenses) != 0;
		}
	}

	public sealed class FirstMilitaryJobCompletionAchievementListener : ConditionAchievementListener
	{
		public override string Id => "first_military_job_completion";
		public override string Title => "Classified Cargo";
		public override string Description => @"
			Complete your first military job
			";

		protected override void SubscribeToEvents()
		{
			SubscribeToJobCompletion(OnJobCompletion);
		}

		void OnJobCompletion(Job job)
		{
			if (IsUnlocked()) return;

			if (JobHasMilitaryLicenseRequirement(job))
			{
				TriggerUnlock();
			}
		}

		private static bool JobHasMilitaryLicenseRequirement(Job job)
		{
			if (job is null) return false;

			const JobLicenses militaryLicenses = JobLicenses.Military1 | JobLicenses.Military2 | JobLicenses.Military3;
			return (job.requiredLicenses & militaryLicenses) != 0;
		}
	}

	public sealed class FirstFragileJobCompletionAchievementListener : ConditionAchievementListener
	{
		public override string Id => "first_fragile_job_completion";
		public override string Title => "Handle With Care";
		public override string Description => @"
			Complete your first fragile job
			";

		protected override void SubscribeToEvents()
		{
			SubscribeToJobCompletion(OnJobCompletion);
		}

		void OnJobCompletion(Job job)
		{
			if (IsUnlocked()) return;

			if (JobHasFragileLicenseRequirement(job))
			{
				TriggerUnlock();
			}
		}

		private static bool JobHasFragileLicenseRequirement(Job job)
		{
			if (job is null) return false;

			return (job.requiredLicenses & JobLicenses.Fragile) != 0;
		}
	}
}
