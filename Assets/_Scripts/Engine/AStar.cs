using System.Collections.Generic;
using UnityEngine; // Dùng Mathf và Vector2Int

public class AStar
{
    private int width;
    private int height;
    private Node[,] grid; // Lưu tham chiếu đến các node

    // Hướng di chuyển: Lên, Xuống, Trái, Phải
    private readonly int[] dx = { 0, 0, -1, 1 };
    private readonly int[] dy = { 1, -1, 0, 0 };

    public AStar(int width, int height)
    {
        this.width = width;
        this.height = height;
        InitializeGrid();
    }

    // Khởi tạo lưới Node (chỉ chạy 1 lần lúc đầu)
    private void InitializeGrid()
    {
        grid = new Node[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Mặc định ban đầu là đi được hết, sau này sẽ update vật cản sau
                grid[x, y] = new Node(x, y, true);
            }
        }
    }

    // Cập nhật dữ liệu vật cản từ GridManager
    public void UpdateGridObstacles(int[,] mapData)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 1 là vật cản -> IsWalkable = false
                bool isWalkable = (mapData[x, y] == 0);
                grid[x, y].IsWalkable = isWalkable;
            }
        }
    }

    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos)
    {
        // 1. Validate đầu vào
        if (!IsValid(startPos) || !IsValid(targetPos)) return null;

        Node startNode = grid[startPos.x, startPos.y];
        Node targetNode = grid[targetPos.x, targetPos.y];

        if (!startNode.IsWalkable || !targetNode.IsWalkable) return null;

        // 2. Reset trạng thái các Node (để dùng lại, đỡ tốn RAM new object)
        // Lưu ý: Với map quá lớn (1000x1000), việc reset toàn bộ map sẽ chậm.
        // Có thể tối ưu bằng cách dùng "Search ID", nhưng ở đây ta reset đơn giản trước.
        ResetAllNodes();

        // 3. Khởi tạo
        PriorityQueue<Node> openSet = new PriorityQueue<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        startNode.G = 0;
        startNode.H = GetHeuristic(startNode, targetNode);
        openSet.Enqueue(startNode);

        // 4. Vòng lặp chính
        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue();

            // Đã đến đích!
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            closedSet.Add(currentNode);

            // Duyệt 4 hướng hàng xóm
            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.IsWalkable || closedSet.Contains(neighbor)) continue;

                float newMovementCostToNeighbor = currentNode.G + 1; // Chi phí đi sang ô bên cạnh là 1

                if (newMovementCostToNeighbor < neighbor.G || !openSet.Contains(neighbor))
                {
                    neighbor.G = newMovementCostToNeighbor;
                    neighbor.H = GetHeuristic(neighbor, targetNode);
                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor);
                    }
                }
            }
        }

        // Không tìm thấy đường
        return null;
    }

    // Hàm truy vết đường đi từ Đích về Nguồn
    private List<Vector2Int> RetracePath(Node startNode, Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(new Vector2Int(currentNode.X, currentNode.Y));
            currentNode = currentNode.Parent;
        }

        // Đảo ngược lại để có đường đi từ Start -> End
        path.Reverse();
        return path;
    }

    // Hàm tính khoảng cách Manhattan (phù hợp cho Grid 4 hướng)
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

    // Reset nhanh (có thể tối ưu thêm sau này)
    private void ResetAllNodes()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y].Reset();
            }
        }
    }
}