using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(TargetingSystemGroup))]
public partial struct MeleeCubeSystem : ISystem
{
    private const int MeleeRetargetInterval = 15; // hardcoded for now

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpatialGridSingleton>();
        state.RequireForUpdate<GridConfigSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        SpatialGridSingleton gridSingleton = SystemAPI.GetSingleton<SpatialGridSingleton>();
        GridConfigSingleton gridConfig = SystemAPI.GetSingleton<GridConfigSingleton>();
        BattleTimerSingleton tick = SystemAPI.GetSingleton<BattleTimerSingleton>();

        ref var gridSnapshot = ref (gridSingleton.UseA
            ? ref gridSingleton.GridA
            : ref gridSingleton.GridB);
        if (!gridSnapshot.IsCreated) return;

        MeleeTargetingJob job = new MeleeTargetingJob
        {
            Grid = gridSnapshot,
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            CellSize = gridConfig.CellSize,
            MapCellMin = gridConfig.MapCellMin,
            MapCellMax = gridConfig.MapCellMax,
            TeamCount = 2, // hardcoded for now
            CurrentTick = tick.CurrentTick,
            RetargetInterval = MeleeRetargetInterval
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(MeleeTargeting))]
[WithNone(typeof(Dead))]
partial struct MeleeTargetingJob : IJobEntity
{
    [ReadOnly] public NativeParallelMultiHashMap<CellTeamKey, Entity> Grid;
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
    public float CellSize;
    public int2 MapCellMin;
    public int2 MapCellMax;
    public int TeamCount;
    public int CurrentTick;
    public int RetargetInterval;

    void Execute(Entity entity, ref Target target, ref Move move, in LocalTransform transform, in Team team)
    {
        bool targetInvalid = target.CurrentTargetEntity == Entity.Null
            || !TransformLookup.HasComponent(target.CurrentTargetEntity);

        // Batch the cube updates, offset using entity index
        bool isMyRetargetTick = (CurrentTick + entity.Index) % RetargetInterval == 0;

        if (!targetInvalid && !isMyRetargetTick) return; 

        int2 ownCell = new int2(
            (int)math.floor(transform.Position.x / CellSize),
            (int)math.floor(transform.Position.z / CellSize)
        );

        int xMin = math.max(MapCellMin.x, ownCell.x - 1);
        int yMin = math.max(MapCellMin.y, ownCell.y - 1);
        int xMax = math.min(MapCellMax.x, ownCell.x + 1);
        int yMax = math.min(MapCellMax.y, ownCell.y + 1);

        Entity nearest = Entity.Null;
        float nearestDistSq = float.MaxValue;

        for (int t = 0; t < TeamCount; t++)
        {
            if (t == team.TeamID) continue; // skip own team

            for (int x = xMin; x <= xMax; x++)
                for (int y = yMin; y <= yMax; y++)
                {
                    CellTeamKey key = new CellTeamKey { Cell = new int2(x, y), TeamID = t };

                    if (Grid.TryGetFirstValue(key, out Entity candidate, out var iterator))
                    {
                        do
                        {
                            float distSq = math.distancesq(transform.Position, TransformLookup[candidate].Position);
                            if (distSq < nearestDistSq)
                            {
                                nearest = candidate;
                                nearestDistSq = distSq;
                            }
                        } while (Grid.TryGetNextValue(out candidate, ref iterator));
                    }
                }
        }

        target.CurrentTargetEntity = nearest;
        if (nearest == Entity.Null)
        {
            target.CurrentTargetPosition = transform.Position;
            move.Direction = float3.zero;
        }
        else
        {
            target.CurrentTargetPosition = TransformLookup[nearest].Position;
            move.Direction = math.normalize(target.CurrentTargetPosition - transform.Position);
        }
    }
}