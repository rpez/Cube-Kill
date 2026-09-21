using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(CombatSystem))]
public partial struct HealthSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        Entity entity = state.EntityManager.CreateEntity(typeof(DamageQueueSingleton));
        state.EntityManager.SetComponentData(entity, new DamageQueueSingleton
        {
            DamageQueue = new NativeQueue<DamageEvent>(Allocator.Persistent)
        });
    }

    public void OnUpdate(ref SystemState state)
    {
        state.EntityManager.CompleteAllTrackedJobs();

        DamageQueueSingleton damageQueue = SystemAPI.GetSingleton<DamageQueueSingleton>();
        ComponentLookup<Health> healthLookup = SystemAPI.GetComponentLookup<Health>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        while (damageQueue.DamageQueue.TryDequeue(out DamageEvent evt))
        {
            if (evt.Target == Entity.Null) continue;
            if (!healthLookup.HasComponent(evt.Target)) continue;

            RefRW<Health> health = healthLookup.GetRefRW(evt.Target);
            health.ValueRW.CurrentHealth -= evt.PhysicalDamage;

            if (health.ValueRO.CurrentHealth <= 0f)
                ecb.AddComponent<Dead>(evt.Target);
        }

        // Add Dead components separately to avoid making structural changes during the loop
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}