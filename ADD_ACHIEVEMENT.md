# Adding a New Achievement

## Goal
This file explains the standard process for adding a new achievement to this mod. Follow these steps so new achievements stay consistent with the existing codebase.

## 1. Choose the achievement type
- Use `ProgressAchievementListener` if the achievement tracks measurable progress.
- Use `ConditionAchievementListener` if it unlocks from a single condition.
- Use `SecretConditionAchievementListener` if title/description should stay hidden until unlocked.

Add the implementation in `StatsAndAchievements/Achievements/AchievementListeners.cs`.

## 2. Implement the listener
- Use a sealed class named `SomethingAchievementListener`.
- Add `Id`, `Title`, `Description`, and the required progress methods if applicable.
- Persist progress in `Main.saaSaveData` using stable keys like ``$"{Id}_..."``.
- Keep logic simple and event-driven where possible.
- Prefer existing game events or existing mod event hooks before adding new ones.

For progress achievements, follow the existing pattern:
- `Value()` = current progress
- `ValueName()` = label shown in stats
- `Target()` = goal
- Override `Progress()` only if default formatting is poor

## 3. Hook the game state
- Reuse existing subscriptions in `AchievementListener.cs` when possible.
- If a new event bridge is needed, add it only if direct game subscription would be worse.
- For world-dependent achievements, rely on initialization after world loading, not in `Main.Load()`.

## 4. Register the achievement
Add the new listener to the appropriate category in `StatsAndAchievements/Patches/WorldStreamingInitPatch.cs`.

## 5. Follow coding conventions
- Use tabs for indentation.
- Keep classes small and focused.
- Use `PascalCase` for types and methods.
- Use `_camelCase` for private fields.
- Avoid unnecessary abstraction and avoid unrelated cleanup.
- Use direct, stable checks over fragile string heuristics when game types are available.

## 6. Build and verify
Run:

```bash
dotnet build StatsAndAchievements.sln
```

Then do a quick in-game smoke test for the achievement trigger, UI text, and save persistence.

## 7. Update achievement documentation
Mark the corresponding entry in `ACHIEVEMENTS.md` as implemented and untested by appending `✅ (untested)` to the heading or entry as appropriate.

Do not skip this step.
