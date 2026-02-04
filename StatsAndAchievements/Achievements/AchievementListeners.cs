using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace StatsAndAchievements.Achievements
{
	public sealed class FooAchievementListener : AchievementListener
	{
		public override string Id => "foooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo";

		protected override void OnSpeedIncreased(float speed)
		{
			if (IsUnlocked()) return;

			if (speed > 20)
			{
				TriggerUnlock();
			}
		}
	}

	public sealed class BarAchievementListener : AchievementListener
	{
		public override string Id => "bar";

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
