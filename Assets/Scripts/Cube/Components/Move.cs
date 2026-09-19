using Unity.Entities;
using Unity.Mathematics;

public struct Move : IComponentData
{
    public float3 Direction;
    public float Speed;
}
