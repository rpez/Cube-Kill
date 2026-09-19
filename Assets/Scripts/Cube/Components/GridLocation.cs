using Unity.Entities;
using Unity.Mathematics;

public struct GridLocation : IComponentData
{
    public int2 CurrentCell;
}
