using System.Collections.Generic;
using UnityEngine;
using Game.Simulation.Movement;

namespace Game.Gameplay.Entities
{
    public class AgentEntity : CharacterEntity
    {
        public List<Vector2Int> CurrentPath { get; private set; }
        public bool IsMoving => CurrentPath != null && CurrentPath.Count > 0;

        public float DashCooldownTimer { get; set; }
        public float DashDurationTimer { get; set; }

        private IMovementStrategy _movementStrategy;

        public AgentEntity(int id, Vector2Int startPosition, IMovementStrategy movementStrategy = null)
            : base(id, startPosition)
        {
            CurrentPath = new List<Vector2Int>();
            _movementStrategy = movementStrategy ?? new DefaultMovementStrategy();
            Speed = 3.5f;
        }

        public void SetPath(List<Vector2Int> path)
        {
            CurrentPath = path;
        }

        public void SetMovementStrategy(IMovementStrategy strategy)
        {
            _movementStrategy = strategy;
        }

        public void Tick(float deltaTime)
        {
            _movementStrategy?.Execute(this, deltaTime);
        }

        public Vector2Int? GetFinalDestination()
        {
            if (CurrentPath != null && CurrentPath.Count > 0)
            {
                return CurrentPath[CurrentPath.Count - 1];
            }
            return null;
        }
    }
}
