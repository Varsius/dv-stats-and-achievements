using DV.JObjectExtstensions;
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
}
