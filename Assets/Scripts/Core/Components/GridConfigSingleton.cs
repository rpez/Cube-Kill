using Unity.Entities;
using Unity.Mathematics;

public struct GridConfigSingleton : IComponentData
{
    public float CellSize;
    public float2 MapMin;
    public float2 MapMax;
    public int2 MapCellMin;
    public int2 MapCellMax;
}
