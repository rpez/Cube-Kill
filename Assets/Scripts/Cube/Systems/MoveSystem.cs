using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(TargetingSystemGroup))]
public partial struct MoveSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {
        MoveJob job = new MoveJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime
        };

        state.Dependency = job.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithNone(typeof(Dead))]
partial struct MoveJob : IJobEntity
{
    public float DeltaTime;

    void Execute(in Move move, ref LocalTransform transform)
    {
        transform = transform.Translate(move.Direction * move.Speed * DeltaTime);
    }
}