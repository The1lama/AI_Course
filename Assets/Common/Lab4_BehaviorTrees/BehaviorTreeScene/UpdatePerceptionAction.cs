using System;
using Common.Lab4_BehaviorTrees.Scripts;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Update Perception", story: "Tries to see if [Self] has Line of Sight against [Target] and switches bool [HasLineOfSight] and if true adds [DistanceToTarget]", category: "Action", id: "2ef8b4569b75767787823dca9508a293")]
public partial class UpdatePerceptionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> HasLineOfSight;
    [SerializeReference] public BlackboardVariable<float> DistanceToTarget;
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
        var sensors  = Self.Value != null ? Self.Value.GetComponent<GuardSensor>() : null;
        
        if (sensors == null)
        {
            if(HasLineOfSight != null) HasLineOfSight.Value = false;
            if (TimeSinceLastSeen != null)
                TimeSinceLastSeen.Value += Time.deltaTime;
            return Status.Failure;
        }

        bool sensed = sensors.TrySeeTarget(out GameObject sensedTarget,     // instde GuardSenor
            out Vector3 sensedPos,
            out bool hasLOS,
            out float toTargetDistance
            );

        if (!sensed && !hasLOS)
        {
            if (HasLineOfSight != null) HasLineOfSight.Value = false;
            if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value += Time.deltaTime;
            return Status.Failure;
        }

        if(Target !=  null) Target.Value = sensedTarget;
        if(HasLineOfSight != null) HasLineOfSight.Value = true;
        if(TimeSinceLastSeen != null) LastKnownPosition.Value = sensedPos;
        if (TimeSinceLastSeen != null) TimeSinceLastSeen.Value = 0f;
        if (DistanceToTarget != null) DistanceToTarget.Value = toTargetDistance;
        
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

