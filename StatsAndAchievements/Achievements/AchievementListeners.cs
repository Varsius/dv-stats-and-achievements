using DV.JObjectExtstensions;
using DV.Logic.Job;
using DV.LocoRestoration;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using System;
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

		public override string Value() => $"{(int)this.maxSpeed} km/h";
		public override string ValueName() => "Highest Speed";
		public override string Target() => "100 km/h";
		public override string Progress() => $"{(int)this.maxSpeed}/100 kmh";

		private float maxSpeed;

		public SpeedDemonAchievementListener()
		{
			float? maxSpeed = Main.saaSaveData.GetFloat($"{Id}_max_speed");
			if (maxSpeed.HasValue)
			{
				this.maxSpeed = maxSpeed.Value;
			}
			else
			{
				this.maxSpeed = 0.0f;
			}
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToSpeedIncreased(OnSpeedIncreased);
		}

		protected override void OnSpeedIncreased(float speed)
		{
			if (speed <= this.maxSpeed)
			{
				return;
			}

			this.maxSpeed = speed;
			Main.saaSaveData.SetFloat($"{Id}_max_speed", speed);

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

		protected override void OnHonkStarted()
		{
			if (IsUnlocked()) return;

			_honkStartedAt = Time.time;
		}

		protected override void OnHonkEnded()
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
			SubscribeToLicenseAcquired(OnLicenseAcquired);
		}

		protected override void OnLicenseAcquired(String licenseName)
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

	public sealed class ScroogeMcDuckAchievementListener : ProgressAchievementListener
	{
		public override string Id => "scrooge_mc_duck";
		public override string Title => "Scrooge McDuck";
		public override string Description => @"
			Earn $1,000,000 throughout your carreer
			";

		public override string Value() => $"${(int)this.moneyEarned}"; // TODO: thousand seperator (see Varsius' KittyCat PR)
		public override string ValueName() => "Money Earned";
		public override string Target() => "$1,000,000";

		private float moneyEarned;

		public ScroogeMcDuckAchievementListener()
		{
			float? moneyEarned = Main.saaSaveData.GetFloat($"{Id}_money_earned");
			if (moneyEarned.HasValue)
			{
				this.moneyEarned = moneyEarned.Value;
			}
			else
			{
				this.moneyEarned = 0.0f;
			}
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToJobCompletion(OnJobCompletion);
		}

		protected override void OnJobCompletion(Job job)
		{
			moneyEarned += job.GetWageForTheJob();
			Main.saaSaveData.SetFloat($"{Id}_money_earned", moneyEarned);

			if (moneyEarned >= 1_000_000f)
			{
				TriggerUnlock();
			}
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

				if (Main.saaSaveData.GetBool(GetVisitedKey(stationName)) == true)
				{
					_visitedStations.Add(stationName);
				}
			}
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToPlayerPositionChanged(OnPlayerPositionChanged);
		}

		protected override void OnPlayerPositionChanged(Vector3 playerPosition)
		{
			if (_stations.Count == 0)
			{
				return;
			}

			string? closestStationName = null;
			float closestDistance = float.MaxValue;

			foreach (KeyValuePair<string, StationController> station in _stations)
			{
				float distanceSquared = station.Value.stationRange.PlayerSqrDistanceFromStationOffice;
				float distance = Mathf.Sqrt(distanceSquared);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					closestStationName = station.Key;
				}

				if (station.Value.stationRange.IsPlayerInRangeForBookletGeneration(distanceSquared) && _visitedStations.Add(station.Key))
				{
					Main.saaSaveData.SetBool(GetVisitedKey(station.Key), true);
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

		private string GetVisitedKey(string stationName) => $"{Id}_visited_{stationName}";
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
				if (Main.saaSaveData.GetBool(GetOperatedKey(trainCarType)) == true)
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
				Main.saaSaveData.SetBool(GetOperatedKey(trainCarType), true);
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

		private string GetOperatedKey(TrainCarType trainCarType) => $"{Id}_operated_{trainCarType}";
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

		protected override void OnPitStopCheckout(float amount, bool hasPaid)
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

		protected override void OnPaintJobApplied()
		{
			if (IsUnlocked()) return;

			TriggerUnlock();
		}
	}

	// TODO: does not work
	// The Garage enum contains 11 values (including relics/museum locos and flat car)
	// Tracking does not work nicely, seams to look at UnlockablesManager from previous save game?
	public sealed class GarageSaleEnthusiastAchievementListener : ProgressAchievementListener
	{
		public override string Id => "garage_sale_enthusiast";
		public override string Title => "Garage Sale Enthusiast";
		public override string Description => @"
			Buy everything in the garages
			";
		public override string Value() => $"{unlockedGaragesCount}";
		public override string ValueName() => "Garages Unlocked";
		public override string Target() => "4";

		private int unlockedGaragesCount;

		public GarageSaleEnthusiastAchievementListener()
		{
			float? unlockedGaragesCount = Main.saaSaveData.GetFloat($"{Id}_unlocked_garages_count");
			if (unlockedGaragesCount.HasValue)
			{
				this.unlockedGaragesCount = (int)unlockedGaragesCount.Value;
			}
			else
			{
				this.unlockedGaragesCount = 0;
			}
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToUnlockedGaragesChanged(OnUnlockedGaragesChanged);
		}

		protected override void OnUnlockedGaragesChanged(int unlockedGaragesCount)
		{
			if (unlockedGaragesCount <= this.unlockedGaragesCount)
			{
				if (IsUnlocked()) return;
			}

			this.unlockedGaragesCount = unlockedGaragesCount;
			Main.saaSaveData.SetFloat($"{Id}_unlocked_garages_count", unlockedGaragesCount);

			if (unlockedGaragesCount >= 4)
			{
				TriggerUnlock();
			}
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

		protected override void OnPlayerPositionChanged(Vector3 playerPosition)
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
		public override string Target() => "4";

		private bool hasCompletedTransport;
		private bool hasCompletedEmptytHaul;
		private bool hasCompletedShuntingLoad;
		private bool hasCompletedShuntingUnload;

		public AllRounderAchievementListener()
		{
			bool? hasCompletedTransport = Main.saaSaveData.GetBool($"{Id}_completed_transport");
			if (hasCompletedTransport.HasValue)
			{
				this.hasCompletedTransport = hasCompletedTransport.Value;
			}
			else
			{
				this.hasCompletedTransport = false;
			}

			bool? hasCompletedEmptytHaul = Main.saaSaveData.GetBool($"{Id}_completed_empty_haul");
			if (hasCompletedEmptytHaul.HasValue)
			{
				this.hasCompletedEmptytHaul = hasCompletedEmptytHaul.Value;
			}
			else
			{
				this.hasCompletedEmptytHaul = false;
			}

			bool? hasCompletedShuntingLoad = Main.saaSaveData.GetBool($"{Id}_completed_shunting_load");
			if (hasCompletedShuntingLoad.HasValue)
			{
				this.hasCompletedShuntingLoad = hasCompletedShuntingLoad.Value;
			}
			else
			{
				this.hasCompletedShuntingLoad = false;
			}

			bool? hasCompletedShuntingUnload = Main.saaSaveData.GetBool($"{Id}_completed_shunting_unload");
			if (hasCompletedShuntingUnload.HasValue)
			{
				this.hasCompletedShuntingUnload = hasCompletedShuntingUnload.Value;
			}
			else
			{
				this.hasCompletedShuntingUnload = false;
			}
		}

		protected override void SubscribeToEvents()
		{
			SubscribeToJobCompletion(OnJobCompletion);
		}

		protected override void OnJobCompletion(Job job)
		{
			if (IsUnlocked()) return;

			switch (job.jobType)
			{
				case DV.ThingTypes.JobType.Transport:
					hasCompletedTransport = true;
					Main.saaSaveData.SetBool($"{Id}_completed_transport", hasCompletedTransport);
					break;
				case DV.ThingTypes.JobType.EmptyHaul:
					hasCompletedEmptytHaul = true;
					Main.saaSaveData.SetBool($"{Id}_completed_empty_haul", hasCompletedEmptytHaul);
					break;
				case DV.ThingTypes.JobType.ShuntingLoad:
					hasCompletedShuntingLoad = true;
					Main.saaSaveData.SetBool($"{Id}_completed_shunting_load", hasCompletedShuntingLoad);
					break;
				case DV.ThingTypes.JobType.ShuntingUnload:
					hasCompletedShuntingUnload = true;
					Main.saaSaveData.SetBool($"{Id}_completed_shunting_unload", hasCompletedShuntingUnload);
					break;
			}

			if (hasCompletedTransport && hasCompletedEmptytHaul && hasCompletedShuntingLoad && hasCompletedShuntingUnload)
			{
				TriggerUnlock();
			}
		}

		private int CompletedJobTypeCount()
		{
			int completedJobTypeCount = 0;

			if (hasCompletedTransport)
				completedJobTypeCount++;
			if (hasCompletedEmptytHaul)
				completedJobTypeCount++;
			if (hasCompletedShuntingLoad)
				completedJobTypeCount++;
			if (hasCompletedShuntingUnload)
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

		protected override void OnJobCompletion(Job job)
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

		protected override void OnJobCompletion(Job job)
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

		protected override void OnJobCompletion(Job job)
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
