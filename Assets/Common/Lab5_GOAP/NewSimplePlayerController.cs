using UnityEngine;
using UnityEngine.InputSystem;

namespace Common.Lab5_GOAP.Scripts
{
    public class NewSimplePlayerController : MonoBehaviour
    {
        [Header("Movement")] 
        public float speed = 5f;


        private Vector2 _moveInput;

        private void Update()
        {
            var dir = new Vector3(_moveInput.x, 0f, _moveInput.y);
            
            if(dir.sqrMagnitude > 1f)
                dir.Normalize();
            
            transform.position += dir * speed * Time.deltaTime;
            
            if(dir.sqrMagnitude > 0.001f)
                transform.forward = dir;
        }

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }
        
    }
    
}
