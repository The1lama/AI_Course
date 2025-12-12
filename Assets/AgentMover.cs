using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Common.Lab2_AStar.Scripts
{
    public class AgentMover : MonoBehaviour
    {
        public GameObject pathFinder;
        public GameObject target;
        private GridManager gridManager;
        [SerializeField] private float moveSpeed = 3f;

        private List<Node> currentPath;
        private int currentIndex = 0;
        Node targetNode;
        Vector3 targetPosition;

        private InputAction moveTowardAction;
        private Rigidbody rb;
        private float time = 0f;
        private float timeLeft = 0.2f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            moveTowardAction = new InputAction(
                name: "StartRoutine",
                type: InputActionType.Button,
                binding: "<Keyboard>/space"
            );
            moveTowardAction.performed += OnSpacePreformed;
            moveTowardAction.Enable();
        }

        private void OnDisable()
        {
            if (moveTowardAction != null)
            {
                moveTowardAction.performed -= OnSpacePreformed;
                moveTowardAction.Disable();
            }
        }
        
        private void OnSpacePreformed(InputAction.CallbackContext obj)
        {
            RunPathFinding();
        }

        private void RunPathFinding()
        {
            pathFinder.GetComponent<Pathfinder>().InitializePath(transform.position, target.transform.position);
        }

        private void Start()
        {
            gridManager =  GridManager.Instance;
        }
        
        public void FollowPath(List<Node> path)
        {
            currentPath = path;
            currentIndex = 0;
        }

        private void Update()
        {
            
            timeLeft -=  Time.deltaTime;
            if (0 >= timeLeft)
            {
                RunPathFinding();
                timeLeft += 0.2f;
            }
            
            if (currentPath == null || currentPath.Count == currentIndex) return;
            
            targetNode =  currentPath[currentIndex];
            targetPosition = gridManager.GetNodeToWorldPosition(targetNode, transform.position);

            rb.MovePosition(targetPosition); //Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            if(Vector3.Distance(transform.position, targetPosition) < 0.001f)
                currentIndex++;
        }
    }
}
