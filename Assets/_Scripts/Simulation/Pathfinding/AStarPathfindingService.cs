using System.Collections.Generic;
using UnityEngine;

namespace Game.Simulation.Pathfinding
{
    public class AStarPathfindingService : IPathfindingService
    {
        private readonly AStar _aStar;

        public AStarPathfindingService(int width, int height)
        {
            _aStar = new AStar(width, height);
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            return _aStar.FindPath(start, end);
        }

        public void UpdateGridObstacles(int[,] gridData)
        {
            _aStar.UpdateGridObstacles(gridData);
        }
    }
}
