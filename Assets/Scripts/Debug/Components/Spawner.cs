using Unity.Entities;

public struct Spawner : IComponentData
{
    public Entity Prefab;
    public int Amount;
    public float Spread;
    public float DistanceBetweenArmies;
}
