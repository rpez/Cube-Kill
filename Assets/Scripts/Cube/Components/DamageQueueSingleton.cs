using Unity.Collections;
using Unity.Entities;

public struct DamageQueueSingleton : IComponentData
{
    public NativeQueue<DamageEvent> DamageQueue;
}