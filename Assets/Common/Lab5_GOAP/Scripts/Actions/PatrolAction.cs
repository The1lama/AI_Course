using System;
using UnityEngine;

namespace Common.Lab5_GOAP.Scripts.Actions
{
    public class PatrolAction : GoapActionBase
    {
        public float arriveDistance = 0.7f;
        
        
        void Reset()
        {
            actionName = "Patrol (One Step)";
            bit = GoapBits.Mask(GoapFact.PatrolStepDone);
            cost = 2f;
            
            preMask = 0;
            addMask = GoapBits.Mask(GoapFact.PatrolStepDone);
            delMask = 0;
        }

        public override void OnEnter(GoapContext ctx)
        {
            if (ctx.PatrolWaypoints == null || ctx.PatrolWaypoints.Length == 0) return;
            
            ctx.Agent.SetDestination(ctx.PatrolWaypoints[ctx.PatrolIndex].position);
        }

        public override GoapStatus Tick(GoapContext ctx)
        {
            // If the player appears, we want to stop patrolling and replan for the chase goal.
            if (ctx.Sensors != null && ctx.Sensors.SeesPlayer)
                return GoapStatus.Failure;
            if (ctx.PatrolWaypoints == null || ctx.PatrolWaypoints.Length == 0) return GoapStatus.Failure;

            if (ctx.Agent.pathPending) return GoapStatus.Running;
            if (ctx.Agent.remainingDistance <= arriveDistance)
            {
                // �Success� here means: completed ONE patrol step (reached current waypoint).
                // We increment the patrol index here, but we do NOT set a new destination, because this action is ending.
                // The next Patrol action�s OnEnter() will set the new destination for the next waypoint.
                ctx.PatrolIndex = (ctx.PatrolIndex + 1) % ctx.PatrolWaypoints.Length;
                return GoapStatus.Success;
            }

            return GoapStatus.Running;
        }

        public override void OnExit(GoapContext ctx)
        {
            base.OnExit(ctx);
            
            if((ctx.GoapAgent._ownedFactsBits & GoapBits.Mask(GoapFact.PatrolStepDone)) != 0) 
                ctx.GoapAgent._ownedFactsBits &= ~GoapBits.Mask(GoapFact.PatrolStepDone);
            
        }
    }
}
