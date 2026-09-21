using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(MoveSystem))]
public partial struct CombatSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattleTimerSingleton>();
        state.RequireForUpdate<DamageQueueSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        BattleTimerSingleton timer = SystemAPI.GetSingleton<BattleTimerSingleton>();

        RefRW<DamageQueueSingleton> damageQueue = SystemAPI.GetSingletonRW<DamageQueueSingleton>();
        NativeQueue<DamageEvent>.ParallelWriter parallelWriter =
            damageQueue.ValueRW.DamageQueue.AsParallelWriter();

        AttackJob job = new AttackJob
        {
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            DamageEventWriter = parallelWriter,
            BattleTimer = timer
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithNone(typeof(Dead))]
partial struct AttackJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
    [ReadOnly] public BattleTimerSingleton BattleTimer;
    public NativeQueue<DamageEvent>.ParallelWriter DamageEventWriter;
    

    private void Execute(ref Attack attack, in Target target, in LocalTransform transform)
    {
        if (BattleTimer.CurrentTick < attack.NextAttackAvailableAt) return;

        bool targetInvalid = target.CurrentTargetEntity == Entity.Null
            || !TransformLookup.HasComponent(target.CurrentTargetEntity);

        if (targetInvalid) return;
        if (math.distancesq(TransformLookup[target.CurrentTargetEntity].Position, transform.Position)
            > attack.Range * attack.Range) return;

        DamageEventWriter.Enqueue(new DamageEvent
        {
            Target = target.CurrentTargetEntity,
            PhysicalDamage = attack.Damage
        });

        attack.NextAttackAvailableAt = BattleTimer.CurrentTick + attack.AttackCooldown;
    }
}

public struct DamageEvent
{
    public Entity Target;
    public float PhysicalDamage;
}