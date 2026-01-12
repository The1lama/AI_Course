using UnityEngine;


namespace Common.Lab5_GOAP.Scripts.Actions
{
    public class TagTargetAction : GoapActionBase
    {
        
        void Reset()
        {
            actionName = "Tag Player";
            bit = GoapBits.Mask(GoapFact.PlayerTagged);

            cost = 1f;
            preMask = GoapBits.Mask(GoapFact.HasWeapon, GoapFact.AtPlayer);
            addMask = GoapBits.Mask(GoapFact.PlayerTagged);
            delMask = 0;
        }
        
        
        public override GoapStatus Tick(GoapContext ctx)
        {
            
            if(!ctx.Sensors.SeesPlayer) return GoapStatus.Failure;
            

            ctx.Agent.SetDestination(ctx.Target.position);
            if ((ctx.Target.transform.position - transform.position).magnitude < 1f)
            {
                Debug.LogWarning("GOAP: Tagged intruder!");
                return GoapStatus.Success;
                
            }
            return GoapStatus.Running;
        }
        
        
        public override void OnExit(GoapContext ctx)
        {
            base.OnExit(ctx);
            
            if((ctx.GoapAgent._ownedFactsBits & GoapBits.Mask(GoapFact.PlayerTagged)) != 0) 
                ctx.GoapAgent._ownedFactsBits &= ~GoapBits.Mask(GoapFact.PlayerTagged);
            
        }
        
        
    }
    
}
