using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ClearTarget", story: "ResetsTarget", category: "Action/Sensing", id: "81ce7a2ae2cef487d93bdb7d88ad2f3b")]
public partial class ClearTargetAction : Action
{

    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> HasLineOfSight;
    [SerializeReference] public BlackboardVariable<float> TimeSinceLastSeen;
    [SerializeReference] public BlackboardVariable<float> DistanceToTarget;
    
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if(Target == null) Target.Value = null;
        if(HasLineOfSight != null) HasLineOfSight.Value = false;
        if(TimeSinceLastSeen != null) TimeSinceLastSeen.Value = 9999f;
        if(DistanceToTarget != null) DistanceToTarget.Value = 9999f;
        
        
        return Status.Success;
    }

}

