using Unity.Entities;
using Unity.Mathematics;

public struct GridConfig : IComponentData
{
    public float CellSize;
    public float2 MapMin;
    public float2 MapMax;
}
