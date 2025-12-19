using System;
using UnityEngine;

namespace Common.Lab5_GOAP.Scripts.Actions
{
    public class MoveToTargetAction : GoapActionBase
    {
        public float distanceTo = 1.7f;
        
        public void Reset()
        {
            actionName = "Move To Target";
            cost = 1f;

            preMask = GoapBits.Mask(GoapFact.SeesPlayer, GoapFact.HasWeapon);
            addMask = GoapBits.Mask(GoapFact.AtPlayer);
            delMask = 0;
        }

        public override bool CheckProcedural(GoapContext ctx)
        {
            return ctx.Target != null;
        }

        public override GoapStatus Tick(GoapContext ctx)
        {
            
            if(ctx.Target == null) return GoapStatus.Failure;
            
            if(ctx.Sensors != null && !ctx.Sensors.SeesPlayer) return GoapStatus.Failure;

            ctx.Agent.SetDestination(ctx.Target.position);

            if (ctx.Agent.pathPending) return GoapStatus.Running;
            
            if(ctx.Agent.remainingDistance < distanceTo) return GoapStatus.Success;

            return GoapStatus.Running;
        }
    }
    
}

