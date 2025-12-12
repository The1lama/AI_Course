using UnityEngine;

namespace Common.Lab2_AStar.Scripts
{
    public class Node
    {
        public int x;
        public int y;
        public Vector2Int Position => new Vector2Int(x, y);
        public bool walkable;
        public GameObject tile;

        public float gCost;                         // Distance from Node to Start Node
        public float hCost;                         // Distance from Node to Target Node
        public float fCost => gCost + hCost;        // Combined distance from g+h
        
        public Node Parent;

        public Node(int x, int y, bool walkable, GameObject tile)
        {
            this.x = x;
            this.y = y;
            this.walkable = walkable;
            this.tile = tile;

            gCost = float.PositiveInfinity;
            hCost = 0f;
            Parent = null;
        }

        public float GetHeuristicCost(Vector2Int target)
        {
            return Mathf.Abs(x - target.x) - Mathf.Abs(y - target.y);
        }  
    }
}
