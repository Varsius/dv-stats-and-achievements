# Repository Guidelines

- Use tabs for indentation.
- Keep achievement logic simple and event-driven.
- Before committing to git, make sure all text file line endings are CRLF.
- Prefer direct game APIs and existing hooks over adding new event bridges or abstraction layers.
- Initialize world-dependent achievement listeners after world loading in `StatsAndAchievements/Patches/WorldStreamingInitPatch.cs`, not in `Main.Load()`.
- Build with `dotnet build StatsAndAchievements.sln`.
- For the full achievement implementation workflow, see `@ADD_ACHIEVEMENT.md`.
