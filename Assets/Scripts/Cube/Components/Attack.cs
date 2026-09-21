using Unity.Entities;

public struct Attack : IComponentData
{
    public float Damage;
    public float Range;
    public float ArmorPenetration;
    public float AttackTravelSpeed;
    public float AttackCooldown;
    public float NextAttackAvailableAt;
    public bool AOE;
}
