using UnityEngine;

namespace Common.Lab4_BehaviorTrees.Scripts
{
    public class GuardSensor : MonoBehaviour
    {
        [Header("Eyes")] 
        [SerializeField] private Transform eye;
        [SerializeField] private GameObject target;
        
        [Header("Senses")]
        public float viewingDistance = 10f;
        public LayerMask obstructionLayerMask;
        
        [Header("DOT product")]
        [SerializeField, Range(0f, 180f)] private float fov = 90f;
        [SerializeField] private bool isInFOV;

        [Header("Debug"), SerializeField] private bool isDebug = true;

        [Header("Debugging shit")]
        public bool isInRangeAndView = false;

        private void Update()
        {
            isInFOV = TargetInViewDot(target.transform.position);
        }
        
        
        public bool TargetInViewDot(Vector3 targetPos)
        {
            var dotProd = Vector3.Dot(eye.transform.TransformDirection(Vector3.forward), (targetPos - eye.transform.position).normalized);
            var cosineThreshold = Mathf.Cos(fov * Mathf.Deg2Rad * 0.5f);
            isInFOV = dotProd >= cosineThreshold;
            return isInFOV;
        }
        
        
        private void OnDrawGizmos()
        {
            if (isDebug)
            {
                // draws ray to player and changes color depening if player is in FOV
                Gizmos.color = isInFOV ? Color.green : Color.red;

                Vector3 rightBoundary = Quaternion.Euler(0, fov * 0.5f, 0) * eye.transform.TransformDirection(eye.transform.forward);
                Vector3 leftBoundary = Quaternion.Euler(0, -fov * 0.5f, 0) * eye.transform.TransformDirection(eye.transform.forward);

                // gets shows wrong direction when facing -z dont care enough to fix right now.
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(eye.transform.position, eye.transform.position + rightBoundary * viewingDistance);
                Gizmos.DrawLine(eye.transform.position, eye.transform.position + leftBoundary * viewingDistance);
            }
        }
    }
}
