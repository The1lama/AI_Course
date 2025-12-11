using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Common.Lab2_AStar.Scripts
{
    public class GridManager : MonoBehaviour
    {
        #region Properties

        [Header("Grid Settings")]
        [SerializeField] private int width  = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private float cellSize = 1f;

        [Header("Prefabs & Materials")] 
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Material walkableMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material startTileMaterial;
        [SerializeField] private Material goalTileMaterial;
        
        [HideInInspector] public Node[,] nodes;
        private Dictionary<GameObject, Node> tileToNode = new();
        
        // Input action for click
        private InputAction clickAction;
        
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;

        public static GridManager Instance { get; private set; }

        #endregion

        #region Start Functions
        
        private void Awake()
        {
            Instance = this;
            GenerateGrid();
            GenerateWalls();
        }

        private void OnEnable()
        {
            clickAction = new InputAction(
                name: "Click",
                type: InputActionType.Button,
                binding: "<Mouse>/leftButton"
                );
            clickAction.performed += OnClickPerformed;
            clickAction.Enable();
        }

        private void OnDisable()
        {
            if (clickAction != null)
            {
                clickAction.performed -= OnClickPerformed;
                clickAction.Disable();
            }
        }
        
        private void OnClickPerformed(InputAction.CallbackContext obj)
        {
            HandleMouseClick();
        }
        
        #endregion

        private void HandleMouseClick()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;

                if (tileToNode.TryGetValue(clicked, out Node node))
                {
                    bool newWalkable = !node.walkable;
                    SetWalkable(node, newWalkable);
                }
            }
        }

        private void GenerateGrid()
        {
            nodes = new Node[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 worldPos = new Vector3(x * cellSize, 0f,  y * cellSize);
                    GameObject tileGo = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                    tileGo.name = $"Tile_{x}_{y}";

                    Node node = new Node(x, y, true, tileGo);
                    nodes[x, y] = node;
                    tileToNode[tileGo] = node;

                    SetTileMaterial(node, walkableMaterial);
                }
            }
        }

        public void GenerateWalls()
        {
            for (int i = 0; i < 40; i++)
            {
                int x =  Random.Range(0, width);
                int y =  Random.Range(0, height);
                
                var node = GetNode(x, y);
                SetTileMaterial(node, wallMaterial);
                node.walkable = false;
                
            } 

        }

        #region Node stuff
        
        public Node GetNode(int x, int y)
        {
            if(x < 0 || x >= width || y < 0 || y >= height) return null;
            return nodes[x, y];
        }
        
        public Vector3 NodeToWorldPosition(Node targetNode, Vector3 currentPosition)
        {
            return new Vector3(targetNode.x * cellSize, currentPosition.y,  targetNode.y * cellSize);
        }

        public Node GetNodeFormWorldPosition(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / cellSize);
            int y = Mathf.RoundToInt(worldPos.z / cellSize);
            return GetNode(x, y);
        }

        public IEnumerable<Node> GetNeighbours(Node node, bool allowDiagonals = false)
        {
            int x = node.x, y = node.y;
            
            // 4-neighbour
            yield return GetNode(x + 1, y);
            yield return GetNode(x - 1, y);
            yield return GetNode(x, y + 1);
            yield return GetNode(x, y - 1);
            
            // +4-neighbour
            if (allowDiagonals)
            {
                yield return GetNode(x + 1, y + 1);
                yield return GetNode(x - 1, y + 1);
                yield return GetNode(x + 1, y - 1);
                yield return GetNode(x - 1, y - 1);
                
            }
        }
        
        #endregion
        
        private void SetWalkable(Node node, bool walkable)
        {
            node.walkable = walkable;
            SetTileMaterial(node, walkable ? walkableMaterial : wallMaterial);
        }
        
        private void SetTileMaterial(Node node, Material material)
        {
            var renderer = node.tile.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }
        }

        #region ResetValues

        public void ResetGridVisuals()
        {
            foreach (var gridNode in nodes)
            {
                if(gridNode.walkable)
                    SetTileMaterial(gridNode, walkableMaterial);
            }
        }

        public void ResetGridValues(bool wantWalls = true)
        {
            foreach (var node in nodes)
            {
                if(!wantWalls)
                    node.walkable = true;       
                node.gCost = float.PositiveInfinity;
                node.hCost = 0f;
                node.Parent = null;
            }
        }

        #endregion


    }
}
