using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[MaterialProperty("_BaseColor")]
public struct MaterialProperties : IComponentData
{
    public float4 BaseColor;
}
