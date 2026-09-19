using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(CubeSpawnAuthoring))]
public partial struct CubeSpawnSystem : ISystem
{
    private Random random;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();
        random = new Random((uint)System.DateTime.Now.Ticks);

        Spawner spawner = SystemAPI.GetSingleton<Spawner>();

        for (int i = 0; i < spawner.Amount; i++)
        {
            Entity newEntity = state.EntityManager.Instantiate(spawner.Prefab);
            float3 randomOffset = (random.NextFloat3() - 0.5f) * spawner.Spread;
            randomOffset.y = 0;
            state.EntityManager.SetComponentData(
                newEntity,
                LocalTransform.FromPosition(randomOffset + new float3(
                    -1f * i % 2 * spawner.DistanceBetweenArmies * 0.5f,
                    0.5f,
                    0f)));
            state.EntityManager.SetSharedComponent(
                newEntity,
                new Team { TeamID = i % 2 });
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        
    }
}