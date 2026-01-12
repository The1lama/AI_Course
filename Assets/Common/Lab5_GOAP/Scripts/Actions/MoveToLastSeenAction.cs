using System;
using UnityEngine;

namespace Common.Lab5_GOAP.Scripts.Actions
{
    public class MoveToLastSeenAction : GoapActionBase
    {
        public float arriveDistance = 0.3f;
        
        void Reset()
        {
            actionName = "Move To Last Seen";
            bit = GoapBits.Mask(GoapFact.hasLastPos);
            cost = 0f;

            preMask = GoapBits.Mask(GoapFact.hasLastPos);
            addMask = GoapBits.Mask(GoapFact.AtPlayer);
            delMask = GoapBits.Mask(GoapFact.hasLastPos);
        }

        public override bool CheckProcedural(GoapContext ctx)
        {
            return ctx.Target != null;
        }

        public override GoapStatus Tick(GoapContext ctx)
        {
            
            if(ctx.Target == null) return GoapStatus.Failure;
            
            if(ctx.Sensors.lastSeenTarget == Vector3.zero) return GoapStatus.Failure;
            
            ctx.Agent.SetDestination(ctx.Sensors.lastSeenTarget);
            
            if (ctx.Agent.pathPending) return GoapStatus.Running;
            
            if (ctx.Agent.remainingDistance <= arriveDistance)
            {
                return GoapStatus.Success;
            }
            
            return GoapStatus.Running;
        }

        public override void OnExit(GoapContext ctx)
        {
            base.OnExit(ctx);
            
            ctx.Sensors.lastSeenTarget = Vector3.zero;
            
        }
    }
}

