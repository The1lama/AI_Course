using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace PlayerController
{
    public class PlayerController : MonoBehaviour
    {

        private Rigidbody _rigidbody;

        private InputAction moveAction;
        private InputAction sprintAction;
        
        [Header("Movement")]
        [SerializeField] private float fSpeed = 5f;
        [SerializeField] private float rotateSpeed = 360f;

        private Vector2  _v2PlayerVelocity;

        #region StartNStuff

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            Initialize();
        }
     
        private void Initialize()
        {
            moveAction = InputSystem.actions.FindAction("Player/Move");
            sprintAction = InputSystem.actions.FindAction("Player/Sprint");
        }

        private void OnEnable()
        {
            InputSystem.actions.Enable();
        }

        private void OnDisable()
        {
            InputSystem.actions.Disable();
        }
        
        #endregion

        private void Update()
        {
            _v2PlayerVelocity = moveAction.ReadValue<Vector2>();
            Dash();
        }

        private void FixedUpdate()
        {
            Move();
        }
    
        private void Move()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            if (moveAction.ReadValue<Vector2>().sqrMagnitude <= 0.0f) return;
            var move = new Vector3(_v2PlayerVelocity.x, 0f, _v2PlayerVelocity.y);
            Rotate(move);
            _rigidbody.linearVelocity = move * fSpeed;
        }

        private void Rotate(Vector3 direction)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
            
        }
        
        private void Dash()
        {
            if(sprintAction.triggered)
            {
                _rigidbody.AddForce(transform.forward*50, ForceMode.Impulse);
            }
        }
 
    }
}

