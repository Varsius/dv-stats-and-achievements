using DV.JObjectExtstensions;
using DV.Logic.Job;
using DV.ThingTypes;
using DV.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

	public sealed class GarageSaleEnthusiastAchievementListener : ConditionAchievementListener
	{
		public override string Id => "garage_sale_enthusiast";
		public override string Title => "Garage Sale Enthusiast";
		public override string Description => @"
			Buy everything in the garages
			";

		protected override void SubscribeToEvents()
		{
			SubscribeToUnlockedGaragesChanged(OnUnlockedGaragesChanged);
		}

		protected override void OnUnlockedGaragesChanged(int unlockedGaragesCount)
		{
			if (IsUnlocked()) return;

			if (unlockedGaragesCount >= 4)
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
		public override string Title => "Handle With Care";
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
}
