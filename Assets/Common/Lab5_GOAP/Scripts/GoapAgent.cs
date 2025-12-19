using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;


namespace Common.Lab5_GOAP.Scripts
{
    public class GoapAgent : MonoBehaviour
    {
        [Header("Scene refs")]
        public GuardSensor sensor;
        public Transform target;
        public Transform weaponPickup;
        public Transform[] patrolWaypoints;

        [Header("Debug")]
        public bool logPlans = true;

        [Header("Planning")] 
        public float minSecondsBetweenReplans = 0.2f;

        private float _nextAllowedReplanTime = 0f;

        private NavMeshAgent _agent;
        private GoapContext _ctx;

        private List<GoapActionBase> _allActions;
        private Queue<GoapActionBase> _plan;
        private GoapActionBase _currentAction;

        private ulong _ownedFactsBits = 0;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();

            _ctx = new GoapContext
            {
                Agent = _agent,
                Target = target,
                Weapon = weaponPickup,
                PatrolWaypoints = patrolWaypoints,
                Sensors = sensor,
                PartrolIndex = 0
            };
            
            _allActions = new List<GoapActionBase>(GetComponents<GoapActionBase>());
        }

        private void Update()
        {
            GoapState current = BuildCurrentState();
            ulong goalMask = SelectGoalMask(current);

            if ((_plan == null || _plan.Count == 0) && Time.time >= _nextAllowedReplanTime) MakePlan(current, goalMask);

            if (_plan == null || _plan.Count == 0) return;

            if (_currentAction == null)
            {
                _currentAction = _plan.Dequeue();
                if (!_currentAction.CheckProcedural(_ctx))
                {
                    InvalidatePlan(throttle: true);
                    return;
                }
                
                _currentAction.OnEnter(_ctx);
            }




            var status = _currentAction.Tick(_ctx);

            if (status == GoapStatus.Running) return;

            if (status == GoapStatus.Success)
            {
                ApplyActionEffectsToOwnedFacts(_currentAction);
                _currentAction.OnExit(_ctx);
                _currentAction = null;
                return;
            }
            
            _currentAction.OnExit(_ctx);
            _currentAction = null;
            InvalidatePlan(throttle: true);
        }

        private void ApplyActionEffectsToOwnedFacts(GoapActionBase a)
        {
            _ownedFactsBits &= ~a.delMask;
            _ownedFactsBits |= a.delMask;
        }

        private void InvalidatePlan(bool throttle)
        {
            _plan = null;
            _currentAction = null;
            
            if(throttle) _nextAllowedReplanTime = Time.time + minSecondsBetweenReplans;
            
        }

        private void MakePlan(GoapState current, ulong goalMask)
        {
            var res = GoapPlanner.Plan(current, goalMask, _allActions);
            if (res == null)
            {
                if (logPlans) Debug.LogWarning("GOAP: No plan found");
                _plan = null;
                return;
            }
            
            _plan = new Queue<GoapActionBase>(res.Actions);

            if (logPlans)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"GOAP Plan (cost {res.TotalCost:0.0}:");
                foreach (var action in res.Actions) sb.AppendLine($"-{action.actionName} (cost {action.cost:0.0}");
                Debug.Log(sb.ToString());
            }
        }

        private ulong SelectGoalMask(GoapState current)
        {
            if (current.Has(GoapFact.SeesPlayer)) return GoapBits.Mask((GoapFact.PlayerTagged));
            return GoapBits.Mask(GoapFact.PatrolStepDone);
        }

        private GoapState BuildCurrentState()
        {
            ulong bits = _ownedFactsBits;

            bool hasWeapon = (bits & GoapBits.Mask(GoapFact.HasWeapon)) != 0;

            if (sensor != null && sensor.SeesPlayer) bits |= GoapBits.Mask((GoapFact.SeesPlayer));
            else bits &= ~GoapBits.Mask(GoapFact.SeesPlayer);

            bool pickupActive = weaponPickup != null && weaponPickup.gameObject.activeInHierarchy;
            bool weaponAvailable = pickupActive && !hasWeapon;

            if (weaponAvailable) bits |= GoapBits.Mask(GoapFact.WeaponExists);
            else bits &= ~GoapBits.Mask(GoapFact.WeaponExists);

            return new GoapState(bits);

        }

        public string GetDebugString()
        {
            var s = BuildCurrentState();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Goal: {GoalMaskToString(SelectGoalMask(s))}");
            sb.AppendLine($"Current Action: {(_currentAction != null ? _currentAction.actionName : "(none)")}");
            sb.AppendLine("Facts:");
            foreach (GoapFact f in System.Enum.GetValues(typeof(GoapFact)))
                sb.AppendLine($"- {f}: {(s.Has(f) ? "true" : "false")}");
            sb.AppendLine("Plan:");
            if (_plan == null || _plan.Count == 0)
            {
                sb.AppendLine("- (none)");
            }
            else
            {
                foreach (var a in _plan)
                    sb.AppendLine($"- {a.actionName}");
            }
            return sb.ToString(); 
        }

        private string GoalMaskToString(ulong goalMask)
        {
            var sb = new System.Text.StringBuilder();
            bool first = true;
            foreach (GoapFact f in System.Enum.GetValues(typeof(GoapFact)))
            {
                ulong bit = 1UL << (int)f;
                if ((goalMask & bit) != 0)
                {
                    if (!first) sb.Append(", ");
                    sb.Append(f);
                    first = false;
                }
            }

            return first ? "(none)" : sb.ToString();
        }
    }
    
}
