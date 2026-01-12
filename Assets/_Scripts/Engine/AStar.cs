using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    private int width;
    private int height;
    private Node[,] grid;


    private int currentSearchId = 0;


    private readonly int[] dx = { 0, 0, -1, 1 };
    private readonly int[] dy = { 1, -1, 0, 0 };

    public AStar(int width, int height)
    {
        this.width = width;
        this.height = height;
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        grid = new Node[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new Node(x, y, true);
            }
        }
    }

    public void UpdateGridObstacles(int[,] mapData)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isWalkable = (mapData[x, y] == 0);
                grid[x, y].IsWalkable = isWalkable;
            }
        }
    }

    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos)
    {
        if (!IsValid(startPos) || !IsValid(targetPos)) return null;

  
        currentSearchId++;


        Node startNode = grid[startPos.x, startPos.y];
        Node targetNode = grid[targetPos.x, targetPos.y];

        if (!startNode.IsWalkable || !targetNode.IsWalkable) return null;

        PriorityQueue<Node> openSet = new PriorityQueue<Node>();


        startNode.LastSearchId = currentSearchId;
        startNode.G = 0;
        startNode.H = GetHeuristic(startNode, targetNode);
        startNode.Parent = null;

        openSet.Enqueue(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue();

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.IsWalkable) continue;

         
                if (neighbor.LastSearchId != currentSearchId)
                {
                    neighbor.G = float.MaxValue;
                    neighbor.Parent = null;
                    neighbor.LastSearchId = currentSearchId; 
                }


                float newMovementCostToNeighbor = currentNode.G + 1;

                if (newMovementCostToNeighbor < neighbor.G)
                {
                    neighbor.G = newMovementCostToNeighbor;
                    neighbor.H = GetHeuristic(neighbor, targetNode);
                    neighbor.Parent = currentNode;

                    openSet.Enqueue(neighbor);
                }
            }
        }

        return null;
    }


    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(new Vector2Int(currentNode.X, currentNode.Y));
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    private float GetHeuristic(Node nodeA, Node nodeB)
    {
        return Mathf.Abs(nodeA.X - nodeB.X) + Mathf.Abs(nodeA.Y - nodeB.Y);
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.X + dx[i];
            int checkY = node.Y + dy[i];

            if (IsValid(new Vector2Int(checkX, checkY)))
            {
                neighbors.Add(grid[checkX, checkY]);
            }
        }
        return neighbors;
    }

    private bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }
}