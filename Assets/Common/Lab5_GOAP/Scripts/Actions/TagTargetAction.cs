using UnityEngine;


namespace Common.Lab5_GOAP.Scripts.Actions
{
    public class TagTargetAction : GoapActionBase
    {
        void Reset()
        {
            actionName = "Tag Player";
            cost = 1f;
            preMask = GoapBits.Mask(GoapFact.HasWeapon, GoapFact.AtPlayer);
            addMask = GoapBits.Mask(GoapFact.PlayerTagged);
            delMask = 0;
        }
        
        
        public override GoapStatus Tick(GoapContext ctx)
        {
            Debug.Log("GOAP: Tagged intruder!");
            return GoapStatus.Success;
        }
    }
    
}
