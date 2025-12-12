
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Common.Lab2_AStar.Scripts
{
    public class Pathfinder : MonoBehaviour
    {
        private GridManager _gridManager;
        public GameObject agentMover;
        
        [Header("Start & Goal")]
        [SerializeField] private Transform start;
        [SerializeField] private Transform goal;
        [SerializeField] private bool isDiagonal = false;
        
        [Header("Materials Paths")]
        [SerializeField] private Material startMaterial;
        [SerializeField] private Material goalMaterial;
        [SerializeField] private Material pathMaterial;
        [SerializeField] private Material openMaterial;
        [SerializeField] private Material closedMaterial;

        private List<Node> _lastPath =  new();
        private Node _startNode;
        private Node _goalNode;

        private InputAction pathFindAction;
        private InputAction resetAction;

        #region Start Functions

            private void Start()
            {
                _gridManager = GridManager.Instance;
            }
            
            private void OnEnable()
            {
               // pathFindAction = new InputAction(
               //     name: "StartRoutine",
               //     type: InputActionType.Button,
               //     binding: "<Keyboard>/space"
               //     );
               // pathFindAction.performed += OnSpacePreformed;
               // pathFindAction.Enable();

                resetAction = new InputAction(
                    name: "ResetRoutine",
                    type: InputActionType.Button,
                    binding: "<Keyboard>/r"
                    );
                resetAction.performed += ResetActionOnPerformed;
                resetAction.Enable();

            }


            private void OnDisable()
            {
                if (pathFindAction != null)
                {
                    pathFindAction.performed -= OnSpacePreformed;
                    pathFindAction.Disable();
                }

                if (resetAction != null)
                {
                    resetAction.performed -= ResetActionOnPerformed;
                    resetAction.Disable();
                }
                
            }


            private void OnSpacePreformed(InputAction.CallbackContext obj)
            {
                RunPathFinding();
            }
            private void ResetActionOnPerformed(InputAction.CallbackContext obj)
            {
                _gridManager.ResetGridValues(false);
                _gridManager.ResetGridVisuals();
            }

        #endregion

        public void InitializePath(Vector3 startPos, Vector3 targetPos)
        {
            _gridManager.ResetGridValues(true);
            _gridManager.ResetGridVisuals();
            
            _lastPath.Clear();
            _startNode = _gridManager.TryGetNodeFormWorldPosition(startPos);
            _goalNode = _gridManager.TryGetNodeFormWorldPosition(targetPos);
            
            RunPathFinding();
        }
        
        
        private void RunPathFinding()
        {
            _gridManager.ResetGridValues(true);

            if (_startNode == _goalNode || _startNode == null || _goalNode == null)
            {
                Debug.LogWarning("Invalid start or goal node");
                return;
            }
            
            HashSet<Node> openListVisuals = new();
            HashSet<Node> closedListVisuals = new();
            
            var lastPathList = FindPath(_startNode, _goalNode, openListVisuals, closedListVisuals);

            #region Set Material for path and Search

            foreach (var node in openListVisuals.Where(node => node.walkable))
                _gridManager.SetTileMaterial(node, openMaterial);

            foreach (var node in closedListVisuals.Where(node => node.walkable))
                _gridManager.SetTileMaterial(node, closedMaterial);

            if (lastPathList != null)
            {
                lastPathList.Reverse();
                foreach (var node in lastPathList)
                    _gridManager.SetTileMaterial(node, pathMaterial);
                
                agentMover.GetComponent<AgentMover>().FollowPath(lastPathList);
            } else Debug.Log("No path found");
            
            _gridManager.SetTileMaterial(_startNode, startMaterial);
            _gridManager.SetTileMaterial(_goalNode, goalMaterial);
            #endregion
        }


        private List<Node> FindPath(Node startNode, Node goalNode, HashSet<Node> openListVisuals = null, HashSet<Node> closedListVisuals = null)
        {
            List<Node> openSet = new();
            HashSet<Node> closedSet = new();
            
            startNode.gCost = 0;
            startNode.hCost = startNode.GetHeuristicCost(goalNode.Position);
            openSet.Add(startNode);
            openListVisuals?.Add(startNode);        // Visuals

            while (openSet.Count > 0)
            {
                Node currentNode = GetLowestFCostNode(openSet, closedSet);

                if (currentNode == goalNode)
                {
                    return ReconstructPath(startNode, goalNode).ToList();
                }
                
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                
                closedListVisuals?.Add(currentNode);    // Visuals

                foreach (var neighbourNode in _gridManager.GetNeighbours(currentNode, isDiagonal).Where(
                             w => w != null && w.walkable && !closedSet.Contains(w) ))
                {
                    float tentativeG = currentNode.gCost + 1;

                    if (tentativeG > neighbourNode.gCost) continue;
                    
                    neighbourNode.Parent = currentNode;
                    neighbourNode.gCost = tentativeG;
                    neighbourNode.hCost = neighbourNode.GetHeuristicCost(goalNode.Position);

                    if (!openSet.Contains(neighbourNode))
                    {
                        openSet.Add(neighbourNode);
                        openListVisuals?.Add(neighbourNode);        // visuals
                    }
                }
            }
            return null;
        }

        private Node GetLowestFCostNode(List<Node> openSet, HashSet<Node> closedSet)
        {
            var lowestNode = openSet[0];

            foreach (var node in openSet)
            {
                if (node.fCost < lowestNode.fCost&& !closedSet.Contains(node))
                    lowestNode = node;
            }
            return lowestNode;
        }

        private IEnumerable<Node> ReconstructPath(Node startNode, Node goalNode)
        {
            var currentNode = goalNode;
            while (currentNode != startNode)
            {
                yield return currentNode;
                currentNode = currentNode.Parent;
            }
        }
    }
}
