using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(TargetingSystemGroup))]
public partial struct MeleeCubeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        UpdateGridLocation();

        // with a filter, e.g. only alive TeamA cubes
        
        // int aliveCount = query.CalculateEntityCount();

        // TryGetTargetEntity(ref state);

        GetTargetPosition();
    }

    private void UpdateGridLocation()
    {

    }

    private Entity? TryGetTargetEntity(ref SystemState state, int ownTeam)
    {
        Entity? candidate = null;
        EntityQuery query = SystemAPI.QueryBuilder().WithNone<Dead>().Build();
        query.SetSharedComponentFilter(new Team { TeamID = ownTeam });

        return candidate;
    }

    private float3 GetTargetPosition()
    {
        return float3.zero;
    }
}
