using UnityEngine;
using Game.Core.Constants;

namespace Game.Simulation.Collision
{
    public class GridCollisionService : ICollisionService
    {
        private readonly int[,] _gridData;
        private readonly int _width;
        private readonly int _height;

        public GridCollisionService(int[,] gridData, int width, int height)
        {
            _gridData = gridData;
            _width = width;
            _height = height;
        }

        public bool CanMoveTo(Vector3 worldPosition)
        {
            if (!IsPositionWalkable(worldPosition))
                return false;

            Vector3[] checkPoints = new Vector3[]
            {
                worldPosition + new Vector3(GameplayConstants.Movement.PLAYER_RADIUS, 0, 0),
                worldPosition + new Vector3(-GameplayConstants.Movement.PLAYER_RADIUS, 0, 0),
                worldPosition + new Vector3(0, 0, GameplayConstants.Movement.PLAYER_RADIUS),
                worldPosition + new Vector3(0, 0, -GameplayConstants.Movement.PLAYER_RADIUS)
            };

            foreach (Vector3 point in checkPoints)
            {
                if (!IsPositionWalkable(point))
                    return false;
            }

            return true;
        }

        public bool IsPositionWalkable(Vector3 worldPosition)
        {
            int gridX = Mathf.FloorToInt(worldPosition.x + 0.5f);
            int gridZ = Mathf.FloorToInt(worldPosition.z + 0.5f);

            if (gridX < 0 || gridX >= _width || gridZ < 0 || gridZ >= _height)
                return false;

            return _gridData[gridX, gridZ] == 0;
        }
    }
}
