using UnityEngine;
using System.Collections.Generic;
namespace Common.Lab3_Steering_Swarm.Scripts.AI
{
    public class SteeringAgent : MonoBehaviour
    {
        #region Variables

        [Header("Scriptable Object")]
        public SteeringSO steeringSo;
        
        [Header("Movement")] 
        public float maxSpeed = 5f;
        public float maxForce = 10f;

        [Header("Arrive")] 
        public float slowingRadius = 3f;

        [Header("Separation")] 
        public float separationRadius = 1.5f;
        public float separationStrength = 5f;
        
        [Header("Cohesion")]
        public float cohesionRadius = 5f;
        public float cohesionStrength = 0.17f;
        
        [Header("Alignment")]
        public float alignmentRadius = 3f;
        public float alignmentStrength = 2f;
        
        [Header("Obstacle Avoidance")]
        public float obstacleAvoidanceRadius = 5f;
        public float obstacleAvoidanceStrength = 10f;
        public float lookAheadDistance = 3f;
        public LayerMask obstacleLayerMask = 0;

        [Header("Ground Following")]
        public float groundCheck = 3f;
        public float hoverHeight = 0.5f;
        public LayerMask groundLayerMask = 0;
        
        
        [Header("Weights")]
        public float arriveWeight = 1f;
        public float separationWeight = 1f;
        public float cohesionWeight = 0.4f;
        public float alignmentWeight = 1f;
        public float avoidanceWeight = 1f;
        
        [Header("Use diff things")]
        public bool separation = true;
        public bool cohesion = true;
        public bool alignment = true;
        public bool avoidance = true;
        
        
        [Header("Debug")]
        public bool drawDebug = true;
        private Vector3 _velocity = Vector3.zero;
        
        
        [Header("Optional")]
        public Transform target;

        private static List<SteeringAgent> allAgents = new();
        

        #endregion

        private void OnEnable()
        {
            if(!allAgents.Contains(this))
                allAgents.Add(this);
            target = GameObject.Find("Target").transform;
            Initialize();
        }

        private void OnDisable()
        {
            allAgents.Remove(this);
        }

        private void Initialize()
        {
            if (steeringSo == null) return;
            
            // Weight
            arriveWeight = steeringSo.arriveWeight;
            separationWeight = steeringSo.separationWeight;
            cohesionWeight = steeringSo.cohesionWeight;
            alignmentWeight = steeringSo.alignmentWeight;
            avoidanceWeight = steeringSo.avoidanceWeight;
            
            // Use things
            separation = steeringSo.separation;
            cohesion = steeringSo.cohesion;
            alignment = steeringSo.alignment;
            avoidance = steeringSo.avoidance;
            
            // Debug
            drawDebug = steeringSo.drawDebug;
        }
        
        
        private void Update()
        {
            Initialize();
            
            Vector3 steering = Vector3.zero;
            steering = ObstacleAvoidance() * avoidanceWeight;
            
            // TODO in Part B/C
            if (target != null)
            {
                //steering = Seek(target.position);
                steering = Arrive(target.position, slowingRadius) * arriveWeight;
            }

            
            if (allAgents.Count > 1)
            {
                if(separation)
                    steering += Separation(separationRadius, separationStrength) *  separationWeight;
                
                if(cohesion)
                    steering += Cohesion(cohesionRadius, cohesionStrength) * cohesionWeight;
                
                if(alignment)
                    steering += Alignment(alignmentRadius, alignmentStrength) *  alignmentWeight;
                
            }
            
            // Limit Steering (Truncate)
            steering = Vector3.ClampMagnitude(steering, maxForce);
            
            // Apply Steering to Velocity
            // Acceleration = Force / Mass. (We assume Mass = 1)
            // Velocity Change = Acceleration * Time.
            _velocity += steering * Time.deltaTime;
            _velocity = Vector3.ClampMagnitude(_velocity, maxForce);
            
            // Move Agent
            transform.position += _velocity * Time.deltaTime;
            
            // Face Movement Direction
            if (_velocity.sqrMagnitude > 0.0001f)
                transform.forward = _velocity.normalized;
        }



        /// <summary>
        /// Advances to target without slowing down
        /// </summary>
        /// <param name="targetPos">To Target Position</param>
        /// <returns>Direction Force</returns>
        private Vector3 Seek(Vector3 targetPos)
        {
            var directionForce = targetPos - transform.position;
            if(directionForce.sqrMagnitude < 0.01f) return Vector3.zero;
            return (directionForce.normalized * maxSpeed) - _velocity;
        }

        /// <summary>
        /// Advances to target with braking in mind, so it slows down before reaching target
        /// </summary>
        /// <param name="targetPos">To Target Position</param>
        /// <param name="slowRadius">The radius before starting to slow down</param>
        /// <returns>Direction force</returns>
        private Vector3 Arrive(Vector3 targetPos, float slowRadius)
        {
            var distanceToTarget = targetPos - transform.position;
            var fDistance = distanceToTarget.magnitude;
            if(fDistance < 0.01f) return Vector3.zero;

            var desSpeed = maxSpeed;
            
            if (fDistance < slowRadius)
            {
                desSpeed = maxSpeed * (fDistance / slowRadius);
            }
            
            return distanceToTarget.normalized * desSpeed - _velocity;
        }

        /// <summary>
        /// Check neighbours and avoids them
        /// </summary>
        /// <param name="radius">The radius to keep the distance</param>
        /// <param name="strength">How hard the veering is</param>
        /// <returns>Direction Force</returns>
        private Vector3 Separation(float radius, float strength)    // Avoid distance from group
        {
            Vector3 force = Vector3.zero;
            int neighbourCount = 0;

            foreach (var other in allAgents)
            {
                if(other == this) continue;
                
                var toMe = transform.position - other.transform.position;
                var otherMagnitude = toMe.magnitude;


                if (otherMagnitude > 0f && otherMagnitude < radius)
                {
                    force += toMe.normalized / otherMagnitude;
                    neighbourCount++;
                }
            }

            if (neighbourCount > 0)
            {
                force /= neighbourCount;
            
                force = force.normalized * maxSpeed;
                force = force - _velocity;
                force *= strength;
            }
            return force;
        }

        private Vector3 Cohesion(float radius, float strength)       // tightness of the group 
        {
            Vector3 force = Vector3.zero;
            Vector3 avgPosition =  Vector3.zero;
            int neighbourCount = 0;

            foreach (var other in allAgents)
            {
                if(other == this) continue;
                
                var toMe = transform.position - other.transform.position;
                var otherMagnitude = toMe.magnitude;


                if (otherMagnitude > 0f && otherMagnitude < radius) // within range
                {
                    avgPosition += other.transform.position;
                    force += toMe.normalized / otherMagnitude;
                    neighbourCount++;
                }
            }

            if (neighbourCount > 0)
            {
                avgPosition /= neighbourCount;
            
                force = Seek(avgPosition) * maxSpeed;
                force = force - _velocity;
                force *= strength;
            }
            return force;
        }
        
        private Vector3 Alignment(float radius, float strength)     // the avg forward position
        {
            Vector3 avgDirection = Vector3.zero;
            int neighbourCount = 0;

            foreach (var other in allAgents)
            {
                if(other == this) continue;
                
                var toMe = transform.position - other.transform.position;
                var otherMagnitude = toMe.magnitude;

                if (otherMagnitude > 0f && otherMagnitude < radius) // within range
                {
                    avgDirection += other.transform.forward;    // stores all the forward positions from all the nodes within range
                    neighbourCount++;
                }
            }

            if (neighbourCount <= 0) return avgDirection;
            
            avgDirection /= neighbourCount;                     // takes the avg direction of all the close neighbours
            
            //avgDirection = Seek(avgDirection) * maxSpeed;
            avgDirection = avgDirection - _velocity;
            avgDirection *= strength;
            return avgDirection * maxSpeed;
        }
        
        private Vector3 ObstacleAvoidance()
        {
            throw new System.NotImplementedException();
        }
        
        
       // public Vector3 Separation(float radius, float strength)
       // {
       //     Vector3 force = Vector3.zero;
       //     int neighbourCount = 0;

       //     foreach (var otherAgent in allAgents)
       //     {
       //         if(otherAgent == this) continue;
       //         
       //         var otherDistance = otherAgent.transform.position - transform.position;
       //         var otherMagnitude = otherDistance.magnitude;
       //         if(otherMagnitude > 0f && otherDistance.sqrMagnitude > radius) continue;
       //        
       //         // if changing to += it flocks the group
       //         force -= otherDistance.normalized* strength;
       //         
       //         
       //         neighbourCount++;
       //     }
       //     
       //     if(neighbourCount == 0) return Vector3.zero;
       //     
       //     return force;
       // }
        private void OnDrawGizmos()
        {
            if (!drawDebug) return;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + _velocity);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, slowingRadius);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, separationRadius);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, cohesionRadius);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, alignmentRadius);
            
            
            


        }
    }
}
