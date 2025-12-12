using System;
using UnityEngine;
using System.Collections.Generic;
namespace Common.Lab3_Steering_Swarm.Scripts.AI
{
    public class SteeringAgent : MonoBehaviour
    {
        #region Variables

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
        
        [Header("Weights")]
        public float arriveWeight = 1f;
        public float separationWeight = 1f;
        public float cohesionWeight = 0.4f;
        public float alignmentWeight = 1f;
        
        [Header("Debug")]
        public bool drawDebug = true;

        private Vector3 velocity = Vector3.zero;
        
        private Vector3 avgVelocity = Vector3.zero;
        
        [Header("Optional")]
        public Transform target;

        public static List<SteeringAgent> allAgents = new();
        

        #endregion

        private void OnEnable()
        {
            allAgents.Add(this);
        }

        private void OnDisable()
        {
            allAgents.Remove(this);
        }

        private void Update()
        {
            Vector3 steering = Vector3.zero;
            
            // TODO in Part B/C
            if (target != null)
            {
                //steering = Seek(target.position);
                steering = Arrive(target.position, slowingRadius) * arriveWeight;
            }

            if (allAgents.Count > 1)
            {
                steering += Separation(separationRadius, separationStrength) *  separationWeight;
                
                steering += Cohesion(cohesionRadius, cohesionStrength) * cohesionWeight;
                
                steering += Alignment(alignmentRadius, alignmentStrength) *  alignmentWeight;
                
            }
            
            
            // Limit Steering (Truncate)
            steering = Vector3.ClampMagnitude(steering, maxForce);
            
            // Apply Steering to Velocity
            // Acceleration = Force / Mass. (We assume Mass = 1)
            // Velocity Change = Acceleration * Time.
            velocity += steering * Time.deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, maxForce);
            
            // Move Agent
            transform.position += velocity * Time.deltaTime;
            
            // Face Movement Direction
            if (velocity.sqrMagnitude > 0.0001f)
                transform.forward = velocity.normalized;
        }


        /// <summary>
        /// Advances to target without slowing down
        /// </summary>
        /// <param name="targetPos">To Target Position</param>
        /// <returns>Direction Force</returns>
        public Vector3 Seek(Vector3 targetPos)
        {
            var directionForce = targetPos - transform.position;
            if(directionForce.sqrMagnitude < 0.01f) return Vector3.zero;
            return (directionForce.normalized * maxSpeed) - velocity;
        }

        /// <summary>
        /// Advances to target with braking in mind, so it slows down before reaching target
        /// </summary>
        /// <param name="targetPos">To Target Position</param>
        /// <param name="slowRadius">The radius before starting to slow down</param>
        /// <returns>Direction force</returns>
        public Vector3 Arrive(Vector3 targetPos, float slowRadius)
        {
            var distanceToTarget = targetPos - transform.position;
            var fDistance = distanceToTarget.magnitude;
            if(fDistance < 0.01f) return Vector3.zero;

            var desSpeed = maxSpeed;
            
            if (fDistance < slowRadius)
            {
                desSpeed = maxSpeed * (fDistance / slowRadius);
            }
            
            return distanceToTarget.normalized * desSpeed - velocity;
        }

        /// <summary>
        /// Check neighbours and avoids them
        /// </summary>
        /// <param name="radius">The radius to keep the distance</param>
        /// <param name="strength">How hard the veering is</param>
        /// <returns>Direction Force</returns>
        public Vector3 Separation(float radius, float strength)
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
                force = force - velocity;
                force *= strength;
            }
            return force;
        }

        public Vector3 Cohesion(float radius, float strength)
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
                force = force - velocity;
                force *= strength;
            }
            return force;
        }
        
        private Vector3 Alignment(float radius, float strength)
        {
            Vector3 avgForce = Vector3.zero;
            int neighbourCount = 0;

            foreach (var other in allAgents)
            {
                if(other == this) continue;
                
                var toMe = transform.position - other.transform.position;
                var otherMagnitude = toMe.magnitude;


                if (otherMagnitude > 0f && otherMagnitude < radius) // within range
                {
                    avgForce += toMe.normalized / otherMagnitude;
                    neighbourCount++;
                }
            }

            if (neighbourCount > 0)
            {
                avgForce /= neighbourCount;
            
                avgForce = Seek(avgForce) * maxSpeed;
                avgForce = avgForce - velocity;
                avgForce *= strength;
                avgVelocity =  avgForce.normalized * maxSpeed;
            }
            return avgForce.normalized * maxSpeed;

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
            Gizmos.DrawLine(transform.position, transform.position + velocity);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + avgVelocity);
        }
    }
}
