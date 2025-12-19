using System;
using UnityEngine;

namespace Common.Lab5_GOAP.Scripts.Actions
{
    public class PatrolAction : GoapActionBase
    {
        public float arriveDistance = 0.7f;

        public void Reset()
        {
            actionName = "Patrol (One Step)";
            cost = 2f;
            
            preMask = 0;
            addMask = GoapBits.Mask(GoapFact.PatrolStepDone);
            delMask = 0;
        }

        public override void OnEnter(GoapContext ctx)
        {
            if (ctx.PatrolWaypoints == null || ctx.PatrolWaypoints.Length == 0) return;
            
            ctx.Agent.SetDestination(ctx.PatrolWaypoints[ctx.PartrolIndex].position);
        }

        public override GoapStatus Tick(GoapContext ctx)
        {
            if (ctx.Sensors == null || ctx.Sensors.SeesPlayer) return GoapStatus.Failure;
            if (ctx.PatrolWaypoints == null || ctx.PatrolWaypoints.Length == 0) return GoapStatus.Failure;

            if (ctx.Agent.pathPending) return GoapStatus.Running;
            if (ctx.Agent.remainingDistance < arriveDistance)
            {
                ctx.PartrolIndex = (ctx.PartrolIndex + 1) % ctx.PatrolWaypoints.Length;
                return GoapStatus.Success;
            }

            return GoapStatus.Running;
        }
    }
}
