using Unity.Collections;
using Unity.Entities;

public struct SpatialGridSingleton : IComponentData
{
    public NativeParallelMultiHashMap<CellTeamKey, Entity> GridA;
    public NativeParallelMultiHashMap<CellTeamKey, Entity> GridB;
    public bool UseA;
}