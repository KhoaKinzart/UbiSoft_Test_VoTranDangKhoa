using System.Collections.Generic;
using UnityEngine;

namespace Game.ProceduralGeneration
{
    public class OpenMapGenerator : IMapGenerator
    {
        private const int MAX_ATTEMPTS = 10;

        [Header("Wall Cluster Settings")]
        private readonly int minClusterSize = 2;
        private readonly int maxClusterSize = 6;
        private readonly float clusterDensity = 0.15f;

        public int[,] Generate(int width, int height, float obstacleDensity)
        {
            int[,] grid = null;
            int attempts = 0;

            while (attempts < MAX_ATTEMPTS)
            {
                grid = GenerateOpenMap(width, height, obstacleDensity);

                if (ValidateConnectivity(grid, width, height))
                {
                    return grid;
                }

                attempts++;
            }

            return GenerateFallbackMap(width, height);
        }

        private int[,] GenerateOpenMap(int width, int height, float obstacleDensity)
        {
            int[,] grid = new int[width, height];

            CreateBorder(grid, width, height);

            List<Rect> safeZones = CreateSafeZones(width, height);

            int totalCells = width * height;
            int targetObstacles = Mathf.RoundToInt(totalCells * obstacleDensity * 0.3f);
            int clusterCount = Mathf.RoundToInt(totalCells * clusterDensity);

            for (int i = 0; i < clusterCount; i++)
            {
                Vector2Int randomPos = GetRandomPosition(width, height);

                if (IsInSafeZone(randomPos, safeZones))
                    continue;

                CreateWallCluster(grid, randomPos, width, height);
            }

            AddScatteredWalls(grid, width, height, safeZones, obstacleDensity);

            SmoothWalls(grid, width, height);

            return grid;
        }

        private void CreateBorder(int[,] grid, int width, int height)
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

        private List<Rect> CreateSafeZones(int width, int height)
        {
            List<Rect> safeZones = new List<Rect>();

            int centerSize = Mathf.Max(5, Mathf.Min(width, height) / 4);
            safeZones.Add(new Rect(
                width / 2 - centerSize / 2,
                height / 2 - centerSize / 2,
                centerSize,
                centerSize
            ));

            int cornerSize = Mathf.Max(3, Mathf.Min(width, height) / 8);
            safeZones.Add(new Rect(2, 2, cornerSize, cornerSize));
            safeZones.Add(new Rect(width - cornerSize - 2, 2, cornerSize, cornerSize));
            safeZones.Add(new Rect(2, height - cornerSize - 2, cornerSize, cornerSize));
            safeZones.Add(new Rect(width - cornerSize - 2, height - cornerSize - 2, cornerSize, cornerSize));

            return safeZones;
        }

        private bool IsInSafeZone(Vector2Int pos, List<Rect> safeZones)
        {
            foreach (var zone in safeZones)
            {
                if (zone.Contains(new Vector2(pos.x, pos.y)))
                    return true;
            }
            return false;
        }

        private void CreateWallCluster(int[,] grid, Vector2Int center, int width, int height)
        {
            int clusterSize = Random.Range(minClusterSize, maxClusterSize + 1);

            ClusterShape shape = (ClusterShape)Random.Range(0, 4);

            switch (shape)
            {
                case ClusterShape.Square:
                    CreateSquareCluster(grid, center, clusterSize, width, height);
                    break;
                case ClusterShape.Line:
                    CreateLineCluster(grid, center, clusterSize, width, height);
                    break;
                case ClusterShape.LShape:
                    CreateLShapeCluster(grid, center, clusterSize, width, height);
                    break;
                case ClusterShape.Cross:
                    CreateCrossCluster(grid, center, clusterSize, width, height);
                    break;
            }
        }

        private void CreateSquareCluster(int[,] grid, Vector2Int center, int size, int width, int height)
        {
            for (int x = center.x - size / 2; x <= center.x + size / 2; x++)
            {
                for (int z = center.y - size / 2; z <= center.y + size / 2; z++)
                {
                    if (IsValidPosition(x, z, width, height))
                    {
                        grid[x, z] = 1;
                    }
                }
            }
        }

        private void CreateLineCluster(int[,] grid, Vector2Int center, int size, int width, int height)
        {
            bool horizontal = Random.value > 0.5f;

            if (horizontal)
            {
                for (int x = center.x - size; x <= center.x + size; x++)
                {
                    if (IsValidPosition(x, center.y, width, height))
                        grid[x, center.y] = 1;

                    if (Random.value > 0.5f && IsValidPosition(x, center.y + 1, width, height))
                        grid[x, center.y + 1] = 1;
                }
            }
            else
            {
                for (int z = center.y - size; z <= center.y + size; z++)
                {
                    if (IsValidPosition(center.x, z, width, height))
                        grid[center.x, z] = 1;

                    if (Random.value > 0.5f && IsValidPosition(center.x + 1, z, width, height))
                        grid[center.x + 1, z] = 1;
                }
            }
        }

        private void CreateLShapeCluster(int[,] grid, Vector2Int center, int size, int width, int height)
        {
            for (int x = center.x; x <= center.x + size; x++)
            {
                if (IsValidPosition(x, center.y, width, height))
                    grid[x, center.y] = 1;
            }

            for (int z = center.y; z <= center.y + size; z++)
            {
                if (IsValidPosition(center.x, z, width, height))
                    grid[center.x, z] = 1;
            }
        }

        private void CreateCrossCluster(int[,] grid, Vector2Int center, int size, int width, int height)
        {
            for (int x = center.x - size; x <= center.x + size; x++)
            {
                if (IsValidPosition(x, center.y, width, height))
                    grid[x, center.y] = 1;
            }

            for (int z = center.y - size; z <= center.y + size; z++)
            {
                if (IsValidPosition(center.x, z, width, height))
                    grid[center.x, z] = 1;
            }
        }

        private void AddScatteredWalls(int[,] grid, int width, int height, List<Rect> safeZones, float density)
        {
            int scatteredCount = Mathf.RoundToInt(width * height * density * 0.1f);

            for (int i = 0; i < scatteredCount; i++)
            {
                Vector2Int pos = GetRandomPosition(width, height);

                if (!IsInSafeZone(pos, safeZones) && grid[pos.x, pos.y] == 0)
                {
                    grid[pos.x, pos.y] = 1;
                }
            }
        }

        private void SmoothWalls(int[,] grid, int width, int height)
        {
            for (int x = 1; x < width - 1; x++)
            {
                for (int z = 1; z < height - 1; z++)
                {
                    if (grid[x, z] == 1)
                    {
                        int neighborWalls = CountNeighborWalls(grid, x, z, width, height);

                        if (neighborWalls <= 1 && Random.value > 0.7f)
                        {
                            grid[x, z] = 0;
                        }
                    }
                }
            }
        }

        private int CountNeighborWalls(int[,] grid, int x, int z, int width, int height)
        {
            int count = 0;

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dz == 0) continue;

                    int nx = x + dx;
                    int nz = z + dz;

                    if (IsValidPosition(nx, nz, width, height) && grid[nx, nz] == 1)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private Vector2Int GetRandomPosition(int width, int height)
        {
            return new Vector2Int(
                Random.Range(2, width - 2),
                Random.Range(2, height - 2)
            );
        }

        private bool IsValidPosition(int x, int z, int width, int height)
        {
            return x > 0 && x < width - 1 && z > 0 && z < height - 1;
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

            return (reachableCount == walkableCount);
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
            int centerX = width / 2;
            int centerZ = height / 2;

            if (grid[centerX, centerZ] == 0)
                return new Vector2Int(centerX, centerZ);

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

        private int[,] GenerateFallbackMap(int width, int height)
        {
            int[,] grid = new int[width, height];
            CreateBorder(grid, width, height);
            return grid;
        }

        private enum ClusterShape
        {
            Square,
            Line,
            LShape,
            Cross
        }
    }
}