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

        [HideInInspector] public Node[,] nodes;
        private Dictionary<GameObject, Node> tileToNode = new();
        
        // Input action for click
        private InputAction clickAction;
        
        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;

        public static GridManager Instance { get; private set; }

        private Vector2Int[] Direction => new Vector2Int[] {
            new Vector2Int(0,1),       // North
            new Vector2Int(1,0),       // East
            new Vector2Int(0,-1),      // West
            new Vector2Int(-1,0),      // South
            
            new Vector2Int(1,1),       // North East
            new Vector2Int(1,-1),      // North West
            new Vector2Int(-1,1),      // South East
            new Vector2Int(-1,-1),     // South West
        };

        private enum Cardinal
        {
            North = 0, 
            South = 1, 
            East = 2, 
            West = 3, 
            
            NorthWest = 4, 
            NorthEast = 5, 
            SouthEast = 6, 
            SouthWest = 7
        }

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
                    SetWalkableNode(node, newWalkable);
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

        private void GenerateWalls()
        {
            for (int i = 0; i < (height*width)*0.4; i++)
            {
                int x =  Random.Range(0, width);
                int y =  Random.Range(0, height);
                
                var node = TryGetNode(x, y);
                SetTileMaterial(node, wallMaterial);
                node.walkable = false;
                
            } 
        }

        #region Node stuff
        
        private Node TryGetNode(int x, int y)
        {
            if(x < 0 || x >= width || y < 0 || y >= height) return null;
            return nodes[x, y];
        }
        private Node TryGetNode(Vector2Int pos)
        {
            return TryGetNode(pos.x, pos.y);
        }
        
        public Vector3 GetNodeToWorldPosition(Node targetNode, Vector3 currentPosition)
        {
            return new Vector3(targetNode.x * cellSize, currentPosition.y,  targetNode.y * cellSize);
        }

        public Node TryGetNodeFormWorldPosition(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / cellSize);
            int y = Mathf.RoundToInt(worldPos.z / cellSize);
            return TryGetNode(x, y);
        }

        public IEnumerable<Node> GetNeighbours(Node node, bool ifAllowDiagonals = false)
        {
            // 4-neighbour
            yield return TryGetNode(node.Position + Direction[(int)Cardinal.North]);
            yield return TryGetNode(node.Position + Direction[(int)Cardinal.East]);
            yield return TryGetNode(node.Position + Direction[(int)Cardinal.West]);
            yield return TryGetNode(node.Position + Direction[(int)Cardinal.South]);
            
            // +4-neighbour
            if (!ifAllowDiagonals) yield break;
            
            yield return TryGetNode(node.Position + Direction[(int)Cardinal.NorthEast]);
            yield return TryGetNode(node.Position + Direction[(int)Cardinal.NorthWest]);
            yield return TryGetNode(node.Position + Direction[(int)Cardinal.SouthEast]);
            yield return TryGetNode(node.Position + Direction[(int)Cardinal.SouthWest]);
        }
        
        #endregion
        
        private void SetWalkableNode(Node node, bool walkable)
        {
            node.walkable = walkable;
            SetTileMaterial(node, walkable ? walkableMaterial : wallMaterial);
        }
        
        public void SetTileMaterial(Node node, Material material)
        {
            var component = node.tile.GetComponent<Renderer>();
            if (component != null && material != null)
            {
                component.material = material;
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
