using Unity.Entities;

public struct Health : IComponentData
{
    public float StartingHealth;
    public float CurrentHealth;
}
