using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Common.Lab2
{
    public class Pathfinder : MonoBehaviour
    {
        private GridManager gridManager;
        
        
        [Header("Start & Goal")]
        [SerializeField] private Transform start;
        [SerializeField] private Transform goal;
        
        [Header("Materials")]
        [SerializeField] private Material startMaterial;
        [SerializeField] private Material goalMaterial;
        [SerializeField] private Material pathMaterial;
        [SerializeField] private Material openMaterial;
        [SerializeField] private Material closedMaterial;

        private List<Node> lastPath;

        private InputAction pathFindAction;
        
        private void OnEnable()
        {
            pathFindAction = new InputAction(
                name: "Click",
                type: InputActionType.Button,
                binding: "<Keyboard>/space"
                );
            pathFindAction.performed += OnClickPerformed;
            pathFindAction.Enable();
        }

        private void OnDisable()
        {
            if (pathFindAction != null)
            {
                pathFindAction.performed -= OnClickPerformed;
                pathFindAction.Disable();
            }
        }
        
        private void OnClickPerformed(InputAction.CallbackContext obj)
        {
            Node startNode = gridManager.GetNodeFormWorldPosition(start.position);
            Node goalNode = gridManager.GetNodeFormWorldPosition(goal.position);

            if (startNode == goalNode || startNode == null || goalNode == null)
            {
                Debug.LogWarning("Invalid start or goal node");
                return;
            }

            ResetGridVisuals();
            
            
            HashSet<Node> openListVisuals = new();
            HashSet<Node> closedListVisuals = new();
            
            
            var lastPathList = FindPath(startNode, goalNode, openListVisuals, closedListVisuals);

            foreach (var node in openListVisuals)
            {
                if(node.walkable)
                    SetTileMaterial(node, openMaterial);
            }

            foreach (var node in closedListVisuals)
            {
                if (node.walkable)
                    SetTileMaterial(node, closedMaterial);
            }

            if (lastPathList != null)
            {
                foreach (var node in lastPathList)
                    SetTileMaterial(node, pathMaterial);
            }
            else Debug.Log("No path found");
            
            
            SetTileMaterial(startNode, startMaterial);
            SetTileMaterial(goalNode, goalMaterial);
            
            
            
        }

        private void ResetGridVisuals()
        {
            throw new System.NotImplementedException();
        }

        private void SetTileMaterial(Node node, Material material)
        {
            var curNode = gridManager.GetNodeFormWorldPosition(node.tile.transform.position);
            
        }

        private void ResetGridValues()
        {
            foreach (var gridManagerNode in gridManager.nodes)
            {
                gridManagerNode.walkable = true;       
                gridManagerNode.gCost = float.PositiveInfinity;
                gridManagerNode.hCost = 0f;
                gridManagerNode.parent = null;
            }
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
                Node currentNode = GetLowestFCostNode(openSet);

                if (currentNode == goalNode)
                {
                    return ReconstuctPath(startNode, goalNode);
                }
                
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                
                closedListVisuals?.Add(currentNode);    // Visuals

                foreach (var neighbourNode in gridManager.GetNeighbours(currentNode).Where(w => w != null && w.walkable && !closedSet.Contains(w)))
                {
                    float tentativeG = currentNode.gCost + 1;

                    if (tentativeG < neighbourNode.gCost)
                    {
                        neighbourNode.parent = currentNode;
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

        private float HeuristicCost(Node node, Node goalNode)
        {
            int dx = Mathf.Abs(node.x - goalNode.x);
            int dy = Mathf.Abs(node.y - goalNode.y);
            return dx + dy;
        }
    }
}
