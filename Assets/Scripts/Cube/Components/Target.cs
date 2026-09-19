using Unity.Entities;
using Unity.Mathematics;

public struct Target : IComponentData
{
    public Entity CurrentTargetEntity;
    public float3 CurrentTargetPosition;
}
