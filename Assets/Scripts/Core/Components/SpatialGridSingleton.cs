using Unity.Collections;
using Unity.Entities;

public struct SpatialGridSingleton : IComponentData
{
    public NativeParallelMultiHashMap<CellTeamKey, Entity> Grid;
}