using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class LevelConfigAuthoring : MonoBehaviour
{
    [Header("FLOAT")]
    [Header("Grid")]
    public float CellSize;

    [Header("FLOAT2")]
    [Header("Grid")]
    public float2 MapMin;
    public float2 MapMax;
}

class LevelConfigBaker : Baker<LevelConfigAuthoring>
{
    public override void Bake(LevelConfigAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new GridConfigSingleton
        {
            CellSize = authoring.CellSize,
            MapMin = authoring.MapMin,
            MapMax = authoring.MapMax,
            MapCellMin = new int2(
                (int)math.floor(authoring.MapMin[0] / authoring.CellSize),
                (int)math.floor(authoring.MapMin[1] / authoring.CellSize)),
            MapCellMax = new int2(
                (int)math.floor(authoring.MapMax[0] / authoring.CellSize),
                (int)math.floor(authoring.MapMax[1] / authoring.CellSize))
        });
    }
}