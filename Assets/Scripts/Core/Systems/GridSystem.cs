using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct GridSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<GridConfig>();

        state.EntityManager.CreateEntity(typeof(SpatialGridSingleton));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        RefRW<SpatialGridSingleton> gridSingleton = SystemAPI.GetSingletonRW<SpatialGridSingleton>();

        if (gridSingleton.ValueRO.Grid.IsCreated)
            gridSingleton.ValueRW.Grid.Dispose();

        EntityQuery query = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, Team>()
            .WithNone<Dead>()
            .Build();
        int aliveCount = query.CalculateEntityCount();

        GridConfig config = SystemAPI.GetSingleton<GridConfig>();
        NativeParallelMultiHashMap<CellTeamKey, Entity> grid =
            new NativeParallelMultiHashMap<CellTeamKey, Entity>(aliveCount, Allocator.TempJob);
        NativeParallelMultiHashMap<CellTeamKey, Entity>.ParallelWriter parallelWriter = grid.AsParallelWriter();

        BuildGridJob job = new BuildGridJob
        {
            CellSize = config.CellSize,
            GridWriter = parallelWriter
        };
        
        state.Dependency = job.ScheduleParallel(query, state.Dependency);

        gridSingleton.ValueRW.Grid = grid;
    }
}

[BurstCompile]
[WithNone(typeof(Dead))]
public partial struct BuildGridJob : IJobEntity
{
    public float CellSize;
    public NativeParallelMultiHashMap<CellTeamKey, Entity>.ParallelWriter GridWriter;

    private void Execute(Entity entity, in LocalTransform transform, in Team team)
    {
        int2 cell = new int2(
            (int)math.floor(transform.Position.x / CellSize),
            (int)math.floor(transform.Position.z / CellSize)
        );

        CellTeamKey key = new CellTeamKey
        {
            Cell = cell,
            TeamID = team.TeamID
        };
        GridWriter.Add(key, entity);
    }
}

public struct CellTeamKey : IEquatable<CellTeamKey>
{
    public int2 Cell;
    public int TeamID;

    public bool Equals(CellTeamKey other) => Cell.Equals(other.Cell) && TeamID == other.TeamID;
    public override int GetHashCode() => HashCode.Combine(Cell, TeamID);
}