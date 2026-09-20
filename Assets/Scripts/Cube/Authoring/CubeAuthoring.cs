using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CubeAuthoring : MonoBehaviour
{
    [Header("FLOAT")]
    [Header("Attack")]
    public float Damage;
    public float Range;
    public float ArmorPenetration;
    public float AttackTravelSpeed;
    [Header("Defence")]
    public float Armor;
    [Header("Health")]
    public float StartingHealth;
    [Header("Move")]
    public float Speed;

    [Header("INT")]
    [Header("Team")]
    public int TeamID;

    [Header("BOOL")]
    [Header("Attack")]
    public bool AOE;
}

class CubeBaker : Baker<CubeAuthoring>
{
    public override void Bake(CubeAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new Attack
        {
            Damage = authoring.Damage,
            Range = authoring.Range,
            ArmorPenetration = authoring.ArmorPenetration,
            AttackTravelSpeed = authoring.AttackTravelSpeed,
            AOE = authoring.AOE
        });
        AddComponent(entity, new Defence
        {
            Armor = authoring.Armor
        });
        AddComponent(entity, new Health
        {
            StartingHealth = authoring.StartingHealth,
            CurrentHealth = authoring.StartingHealth
        });
        AddComponent(entity, new Move
        {
            Direction = float3.zero,
            Speed = authoring.Speed
        });
    }
}