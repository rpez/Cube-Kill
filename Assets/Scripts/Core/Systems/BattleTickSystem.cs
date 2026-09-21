using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct BattleTickSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateEntity(typeof(BattleTimerSingleton));
    }

    public void OnUpdate(ref SystemState state)
    {
        RefRW<BattleTimerSingleton> tickSingleton = SystemAPI.GetSingletonRW<BattleTimerSingleton>();
        tickSingleton.ValueRW.CurrentTick++;
    }
}