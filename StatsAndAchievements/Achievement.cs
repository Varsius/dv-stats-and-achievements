using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatsAndAchievements
{
	public sealed class FooAchievement : AchievementListener
	{
		public override string Id => "foo";

		protected override void OnSpeedIncreased(float speed)
		{
			if (IsUnlocked) return;

			if (speed > 5)
			{
				TriggerUnlock();
			}
		}
	}
}
