
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Common.Lab2_AStar.Scripts
{
    public class Pathfinder : MonoBehaviour
    {
        private GridManager gridManager;
        public GameObject agentMover;
        
        [Header("Start & Goal")]
        [SerializeField] private Transform start;
        [SerializeField] private Transform goal;
        
        [Header("Materials")]
        [SerializeField] private Material startMaterial;
        [SerializeField] private Material goalMaterial;
        [SerializeField] private Material pathMaterial;
        [SerializeField] private Material openMaterial;
        [SerializeField] private Material closedMaterial;

        private List<Node> lastPath =  new();
        private Node startNode;
        private Node goalNode;

        private InputAction pathFindAction;
        private InputAction resetAction;

        #region Start Functions

            private void Start()
            {
                gridManager = GridManager.Instance;
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
                gridManager.ResetGridValues(false);
                gridManager.ResetGridVisuals();
            }

        #endregion

        public void InitializePath(Vector3 startPos, Vector3 targetPos)
        {
            gridManager.ResetGridValues(true);
            gridManager.ResetGridVisuals();
            
            lastPath.Clear();
            startNode = gridManager.GetNodeFormWorldPosition(startPos);
            goalNode = gridManager.GetNodeFormWorldPosition(targetPos);
            
            RunPathFinding();
        }
        
        
        private void RunPathFinding()
        {
            gridManager.ResetGridValues(true);

            if (startNode == goalNode || startNode == null || goalNode == null)
            {
                Debug.LogWarning("Invalid start or goal node");
                return;
            }
            
            HashSet<Node> openListVisuals = new();
            HashSet<Node> closedListVisuals = new();
            
            var lastPathList = FindPath(startNode, goalNode, openListVisuals, closedListVisuals);

            foreach (var node in openListVisuals.Where(node => node.walkable))
            {
                SetTileMaterial(node, openMaterial);
            }

            foreach (var node in closedListVisuals.Where(node => node.walkable))
            {
                SetTileMaterial(node, closedMaterial);
            }

            if (lastPathList != null)
            {
                lastPathList.Reverse();
                foreach (var node in lastPathList)
                    SetTileMaterial(node, pathMaterial);
                
                agentMover.GetComponent<AgentMover>().FollowPath(lastPathList);
            }
            else Debug.Log("No path found");
            
            SetTileMaterial(startNode, startMaterial);
            SetTileMaterial(goalNode, goalMaterial);
        }


        public List<Node> FindPath(Node startNode, Node goalNode, HashSet<Node> openListVisuals = null, HashSet<Node> closedListVisuals = null)
        {
            List<Node> openSet = new();
            HashSet<Node> closedSet = new();
            
            startNode.gCost = 0;
            startNode.hCost = HeuristicCost(startNode, goalNode);
            openSet.Add(startNode);
            openListVisuals?.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = GetLowestFCostNode(openSet, closedSet);

                if (currentNode == goalNode)
                {
                    return ReconstructPath(startNode, goalNode);
                }
                
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                
                closedListVisuals?.Add(currentNode);    // Visuals

                foreach (var neighbourNode in gridManager.GetNeighbours(currentNode, false).Where(w => w != null && w.walkable && !closedSet.Contains(w)))
                {
                    float tentativeG = currentNode.gCost + 1;

                    if (tentativeG < neighbourNode.gCost)
                    {
                        neighbourNode.Parent = currentNode;
                        neighbourNode.gCost = tentativeG;
                        neighbourNode.hCost = HeuristicCost(neighbourNode, goalNode);

                        if (!openSet.Contains(neighbourNode))
                        {
                            openSet.Add(neighbourNode);
                            openListVisuals?.Add(neighbourNode);        // visuals
                        }
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

        private List<Node> ReconstructPath(Node startNode, Node goalNode)
        {
            List<Node> nodePathQueue = new();
            
            var currentNode = goalNode;

            while (currentNode != startNode)
            {
                nodePathQueue.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            
            return  nodePathQueue;
        }

        private float HeuristicCost(Node node, Node goalNode)
        {
            int dx = Mathf.Abs(node.x - goalNode.x);
            int dy = Mathf.Abs(node.y - goalNode.y);
            return dx + dy;
        }
        
        private void SetTileMaterial(Node node, Material material)
        {
            var curNode = gridManager.GetNodeFormWorldPosition(node.tile.transform.position);
            var curRenderer = curNode.tile.GetComponent<Renderer>();
            if (curRenderer != null && material != null)
                curRenderer.material = material;
        }
    }
}
