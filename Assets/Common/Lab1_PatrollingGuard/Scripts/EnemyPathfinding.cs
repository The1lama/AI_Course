using System.Collections;
using UnityEngine;
using UnityEngine.AI;


namespace EnemyStuff
{
    public enum EnemyState
    {
        Idle,
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
        public float ChageRange = 10f;
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
                case EnemyState.Idle:
                    UpdateIdle();
                    break;
                
                default:
                    Debug.LogError("Invalid state");
                    break;
            }
            
            
        }

        private void UpdateIdle()
        {
            TargetInView(0.9f);
        }


        private bool TargetInView(float degrees)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            // calculate a unit vector from the other object to this object
            Vector3 toOther = Vector3.Normalize(target.position - transform.position);
            // use the dot product sign to determine whether other is in front or behind 


            if (Vector3.Dot(forward, toOther) > degrees)
            {
                return true;
            }
            return false;
        }

        private void UpdatePatrolling()
        {
            if (distanceToTarget <= ChageRange && TargetInView(0.9f))
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
            
            if (distanceToTarget >= loseRange || !TargetInView(0.2f))
            {
                currentState = EnemyState.ReturningToPatrol;
                material.color = Color.yellow;
            }
        }

        private void UpdateReturningToPatrol()
        {
            // if the enemy sees the player and is in range
            if (distanceToTarget <= ChageRange && TargetInView(0.2f))
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
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ChageRange);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, loseRange);

         //   Vector3 angleRight = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z + 10);
         //   Vector3 angleLeft = new Vector3(transform.forward.x, transform.forward.y, transform.forward.z -10);
         //   
         //   Gizmos.color = Color.blue;
         //   Gizmos.DrawLine(transform.position, angleRight);
         //   Gizmos.DrawLine(transform.position, angleLeft);

        }
    }
}


