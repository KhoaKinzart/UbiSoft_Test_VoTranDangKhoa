using System.Collections.Generic;
using UnityEngine;

namespace Game.Simulation.Pathfinding
{
    public interface IPathfindingService
    {
        List<Vector2Int> FindPath(Vector2Int start, Vector2Int end);
        void UpdateGridObstacles(int[,] gridData);
    }
}
