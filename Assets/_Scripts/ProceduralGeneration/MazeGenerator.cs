using System.Collections.Generic;
using UnityEngine;

namespace Game.ProceduralGeneration
{
    public class MazeGenerator : IMapGenerator
    {
        private const int MAX_REGENERATION_ATTEMPTS = 3;
        private const int SAFE_ZONE_SIZE = 3; // Kích thước vùng an toàn (3x3)

        public int[,] Generate(int width, int height, float obstacleDensity)
        {
            int[,] grid = null;
            int attempts = 0;

            while (attempts < MAX_REGENERATION_ATTEMPTS)
            {
                // Bước 1, 2, 3: Tạo mê cung và bào mòn
                grid = GenerateErodedMaze(width, height, obstacleDensity);

                // Bước 4: Kiểm tra tính liên thông
                if (ValidateConnectivity(grid, width, height))
                {
                    return grid;
                }
                attempts++;
            }

            return GenerateFallbackMap(width, height);
        }

        private int[,] GenerateErodedMaze(int width, int height, float targetDensity)
        {
            // 1. Khởi tạo full tường
            int[,] grid = new int[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    grid[x, z] = 1;
                }
            }

            // 2. Đào mê cung (Recursive Backtracker)
            CarveMazePath(grid, width, height);

            // 3. Bào mòn tường (Erosion) - giúp map thoáng hơn
            ErodeWalls(grid, width, height, targetDensity);

            // 4. Tạo vùng an toàn tuyệt đối ở giữa (tránh player bị kẹt khi spawn)
            CreateSafeZone(grid, width, height);

            // 5. Đóng khung viền
            EnsureBoundaries(grid, width, height);

            return grid;
        }

        private void CreateSafeZone(int[,] grid, int width, int height)
        {
            int centerX = width / 2;
            int centerZ = height / 2;
            int radius = SAFE_ZONE_SIZE / 2;

            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                for (int z = centerZ - radius; z <= centerZ + radius; z++)
                {
                    if (x > 0 && x < width - 1 && z > 0 && z < height - 1)
                    {
                        grid[x, z] = 0; // 0 = Ground
                    }
                }
            }
        }

        private void CarveMazePath(int[,] grid, int width, int height)
        {
            int mazeWidth = (width - 1) / 2;
            int mazeHeight = (height - 1) / 2;

            if (mazeWidth < 2 || mazeHeight < 2) return;

            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            bool[,] visited = new bool[mazeWidth, mazeHeight];

            // Bắt đầu từ giữa
            Vector2Int start = new Vector2Int(mazeWidth / 2, mazeHeight / 2);
            stack.Push(start);
            visited[start.x, start.y] = true;
            grid[start.x * 2 + 1, start.y * 2 + 1] = 0;

            Vector2Int[] directions = {
                new Vector2Int(0, 1), new Vector2Int(1, 0),
                new Vector2Int(0, -1), new Vector2Int(-1, 0)
            };

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();
                List<Vector2Int> unvisitedNeighbors = new List<Vector2Int>();

                foreach (var dir in directions)
                {
                    Vector2Int neighbor = current + dir;
                    if (neighbor.x >= 0 && neighbor.x < mazeWidth &&
                        neighbor.y >= 0 && neighbor.y < mazeHeight &&
                        !visited[neighbor.x, neighbor.y])
                    {
                        unvisitedNeighbors.Add(neighbor);
                    }
                }

                if (unvisitedNeighbors.Count > 0)
                {
                    Vector2Int chosen = unvisitedNeighbors[Random.Range(0, unvisitedNeighbors.Count)];
                    visited[chosen.x, chosen.y] = true;

                    int gridX = chosen.x * 2 + 1;
                    int gridZ = chosen.y * 2 + 1;
                    int wallX = (current.x * 2 + 1 + gridX) / 2;
                    int wallZ = (current.y * 2 + 1 + gridZ) / 2;

                    grid[wallX, wallZ] = 0;
                    grid[gridX, gridZ] = 0;

                    stack.Push(chosen);
                }
                else
                {
                    stack.Pop();
                }
            }
        }

        private void ErodeWalls(int[,] grid, int width, int height, float density)
        {
            for (int x = 1; x < width - 1; x++)
            {
                for (int z = 1; z < height - 1; z++)
                {
                    // Nếu là tường, có xác suất bị xóa dựa trên density
                    if (grid[x, z] == 1 && Random.value > density)
                    {
                        grid[x, z] = 0;
                    }
                }
            }
        }

        private void EnsureBoundaries(int[,] grid, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                grid[x, 0] = 1;
                grid[x, height - 1] = 1;
            }
            for (int z = 0; z < height; z++)
            {
                grid[0, z] = 1;
                grid[width - 1, z] = 1;
            }
        }

        public bool ValidateConnectivity(int[,] grid, int width, int height)
        {
            int walkableCount = CountWalkableCells(grid, width, height);
            if (walkableCount == 0) return false;

            // Start từ tâm map (nơi chắc chắn đã xóa tường nhờ CreateSafeZone)
            Vector2Int startPos = new Vector2Int(width / 2, height / 2);

            // Fallback nếu tâm map vẫn lỗi (rất hiếm)
            if (grid[startPos.x, startPos.y] == 1)
                startPos = FindFirstWalkableCell(grid, width, height);

            if (startPos.x == -1) return false;

            int reachableCount = FloodFillCount(grid, width, height, startPos);
            return reachableCount == walkableCount;
        }

        // --- Utility Functions ---

        private int CountWalkableCells(int[,] grid, int width, int height)
        {
            int count = 0;
            for (int x = 0; x < width; x++)
                for (int z = 0; z < height; z++)
                    if (grid[x, z] == 0) count++;
            return count;
        }

        private Vector2Int FindFirstWalkableCell(int[,] grid, int width, int height)
        {
            int centerX = width / 2;
            int centerZ = height / 2;
            for (int r = 0; r < width; r++)
            {
                for (int x = centerX - r; x <= centerX + r; x++)
                {
                    for (int z = centerZ - r; z <= centerZ + r; z++)
                    {
                        if (x > 0 && x < width - 1 && z > 0 && z < height - 1)
                        {
                            if (grid[x, z] == 0) return new Vector2Int(x, z);
                        }
                    }
                }
            }
            return new Vector2Int(-1, -1);
        }

        private int FloodFillCount(int[,] grid, int width, int height, Vector2Int start)
        {
            bool[,] visited = new bool[width, height];
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            visited[start.x, start.y] = true;
            int count = 1;

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nx = current.x + dx[i];
                    int ny = current.y + dy[i];

                    // --- SỬA LỖI Ở DÒNG DƯỚI NÀY (Thay nz bằng ny) ---
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && !visited[nx, ny] && grid[nx, ny] == 0)
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                        count++;
                    }
                }
            }
            return count;
        }

        private int[,] GenerateFallbackMap(int width, int height)
        {
            int[,] grid = new int[width, height];
            EnsureBoundaries(grid, width, height);
            CreateSafeZone(grid, width, height);
            return grid;
        }
    }
}