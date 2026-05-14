using System.Collections.Generic;
namespace StatsAndAchievements.Trackers;

public sealed class StatTrackerManager
{
	public SpeedStatTracker Speed { get; }
	public LicenseProgressStatTracker Licenses { get; }
	public SteamEngineRequirementsStatTracker SteamEngineRequirements { get; }
	public TeleportsStatTracker Teleports { get; }
	public FastTravelsStatTracker FastTravels { get; }
	public DistanceTravelledStatTracker DistanceTravelled { get; }
	public MoneyEarnedStatTracker MoneyEarned { get; }
	public MoneySpentStatTracker MoneySpent { get; }
	public DamageCausedStatTracker DamageCaused { get; }
	public ServiceResourcesStatTracker ServiceResources { get; }
	public DerailmentsStatTracker Derailments { get; }
	public TurntableRotationStatTracker TurntableRotation { get; }
	public VisitedStationsStatTracker VisitedStations { get; }
	public OperatedVehiclesStatTracker OperatedVehicles { get; }
	public DistanceDrivenStatTracker DistanceDriven { get; }
	public JunctionsSwitchedStatTracker JunctionsSwitched { get; }
	public UnlockedGaragesStatTracker UnlockedGarages { get; }
	public CompletedJobsStatTracker CompletedJobs { get; }

	public IReadOnlyList<StatTracker> Trackers { get; }

	public StatTrackerManager()
	{
		Speed = new SpeedStatTracker();
		Licenses = new LicenseProgressStatTracker();
		SteamEngineRequirements = new SteamEngineRequirementsStatTracker();
		Teleports = new TeleportsStatTracker();
		FastTravels = new FastTravelsStatTracker();
		DistanceTravelled = new DistanceTravelledStatTracker();
		MoneyEarned = new MoneyEarnedStatTracker();
		MoneySpent = new MoneySpentStatTracker();
		DamageCaused = new DamageCausedStatTracker();
		ServiceResources = new ServiceResourcesStatTracker();
		Derailments = new DerailmentsStatTracker();
		TurntableRotation = new TurntableRotationStatTracker();
		VisitedStations = new VisitedStationsStatTracker();
		OperatedVehicles = new OperatedVehiclesStatTracker();
		DistanceDriven = new DistanceDrivenStatTracker();
		JunctionsSwitched = new JunctionsSwitchedStatTracker();
		UnlockedGarages = new UnlockedGaragesStatTracker();
		CompletedJobs = new CompletedJobsStatTracker();

		Trackers = new StatTracker[]
		{
			CompletedJobs,
			VisitedStations,
			OperatedVehicles,
			DistanceDriven,
			JunctionsSwitched,
			Licenses,
			SteamEngineRequirements,
			Teleports,
			FastTravels,
			DistanceTravelled,
			Speed,
			UnlockedGarages,
			MoneyEarned,
			MoneySpent,
			DamageCaused,
			ServiceResources,
			Derailments,
			TurntableRotation
		};

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
