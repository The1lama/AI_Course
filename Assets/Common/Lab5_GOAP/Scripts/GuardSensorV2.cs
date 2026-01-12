using System;
using UnityEngine;

namespace Common.Lab5_GOAP.Scripts
{
    public class GuardSensorV2 : MonoBehaviour
    {

        public Transform player;
        public float viewRange = 10f;
        public LayerMask occluders = ~0; // everything by default
        public bool useLineOfSightRaycast = true;
        public bool SeesPlayer { get; private set; }
        public Vector3 lastSeenTarget =  Vector3.zero;

        // Update is called once per frame
        void Update()
        {
            SeesPlayer = false;
            if (player == null)
            {
                return;
            }
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > viewRange)
            {
                return;
            }
            if (!useLineOfSightRaycast)
            {
                SeesPlayer = true;
                lastSeenTarget =  player.position;
                return;
            }
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 target = player.position + Vector3.up * 0.5f;
            Vector3 dir = (target - origin);
            float len = dir.magnitude;
            
            // Used to ignore LOS as if the target it to close to not guard 
            if (len <= 1.4f)
            {
                SeesPlayer = true; 
                lastSeenTarget = player.position;
                return;
            }

            if (Physics.Raycast(origin, dir / len, out RaycastHit hit, len, occluders))
            {
                Debug.DrawLine(origin, hit.point, Color.red);
                // Sees player only if the first thing hit is the player
                if (hit.transform == player)
                {
                    SeesPlayer = true;
                    lastSeenTarget = player.position;
                }
            }
            else
            {
                Debug.DrawRay(origin, dir / len, Color.red);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, viewRange);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, viewRange);
            Gizmos.DrawWireSphere(transform.position, 1.4f);
            
        }
    }

}
