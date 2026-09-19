using Unity.Entities;

[System.Flags]
public enum CubeStateModifier : ushort
{
    None = 0,
    Rooted = 1 << 1,
    Disarmed = 1 << 2,
    AirBorne = 1 << 3,
    Invulnerable = 1 << 4,
    Immovable = 1 << 5,
}

public struct CubeState : IComponentData
{
    public CubeStateModifier Modifier;
}
