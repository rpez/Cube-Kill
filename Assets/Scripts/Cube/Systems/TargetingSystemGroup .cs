using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(GridSystem))]
public partial class TargetingSystemGroup : ComponentSystemGroup {}
