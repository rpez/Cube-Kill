# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Cube-Kill is a Unity DOTS/ECS auto-battler prototype: two teams of cubes spawn facing each other, acquire the nearest enemy, and close distance. Unity **6000.3.19f1**, Entities 1.4.8, Entities Graphics, URP.

## Building and running

There are no tests, assembly definitions, build scripts, or CI in this repo. Verification is manual: open `Assets/Scenes/GameScene.unity` in the Editor and enter Play mode. Runtime entities are baked from the subscene `Assets/Scenes/GameScene/SubScene-Cube.unity`.

Because everything is Editor-driven, a code change cannot be validated from the command line — compile errors and ECS safety exceptions only surface in the Editor console. Ask the user to run Play mode and report the console output rather than claiming a change works.

## Where entity components come from

Cube entities assemble their components from **two separate places**, and this split is the most common source of "the system silently does nothing" bugs:

- `CubeAuthoring` / `CubeBaker` (`Assets/Scripts/Cube/Authoring/`) bakes **authored stats** onto the prefab: `Attack`, `Defence`, `Health`, `Move` (speed only).
- `CubeSpawnSystem` (`Assets/Scripts/Debug/Systems/`) instantiates that prefab and adds **runtime state** per instance: `MaterialProperties`, `MeleeTargeting`, `Target`, and the `Team` shared component.

`IJobEntity` derives its query from the `Execute` signature, so any component named there must already exist on the entity. Adding a parameter to an `Execute` without adding the component in the baker or the spawner makes the query match nothing — no error, the job just never runs.

Other authoring: `LevelConfigAuthoring` → `GridConfigSingleton` (cell size, map bounds, precomputed min/max cell), `CubeSpawnAuthoring` → `Spawner`.

## System execution order

All gameplay runs in `FixedStepSimulationSystemGroup`:

`CubeSpawnSystem` (runs once, guarded by a `once` bool) → `GridSystem` → `TargetingSystemGroup` (`[UpdateAfter(GridSystem)]`, contains `MeleeCubeSystem`) → `MoveSystem`.

## The spatial grid and its job dependencies

`GridSystem` rebuilds the broadphase every tick: it disposes the previous `NativeParallelMultiHashMap<CellTeamKey, Entity>`, allocates a fresh `Allocator.TempJob` one sized to the alive count, and fills it with `BuildGridJob`. The map lives in `SpatialGridSingleton`. Cells are computed on the **XZ plane** (`y` ignored).

The key is `CellTeamKey { int2 Cell; int TeamID; }` — bucketing by team as well as cell is what lets targeting iterate only enemy teams instead of filtering self-hits afterward.

**Dependency management here is manual and fragile.** The grid is a raw `NativeContainer` held in a field of a singleton component, so ECS's per-component-type dependency tracking does not order jobs that touch it — `BuildGridJob` and `MeleeTargetingJob` only share *read-only* access to `LocalTransform`/`Team`, and reader-reader access never inserts a wait. Nothing automatically completes last tick's targeting job before `GridSystem` disposes the map it is still reading. `GridSystem` therefore calls `state.EntityManager.CompleteAllTrackedJobs()` before disposing — a full sync point, deliberately blunt. If you restructure grid lifetime, that safety exception (`"must call JobHandle.Complete() ... before you can deallocate"`) is what comes back.

## Targeting and movement are deliberately separate

`MeleeCubeSystem` only *decides*: it scans the 3×3 cell neighborhood (clamped to `MapCellMin`/`MapCellMax`), skips its own team, finds the nearest enemy, and writes `Target` and `Move.Direction`. `MoveSystem` only *applies*: it writes `LocalTransform` from `Move.Direction * Speed * DeltaTime`.

Do not merge these back into one job. A job cannot hold both `ref LocalTransform` (write via the query) and `ComponentLookup<LocalTransform>` (random-access read of other entities' positions) — Unity rejects it as container aliasing, since one worker could write a transform while another reads it. Targeting takes `in LocalTransform` for exactly this reason.

Note that `LocalTransform.Translate` returns a new value rather than mutating in place; the result must be assigned back, or the call is a silent no-op.

`Team` is `ISharedComponentData` (enables chunk filtering via `SetSharedComponentFilter`). Team IDs are ints; `MeleeCubeSystem` hardcodes `TeamCount = 2`.

## Not yet implemented

There is no combat, damage, or death system. `Dead` is excluded in queries (`WithNone<Dead>`) but nothing ever adds it. `Health`, `Defence`, `Attack` (including `NextAttackAvailableAt`), `CubeState`/`CubeStateModifier`, `GridLocation`, `Busy`, and `Idle` are scaffolding with no system reading or writing them yet.

`Assets/Scripts/Debug/` is test scaffolding rather than real game flow — `CubeSpawnSystem` is a one-shot debug spawner that splits cubes into teams by index parity and colors them per team.

## Conventions

Explicit types are used throughout instead of `var`. Authoring MonoBehaviours group their serialized fields by C# type with `[Header]` attributes (`FLOAT`, `INT`, `BOOL`) rather than by feature.
