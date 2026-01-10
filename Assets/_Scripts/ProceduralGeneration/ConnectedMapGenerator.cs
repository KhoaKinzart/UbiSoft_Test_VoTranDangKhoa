using System.Collections.Generic;
using UnityEngine;

namespace Game.ProceduralGeneration
{
    public class ConnectedMapGenerator : IMapGenerator
    {
        private const int MAX_REGENERATION_ATTEMPTS = 10;
        private const int MIN_OBSTACLE_COUNT = 5;

        public int[,] Generate(int width, int height, float obstacleDensity)
        {
            int[,] grid = null;
            int attempts = 0;

            while (attempts < MAX_REGENERATION_ATTEMPTS)
            {
                grid = GenerateRandomGrid(width, height, obstacleDensity);
                
                if (ValidateConnectivity(grid, width, height) && ValidateMinimumObstacles(grid, width, height))
                {
                    return grid;
                }

                attempts++;
            }

            return GenerateFallbackMap(width, height);
        }

        private bool ValidateMinimumObstacles(int[,] grid, int width, int height)
        {
            int obstacleCount = 0;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    if (grid[x, z] == 1) obstacleCount++;
                }
            }

            int edgeObstacles = (width * 2) + (height * 2) - 4;
            int innerObstacles = obstacleCount - edgeObstacles;

            if (innerObstacles < MIN_OBSTACLE_COUNT)
            {
                return false;
            }

            return true;
        }

        public bool ValidateConnectivity(int[,] grid, int width, int height)
        {
            int walkableCount = CountWalkableCells(grid, width, height);
            
            if (walkableCount == 0)
                return false;

            Vector2Int startPos = FindFirstWalkableCell(grid, width, height);
            if (startPos.x == -1)
                return false;

            int reachableCount = FloodFillCount(grid, width, height, startPos);

            bool isFullyConnected = (reachableCount == walkableCount);
            
            return isFullyConnected;
        }

        private int[,] GenerateRandomGrid(int width, int height, float obstacleDensity)
        {
            int[,] grid = new int[width, height];
            int centerX = width / 2;
            int centerZ = height / 2;
            int safeZoneSize = Mathf.Max(2, width / 8);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    bool isEdge = (x == 0 || z == 0 || x == width - 1 || z == height - 1);
                    bool isSafe = (Mathf.Abs(x - centerX) <= safeZoneSize && Mathf.Abs(z - centerZ) <= safeZoneSize);

                    if (isEdge)
                    {
                        grid[x, z] = 1;
                    }
                    else if (isSafe)
                    {
                        grid[x, z] = 0;
                    }
                    else
                    {
                        grid[x, z] = Random.value < obstacleDensity ? 1 : 0;
                    }
                }
            }

            return grid;
        }

        private int[,] GenerateFallbackMap(int width, int height)
        {
            int[,] grid = new int[width, height];
            int centerX = width / 2;
            int centerZ = height / 2;
            int safeZoneSize = Mathf.Max(3, width / 6);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    bool isEdge = (x == 0 || z == 0 || x == width - 1 || z == height - 1);
                    bool isCenterSafe = (Mathf.Abs(x - centerX) <= safeZoneSize && Mathf.Abs(z - centerZ) <= safeZoneSize);

                    if (isEdge)
                    {
                        grid[x, z] = 1;
                    }
                    else if (isCenterSafe)
                    {
                        grid[x, z] = 0;
                    }
                    else
                    {
                        bool isPatternObstacle = ((x + z) % 4 == 0) && (x % 2 == 0 || z % 2 == 0);
                        grid[x, z] = isPatternObstacle ? 1 : 0;
                    }
                }
            }

            return grid;
        }

        private int CountWalkableCells(int[,] grid, int width, int height)
        {
            int count = 0;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    if (grid[x, z] == 0)
                        count++;
                }
            }
            return count;
        }

        private Vector2Int FindFirstWalkableCell(int[,] grid, int width, int height)
        {
            for (int x = 1; x < width - 1; x++)
            {
                for (int z = 1; z < height - 1; z++)
                {
                    if (grid[x, z] == 0)
                        return new Vector2Int(x, z);
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

            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                foreach (var dir in directions)
                {
                    int nx = current.x + dir.x;
                    int nz = current.y + dir.y;

                    if (nx >= 0 && nx < width && nz >= 0 && nz < height &&
                        !visited[nx, nz] && grid[nx, nz] == 0)
                    {
                        visited[nx, nz] = true;
                        queue.Enqueue(new Vector2Int(nx, nz));
                        count++;
                    }
                }
            }

            return count;
        }
    }
}
