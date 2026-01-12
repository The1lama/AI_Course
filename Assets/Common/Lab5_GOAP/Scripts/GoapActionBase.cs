using UnityEngine;
using UnityEngine.AI;

namespace Common.Lab5_GOAP.Scripts
{
    public enum GoapStatus { Running, Success, Failure }

    public class GoapContext
    {
        public NavMeshAgent Agent;
        public Transform Target;
        public Transform lastSeenTarget;
        public Transform Weapon;
        public Transform[] PatrolWaypoints;
        public GuardSensorV2 Sensors;
        public int PatrolIndex;
        public GoapAgent GoapAgent;
    }
        
    public abstract class GoapActionBase : MonoBehaviour
    {
        [Header("GOAP (planner-visible)")]
        public string actionName = "Action";

        [field: SerializeField] public ulong bit;
        public float cost = 1f;

        public ulong preMask;   // Required facts
        public ulong addMask;   // Facts to add on success
        public ulong delMask;   // Facts to delete on success
        
        public virtual bool CheckProcedural(GoapContext ctx) => true;

        public virtual void OnEnter(GoapContext ctx){}
        public abstract GoapStatus Tick(GoapContext ctx);
        public virtual void OnExit(GoapContext ctx) { }
        
        public bool CanApplyTo(GoapState s) => (s.Bits & preMask) == preMask;

        public GoapState ApplyTo(GoapState s)
        {
            ulong bits = s.Bits;
            bits &= ~delMask;
            bits |= addMask;
            return new GoapState(bits);
        }



    }
    
}
