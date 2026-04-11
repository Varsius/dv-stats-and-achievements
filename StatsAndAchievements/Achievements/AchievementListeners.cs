using DV.JObjectExtstensions;
using DV.Logic.Job;
using DV.Utils;
using System;
using System.Collections;
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
			if (maxSpeed.HasValue) {
				this.maxSpeed = maxSpeed.Value;
			} else
			{
				this.maxSpeed = 0.0f;
			}
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

	public sealed class MaximumHonkAchievementListener : ConditionAchievementListener
	{
		public override string Id => "maximum_honk";
		public override string Title => "Maximum Honk";
		public override string Description => @"
			Use the horn for a honk of at least 5 seconds
			";

		private float _honkStartedAt = 0.0f;

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

	public sealed class AllRounderAchievementListener : ConditionAchievementListener
	{
		public override string Id => "all_rounder";
		public override string Title => "All-Rounder";
		public override string Description => @"
			Complete a job of each type
			";

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

		protected override void OnJobCompletion(Job job)
		{
			if (IsUnlocked()) return;

			switch (job.jobType) {
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
	}
}
