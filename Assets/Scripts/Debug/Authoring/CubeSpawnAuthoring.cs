using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CubeSpawnAuthoring : MonoBehaviour
{
    [Header("Entity")]
    [Header("Spawner")]
    public GameObject Prefab;

    [Header("INT")]
    [Header("Spawner")]
    public int SpawnAmount;

    [Header("FLOAT")]
    [Header("Spawner")]
    public float Spread;
    public float DistanceBetweenArmies;
}

class CubeSpawnBaker : Baker<CubeSpawnAuthoring>
{
    public override void Bake(CubeSpawnAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new Spawner
        {
            Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
            Amount = authoring.SpawnAmount,
            Spread = authoring.Spread,
            DistanceBetweenArmies = authoring.DistanceBetweenArmies
        });
    }
}