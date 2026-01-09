using UnityEngine;

namespace Common.Lab5_GOAP.Scripts
{
    public class GuardSensor : MonoBehaviour
    {
        [Header("Target")]
        public Transform _target;

        [Header("Attack Range")]
        public float attackRange = 5;
        
        [Header("View")]
        public float viewingDistance = 10f;
        [SerializeField, Range(0f, 180f)] private float fov = 90f;
        public LayerMask obstructionLayerMask;
        
        [SerializeField] private bool isInRangeAndSeen;

        [Header("Debug"), SerializeField] private bool isDebug = true;

        private Transform cachedTarget;
        public bool SeesPlayer { get; set; }

        private void Awake()
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            cachedTarget = go != null ? go.transform : null;
        }


        private bool IsLineOfSight()
        {
            if (!Physics.Linecast(transform.position, cachedTarget.position, obstructionLayerMask) && WithinDistanceToTarget(cachedTarget.position))
            {
                Debug.DrawRay(transform.position, cachedTarget.position - transform.position, Color.green);
                return true;
            }
            Debug.DrawRay(transform.position, cachedTarget.position - transform.position, Color.red);
            return false;
        }

        private bool WithinDistanceToTarget(Vector3 targetPosition)
        {
            return (targetPosition - transform.transform.position).magnitude < viewingDistance+0.5f;
        }

        private void Update()
        {
            TrySeeTarget(out var t, out Vector3 vector3, out var _, out var __);
        }
        
        
        
        public bool TrySeeTarget(out GameObject target, out Vector3 lastKnownPosition, out bool hasLineOfSight, out float toTargetDistance)
        {
            // Check flow
            // 1. Distance
            // 2. Cone Vision
            // 3. Line of sight 
            
            
            target = null;
            lastKnownPosition = default;
            hasLineOfSight = false;
            toTargetDistance = 99999f;

            if (cachedTarget == null)
            {
                SeesPlayer = false;
                return false;
            }
            
            var toTarget = cachedTarget.position - transform.position;

            if (toTarget.magnitude > viewingDistance + 0.5f) return false;      // Gets the distance from guard to target


            // Check the cone vision from guard to target if the targets in view
            var dotProd = Vector3.Dot(transform.TransformDirection(Vector3.forward), (toTarget).normalized);
            var cosineThreshold = Mathf.Cos(fov * Mathf.Deg2Rad * 0.5f);
            isInRangeAndSeen = dotProd >= cosineThreshold;
            if (!isInRangeAndSeen)
            {
                SeesPlayer = false;
                return false;
            }

            // Check LIne of sight from guard to target 
            if (Physics.Linecast(transform.position, cachedTarget.position, obstructionLayerMask))
            {
                SeesPlayer = false;
                return false;
            }

            SeesPlayer = true;
            target = cachedTarget.gameObject;
            lastKnownPosition = cachedTarget.position;
            hasLineOfSight = true;
            toTargetDistance = toTarget.magnitude;
            return true;
        }
        
        private void OnDrawGizmos()
        {
            if (isDebug)
            {
                Gizmos.color = isInRangeAndSeen ? Color.green : Color.red;
                Gizmos.DrawWireSphere(transform.position, viewingDistance);
                
                
                Vector3 rightBoundary = Quaternion.Euler(0, fov * 0.5f, 0) * transform.TransformDirection(transform.forward);
                Vector3 leftBoundary = Quaternion.Euler(0, -fov * 0.5f, 0) * transform.TransformDirection(transform.forward);

                // gets shows wrong direction when facing -z dont care enough to fix right now.
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewingDistance);
                Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewingDistance);
                
                
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, attackRange);
            }
        }
    }
}
