using System.Collections.Generic;
using System.Linq;

namespace StatsAndAchievements.Trackers;

public sealed class StatTrackerManager
{
	public SpeedStatTracker Speed { get; }
	public LicenseProgressStatTracker Licenses { get; }
	public SteamEngineRequirementsStatTracker SteamEngineRequirements { get; }
	public MoneyEarnedStatTracker MoneyEarned { get; }
	public VisitedStationsStatTracker VisitedStations { get; }
	public OperatedVehiclesStatTracker OperatedVehicles { get; }
	public UnlockedGaragesStatTracker UnlockedGarages { get; }
	public CompletedJobTypesStatTracker CompletedJobTypes { get; }

	public IReadOnlyList<StatTracker> Trackers { get; }

	public StatTrackerManager()
	{
		Speed = new SpeedStatTracker();
		Licenses = new LicenseProgressStatTracker();
		SteamEngineRequirements = new SteamEngineRequirementsStatTracker();
		MoneyEarned = new MoneyEarnedStatTracker();
		VisitedStations = new VisitedStationsStatTracker();
		OperatedVehicles = new OperatedVehiclesStatTracker();
		UnlockedGarages = new UnlockedGaragesStatTracker();
		CompletedJobTypes = new CompletedJobTypesStatTracker();

		Trackers = new StatTracker[]
		{
			CompletedJobTypes,
			VisitedStations,
			OperatedVehicles,
			Licenses,
			SteamEngineRequirements,
			Speed,
			UnlockedGarages,
			MoneyEarned
		}
			.OrderBy(tracker => tracker.SortOrder)
			.ToList();

		foreach (StatTracker tracker in Trackers)
		{
			tracker.Attach();
		}
	}

	internal void Detach()
	{
		foreach (StatTracker tracker in Trackers)
		{
			tracker.Detach();
		}
	}
}
