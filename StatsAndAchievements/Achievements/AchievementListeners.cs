using DV.Logic.Job;
using DV.LocoRestoration;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using StatsAndAchievements.Trackers;
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

		public override string Value() => Tracker.Value();
		public override string Target() => "150 km/h";
		public override string Progress() => $"{(int)Tracker.CurrentSpeed}/150 km/h";

		private SpeedStatTracker Tracker => Main.statTrackerManager.Speed;

		protected override void SubscribeToEvents()
		{
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		private void OnTrackerChanged()
		{
			if (!IsUnlocked() && Tracker.CurrentSpeed >= 150.0f)
			{
				TriggerUnlock();
			}
		}
	}

	public sealed class MarathonAchievementListener : ProgressAchievementListener
	{
		private const float MarathonDistanceMeters = 42195f;

		public override string Id => "marathon";
		public override string Title => "Marathon";
		public override string Description => @"
			Walk a marathon distance
			";

		public override string Value() => FormatDistanceValue(Tracker.WalkingDistanceTravelled);
		public override string Target() => "42.2 km";
		public override string Progress() => $"{FormatDistanceValue(Tracker.WalkingDistanceTravelled)}/42.2 km";

		private DistanceTravelledStatTracker Tracker => Main.statTrackerManager.DistanceTravelled;

		protected override void SubscribeToEvents()
		{
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		private void OnTrackerChanged()
		{
			if (!IsUnlocked() && Tracker.WalkingDistanceTravelled >= MarathonDistanceMeters)
			{
				TriggerUnlock();
			}
		}

		// TODO: distance formatting is implement in several places.
		// This should also include meters and abstractions for very large numbers
		// The same is true for money values
		private static string FormatDistanceValue(float distanceInMeters)
		{
			return $"{distanceInMeters / 1000f:0.0}";
		}
	}

	public sealed class MaximumHonkAchievementListener : SecretConditionAchievementListener
	{
		private const float RequiredHonkSeconds = 5f;

		public override string Id => "maximum_honk";
		public override string Title => "Maximum Honk";
		public override string Description => @"
			Use the horn for a honk of at least 5 seconds
			";

		private HornAndBellTimeStatTracker Tracker => Main.statTrackerManager.HornAndBellTime;

		protected override void SubscribeToEvents()
		{
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		private void OnTrackerChanged()
		{
			if (!IsUnlocked() && Tracker.CurrentHornSessionDuration >= RequiredHonkSeconds)
			{
				TriggerUnlock();
			}
		}
	}

	public sealed class TheEndAchievementListener : ProgressAchievementListener
	{
		private static int TargetLicenseCount => SingletonBehaviour<LicenseManager>.Instance.AllLicensesCount;

		public override string Id => "the_end";
		public override string Title => "The End?";
		public override string Description => @"
			Acquire all licenses
			";
		public override string Value() => Tracker.Value();
		public override string Target() => $"{TargetLicenseCount}";

		private LicenseProgressStatTracker Tracker => Main.statTrackerManager.Licenses;

		protected override void SubscribeToEvents()
		{
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		private void OnTrackerChanged()
		{
			if (!IsUnlocked() && Tracker.CurrentCount >= TargetLicenseCount)
			{
				TriggerUnlock();
			}
		}
	}

	// This achievement is implemented as a conditional achievement, since the total amount of licenses
	// is already tracked in the vanilla stats menu
	public sealed class SteamEngineRequirementsAchievementListener : ProgressAchievementListener
	{
		private const int RequiredItemCount = 4;

		public override string Id => "steam_engine_requirements";
		public override string Title => "Fully equipped";
		public override string Description => @"
			Acquire everything you need to drive a steamer, including the license, a shovel, a lighter and oil.
		";

		public override string Value() => Tracker.Value();
		public override string Target() => $"{RequiredItemCount}";

		private SteamEngineRequirementsStatTracker Tracker => Main.statTrackerManager.SteamEngineRequirements;

		protected override void SubscribeToEvents()
		{
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		private void OnTrackerChanged()
		{
			if (!IsUnlocked() && Tracker.CurrentCount >= RequiredItemCount)
			{
				TriggerUnlock();
			}
		}
	}

	public sealed class ScroogeMcDuckAchievementListener : MilestoneAchievementListener
	{
		private float _previousValue;

		public override string Id => "scrooge_mc_duck";
		public override string Value() => Tracker.Value(); // TODO: thousand separator (see Varsius' KittyCat PR)
		protected override string BaseTitle => "Scrooge McDuck";
		protected override string MaxRankDescription => "Earn $10,000,000 throughout your career";
		protected override float[] Milestones => [100_000f, 1_000_000f, 10_000_000f];
		protected override float CurrentValue => Tracker.CurrentValue;

		private MoneyEarnedStatTracker Tracker => Main.statTrackerManager.MoneyEarned;

		protected override void SubscribeToEvents()
		{
			_previousValue = CurrentValue;
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		protected override string GetMilestoneDescription(float target)
		{
			return $"Earn ${(int)target} throughout your career";
		}

		protected override string FormatValue(float value)
		{
			return $"${(int)value}";
		}

		private void OnTrackerChanged()
		{
			float currentValue = CurrentValue;
			NotifyMilestoneProgress(_previousValue, currentValue);
			_previousValue = currentValue;
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
				List<string> remainingStations = Tracker.GetRemainingStationNames();
				if (remainingStations.Count == 0)
				{
					return "Visit all stations";
				}

				return $"Visit all stations\n\nNot visited yet: {string.Join(", ", remainingStations)}";
			}
		}

		public override string Value() => Tracker.Value();
		public override string Target() => $"{KnownStationCount()}";

		private VisitedStationsStatTracker Tracker => Main.statTrackerManager.VisitedStations;

		protected override void SubscribeToEvents()
		{
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		private void OnTrackerChanged()
		{
			if (!IsUnlocked() && KnownStationCount() > 0 && Tracker.CurrentCount >= KnownStationCount())
			{
				TriggerUnlock();
			}
		}

		private int KnownStationCount()
		{
			return Tracker.CurrentCount + Tracker.GetRemainingStationNames().Count;
		}
	}

	public sealed class CompleteTheMuseumAchievementListener : ConditionAchievementListener
	{
		public override string Id => "complete_the_museum";
		public override string Title => "Master Curator";
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
				List<string> remainingVehicles = Tracker.GetRemainingVehicleNames();
				if (remainingVehicles.Count == 0)
				{
					return "Operate every drivable vehicle";
				}

				return $"Operate every drivable vehicle\n\nNot operated yet: {string.Join(", ", remainingVehicles)}";
			}
		}

		public override string Value() => Tracker.Value();
		public override string Target() => $"{TrackableVehicleCount()}";

		private OperatedVehiclesStatTracker Tracker => Main.statTrackerManager.OperatedVehicles;

		protected override void SubscribeToEvents()
		{
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		private void OnTrackerChanged()
		{
			if (!IsUnlocked() && Tracker.CurrentCount >= TrackableVehicleCount())
			{
				TriggerUnlock();
			}
		}

		private int TrackableVehicleCount()
		{
			return Tracker.CurrentCount + Tracker.GetRemainingVehicleNames().Count;
		}
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
			Restore the paint job of any demonstrator locomotive in the museum
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
				List<string> remainingGarages = Tracker.GetRemainingGarageNames();
				if (remainingGarages.Count == 0)
				{
					return "Buy everything in the garages";
				}

				return $"Buy everything in the garages\n\nNot unlocked yet: {string.Join(", ", remainingGarages)}";
			}
		}

		public override string Value() => Tracker.Value();
		public override string Target() => $"{TrackableGarageCount()}";

		private UnlockedGaragesStatTracker Tracker => Main.statTrackerManager.UnlockedGarages;

		protected override void SubscribeToEvents()
		{
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		private void OnTrackerChanged()
		{
			if (!IsUnlocked() && Tracker.CurrentCount >= TrackableGarageCount())
			{
				TriggerUnlock();
			}
		}

		private int TrackableGarageCount()
		{
			return Tracker.CurrentCount + Tracker.GetRemainingGarageNames().Count;
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

	public sealed class GetRotatedAchievementListener : SecretConditionAchievementListener
	{
		private const float RequiredRotationDegrees = 360f;

		public override string Id => "get_rotated";
		public override string Title => "Get Rotated";
		public override string Description => @"
			Rotate a turntable by one full rotation
			";

		private TurntableRotationStatTracker Tracker => Main.statTrackerManager.TurntableRotation;

		protected override void SubscribeToEvents()
		{
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		private void OnTrackerChanged()
		{
			if (!IsUnlocked() && Tracker.TotalDegrees >= RequiredRotationDegrees)
			{
				TriggerUnlock();
			}
		}
	}

	// TODO: When we are able to detect if passenger jobs is installed alongside this mod, then conditionally include regional/express job
	public sealed class AllRounderAchievementListener : ProgressAchievementListener
	{
		private const int RequiredJobTypeCount = 4;

		public override string Id => "all_rounder";
		public override string Title => "All-Rounder";
		public override string Description
		{
			get
			{
				List<string> remainingJobTypes = GetRemainingJobTypes();
				if (remainingJobTypes.Count == 0)
				{
					return "Complete a job of each type";
				}

				return $"Complete a job of each type\n\nNot completed yet: {string.Join(", ", remainingJobTypes)}";
			}
		}

		public override string Value() => $"{CountCompletedTypes()}";
		public override string Target() => $"{RequiredJobTypeCount}";

		private CompletedJobsStatTracker Tracker => Main.statTrackerManager.CompletedJobs;

		protected override void SubscribeToEvents()
		{
			SubscribeToTrackerChanged(Tracker, OnTrackerChanged);
			OnTrackerChanged();
		}

		private void OnTrackerChanged()
		{
			if (!IsUnlocked() && CountCompletedTypes() >= RequiredJobTypeCount)
			{
				TriggerUnlock();
			}
		}

		private int CountCompletedTypes()
		{
			int count = 0;
			if (Tracker.TransportCount > 0) count++;
			if (Tracker.EmptyHaulCount > 0) count++;
			if (Tracker.ShuntingLoadCount > 0) count++;
			if (Tracker.ShuntingUnloadCount > 0) count++;
			return count;
		}

		private List<string> GetRemainingJobTypes()
		{
			List<string> remainingJobTypes = new List<string>();
			if (Tracker.TransportCount == 0) remainingJobTypes.Add("Transport");
			if (Tracker.EmptyHaulCount == 0) remainingJobTypes.Add("Empty Haul");
			if (Tracker.ShuntingLoadCount == 0) remainingJobTypes.Add("Shunting Load");
			if (Tracker.ShuntingUnloadCount == 0) remainingJobTypes.Add("Shunting Unload");
			return remainingJobTypes;
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
