using UnityEngine;

namespace Game.ProceduralGeneration
{
    public class CellularAutomataGenerator : IMapGenerator
    {
        private const int SMOOTHING_ITERATIONS = 4;
        private const int MAX_REGENERATION_ATTEMPTS = 10;

        public int[,] Generate(int width, int height, float wallDensity)
        {
            int[,] grid = null;
            int attempts = 0;

            while (attempts < MAX_REGENERATION_ATTEMPTS)
            {
                grid = InitializeGrid(width, height, wallDensity);
                
                for (int i = 0; i < SMOOTHING_ITERATIONS; i++)
                {
                    grid = SmoothGrid(grid, width, height);
                }

                if (ValidateConnectivity(grid, width, height))
                {
                    return grid;
                }

                attempts++;
            }

            return GenerateFallbackMap(width, height);
        }

        public bool ValidateConnectivity(int[,] grid, int width, int height)
        {
            var validator = new ConnectedMapGenerator();
            return validator.ValidateConnectivity(grid, width, height);
        }

        private int[,] InitializeGrid(int width, int height, float wallDensity)
        {
            int[,] grid = new int[width, height];
            int centerX = width / 2;
            int centerZ = height / 2;
            int safeZoneSize = Mathf.Max(3, width / 8);

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
                        grid[x, z] = Random.value < wallDensity ? 1 : 0;
                    }
                }
            }

            return grid;
        }

        private int[,] SmoothGrid(int[,] oldGrid, int width, int height)
        {
            int[,] newGrid = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    bool isEdge = (x == 0 || z == 0 || x == width - 1 || z == height - 1);

                    if (isEdge)
                    {
                        newGrid[x, z] = 1;
                    }
                    else
                    {
                        int wallCount = CountAdjacentWalls(oldGrid, x, z, width, height);
                        
                        if (wallCount > 4)
                            newGrid[x, z] = 1;
                        else if (wallCount < 4)
                            newGrid[x, z] = 0;
                        else
                            newGrid[x, z] = oldGrid[x, z];
                    }
                }
            }

            return newGrid;
        }

        private int CountAdjacentWalls(int[,] grid, int x, int z, int width, int height)
        {
            int wallCount = 0;

            for (int nx = x - 1; nx <= x + 1; nx++)
            {
                for (int nz = z - 1; nz <= z + 1; nz++)
                {
                    if (nx == x && nz == z)
                        continue;

                    if (nx < 0 || nx >= width || nz < 0 || nz >= height)
                    {
                        wallCount++;
                    }
                    else if (grid[nx, nz] == 1)
                    {
                        wallCount++;
                    }
                }
            }

            return wallCount;
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
    }
}
