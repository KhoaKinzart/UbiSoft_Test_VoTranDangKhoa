using UnityEngine;

namespace Game.Simulation.Collision
{
    public interface ICollisionService
    {
        bool CanMoveTo(Vector3 worldPosition);
        bool IsPositionWalkable(Vector3 worldPosition);
    }
}
