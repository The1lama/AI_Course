using System.Collections;
using UnityEngine;
using UnityEngine.AI;


namespace EnemyStuff
{
    public enum EnemyState
    {
        Patrolling,
        Chasing,
        ReturningToPatrol
    }
    
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyPathfinding : MonoBehaviour
    { 
        
        [Header("References")]
        public Transform target;

        [Header("Chase settings")] 
        public float ChaseRange = 10f;
        public float loseRange = 13f;
        
        private EnemyState currentState = EnemyState.Patrolling;

        private float distanceToTarget;
        
        [Header("Movement")]
        public float fSpeed = 5;
        public float fWayPointTolerance;
        public Transform[] goal;
    
        private int iCurrentWaipoint = 0;

        private NavMeshAgent agent;
        private Material material;
        
        [Header("DOT product")]
        [SerializeField, Range(0f, 180f)] float fov = 90f;
        [SerializeField] bool isInFOV;

        [SerializeField, Header("Debug")] private bool isDebug = true;
        
        
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            material = GetComponent<Renderer>().material;
            
        }

        private void Start()
        {
            SetUpAgent();
        }

        private void SetUpAgent()
        {
            if (goal.Length <= 0)
            {
                Debug.LogError("No goal found");
                return;
            }
            agent.speed = fSpeed;
            agent.autoRepath = true;
            agent.stoppingDistance = fWayPointTolerance;
            agent.destination = goal[0].position;
        }
   
        private void Update()
        {
            if (target == null)
            {
                Debug.LogError("No target found");
                return;
            }

            distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            switch (currentState)
            {
                case EnemyState.Chasing:
                    UpdateChasing();
                    break;
                case EnemyState.Patrolling:
                    UpdatePatrolling();
                    break;
                case EnemyState.ReturningToPatrol:
                    UpdateReturningToPatrol();
                    break;
                
                default:
                    Debug.LogError("Invalid state");
                    break;
            }
            
            
        }

        private bool TargetInViewDot()
        {
            var dotProd = Vector3.Dot(transform.TransformDirection(Vector3.forward), (target.position - transform.position).normalized);
            var cosineThreshold = Mathf.Cos(fov * Mathf.Deg2Rad * 0.5f);
            isInFOV = dotProd >= cosineThreshold;
            return isInFOV;
        }

        private void UpdatePatrolling()
        {
            if (distanceToTarget <= ChaseRange && TargetInViewDot())
            {
                currentState = EnemyState.Chasing;
                agent.ResetPath();
                material.color = Color.red;
                return;
            }
            
            // Check for the distance of current waypoint
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) MoveToWayPoint();

        }
        
        private void MoveToWayPoint()
        {
            iCurrentWaipoint = (iCurrentWaipoint += 1) % goal.Length;
            agent.SetDestination(goal[iCurrentWaipoint].position);
        }
        
        private void UpdateChasing()
        {
            agent.SetDestination(target.position);
            
            if (distanceToTarget >= loseRange || !TargetInViewDot())
            {
                currentState = EnemyState.ReturningToPatrol;
                material.color = Color.yellow;
            }
        }

        private void UpdateReturningToPatrol()
        {
            // if the enemy sees the player and is in range
            if (distanceToTarget <= ChaseRange && TargetInViewDot())
            {
                currentState = EnemyState.Chasing;
                agent.ResetPath();
                material.color = Color.red;
                return;
            }
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) StartCoroutine(LookAround());
        }

        private IEnumerator LookAround()
        {
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f);
            
            yield return new WaitForSeconds(0.5f);
            
            Quaternion OtherRotationTarget = Quaternion.LookRotation(Vector3.right + Vector3.right, Vector3.up);
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, OtherRotationTarget, 360f);
            
            yield return new WaitForSeconds(0.5f);

            currentState = EnemyState.Patrolling;
            material.color = Color.blue;

        }


        private void OnDrawGizmos()
        {
            if (isDebug)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, ChaseRange);
            
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, loseRange);

                // draws ray to player and changes color depening if player is in FOV
                Gizmos.color = isInFOV ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, target.position);

                Vector3 rightBoundary = Quaternion.Euler(0, fov * 0.5f, 0) * transform.TransformDirection(transform.forward);
                Vector3 leftBoundary = Quaternion.Euler(0, -fov * 0.5f, 0) * transform.TransformDirection(transform.forward);

                // gets shows wrong direction when facing -z dont care enough to fix right now.
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, transform.position + rightBoundary * ChaseRange);
                Gizmos.DrawLine(transform.position, transform.position + leftBoundary * ChaseRange);
            }
        }
    }
}


