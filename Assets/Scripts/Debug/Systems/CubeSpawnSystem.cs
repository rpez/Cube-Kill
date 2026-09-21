using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct CubeSpawnSystem : ISystem
{
    private Random random;
    private bool once;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();
        random = new Random((uint)System.DateTime.Now.Ticks);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (once) return;
        once = true;
        Spawner spawner = SystemAPI.GetSingleton<Spawner>();

        for (int i = 0; i < spawner.Amount; i++)
        {
            Entity newEntity = state.EntityManager.Instantiate(spawner.Prefab);
            float3 randomOffset = (random.NextFloat3() - 0.5f) * spawner.Spread;
            randomOffset.y = 0;
            int team = i % 2;
            state.EntityManager.SetComponentData(
                newEntity,
                LocalTransform.FromPosition(randomOffset + new float3(
                    -1f * (team * 2f - 1f) * spawner.DistanceBetweenArmies * 0.5f,
                    0f,
                    0f)));
            state.EntityManager.AddComponentData(
                newEntity,
                new MaterialProperties { BaseColor = GetColorForTeam(team) });
            state.EntityManager.AddComponentData(
                newEntity,
                new MeleeTargeting());
            state.EntityManager.AddComponentData(
                newEntity,
                new Target { CurrentTargetEntity = Entity.Null, CurrentTargetPosition = float3.zero });
            state.EntityManager.AddSharedComponent(
                newEntity,
                new Team { TeamID = team });
        }
    }

    private float4 GetColorForTeam(int teamId)
    {
        // could be a fixed palette, or something procedural (HSV hue rotation per team index)
        return teamId switch
        {
            0 => new float4(0, 0, 1, 1),
            1 => new float4(1, 0, 0, 1),
            2 => new float4(0, 1, 0, 1),
            _ => new float4(0.8f, 0.8f, 0.8f, 1)
        };
    }
}