using System;
using Common.Lab4_BehaviorTrees.Scripts;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update Perception", story: "For seeing [target]", category: "Action/Sensing", id: "2ef8b4569b75767787823dca9508a293")]
public partial class UpdatePerceptionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> HasLineOfSight;
    [SerializeReference] public BlackboardVariable<Vector3> LastKnownPosition;
    [SerializeReference] public BlackboardVariable<float> TimeSinceLastSeen;
    protected override Status OnStart()
    {
        if(TimeSinceLastSeen != null && TimeSinceLastSeen.Value < 0f)
            TimeSinceLastSeen.Value = 999f;
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var sensors = GameObject != null ? GameObject.GetComponent<GuardSensor>() : null;

        if (sensors == null)
        {
            if(HasLineOfSight != null) HasLineOfSight.Value = false;
            if (TimeSinceLastSeen != null)
                TimeSinceLastSeen.Value += Time.deltaTime;
            return Status.Failure;
        }

        bool sensed = sensors.TrySeeTarget(out GameObject sensedTarget,
            out Vector3 sensedPos,
            out bool hasLOS
            );

        if (sensed && hasLOS)
        {
            if(Target !=  null) Target.Value = sensedTarget;
            if(HasLineOfSight != null) HasLineOfSight.Value = true;
            if(TimeSinceLastSeen != null) LastKnownPosition.Value = sensedPos;
            if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value = 0f;
        }
        else
        {
            if(HasLineOfSight != null) HasLineOfSight.Value = false;
            if(TimeSinceLastSeen != null) TimeSinceLastSeen.Value += Time.deltaTime;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
    
    
}

