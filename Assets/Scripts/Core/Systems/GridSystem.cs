using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct GridSystem : ISystem
{
    private int tickCounter;
    private const int RebuildInterval = 10; // hardcoded for now

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<GridConfigSingleton>();

        state.EntityManager.CreateEntity(typeof(SpatialGridSingleton));
    }
    
    public void OnDestroy(ref SystemState state)
    {
        SpatialGridSingleton gridSingleton = SystemAPI.GetSingleton<SpatialGridSingleton>();
        if (gridSingleton.GridA.IsCreated) gridSingleton.GridA.Dispose();
        if (gridSingleton.GridB.IsCreated) gridSingleton.GridB.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        tickCounter++;
        if (tickCounter % RebuildInterval != 0) return; // Reduce update rate to improve performance and race conditions

        RefRW<SpatialGridSingleton> gridSingleton = SystemAPI.GetSingletonRW<SpatialGridSingleton>();
        bool writeToA = !gridSingleton.ValueRO.UseA;

        ref var writeGrid = ref (writeToA
            ? ref gridSingleton.ValueRW.GridA
            : ref gridSingleton.ValueRW.GridB);

        EntityQuery query = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, Team>()
            .WithNone<Dead>()
            .Build();
        int aliveCount = query.CalculateEntityCount();

        if (writeGrid.IsCreated) writeGrid.Clear();
        else writeGrid = new NativeParallelMultiHashMap<CellTeamKey, Entity>(aliveCount, Allocator.Persistent);
        NativeParallelMultiHashMap<CellTeamKey, Entity>.ParallelWriter parallelWriter = writeGrid.AsParallelWriter();

        GridConfigSingleton config = SystemAPI.GetSingleton<GridConfigSingleton>();
        BuildGridJob job = new BuildGridJob
        {
            CellSize = config.CellSize,
            GridWriter = parallelWriter
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);

        gridSingleton.ValueRW.UseA = writeToA;
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