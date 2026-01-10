using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Gameplay.Entities;
using Game.Simulation.Movement;
using Game.Simulation.Pathfinding;
using Game.Networking.Snapshot;

namespace Game.Simulation.Server.Managers
{
    public class AgentManager : IAgentManager
    {
        private readonly List<AgentEntity> _agents;
        private readonly IPathfindingService _pathfinding;
        private readonly ICollectibleManager _collectibleManager;
        private readonly int[,] _gridData;
        private readonly int _gridWidth;
        private readonly int _gridHeight;

        public IReadOnlyList<AgentEntity> Agents => _agents;

        public AgentManager(
            int[,] gridData, 
            int gridWidth, 
            int gridHeight,
            IPathfindingService pathfinding,
            ICollectibleManager collectibleManager)
        {
            _agents = new List<AgentEntity>();
            _gridData = gridData;
            _gridWidth = gridWidth;
            _gridHeight = gridHeight;
            _pathfinding = pathfinding;
            _collectibleManager = collectibleManager;
        }

        public void SpawnAgents(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2Int pos = GetRandomWalkablePosition();
                var agent = new AgentEntity(i, pos, new DefaultMovementStrategy());
                AssignNearestCollectibleAsTarget(agent);
                _agents.Add(agent);
            }
        }

        public void Update(float deltaTime)
        {
            foreach (var agent in _agents)
            {
                agent.Tick(deltaTime);
                _collectibleManager.CheckCollections(agent);

                if (!agent.IsMoving || IsTargetCollectibleGone(agent))
                {
                    AssignNearestCollectibleAsTarget(agent);
                }
            }
        }

        public List<EntitySnapshot> GetSnapshots(float timestamp)
        {
            var snapshots = new List<EntitySnapshot>();

            foreach (var agent in _agents)
            {
                snapshots.Add(new EntitySnapshot
                {
                    EntityID = agent.ID,
                    Position = agent.Position,
                    Stamina = agent.CurrentStamina,
                    Timestamp = timestamp
                });
            }

            return snapshots;
        }

        private bool IsTargetCollectibleGone(AgentEntity agent)
        {
            if (!agent.IsMoving) return false;

            Vector2Int? finalDest = agent.GetFinalDestination();
            if (!finalDest.HasValue) return false;

            foreach (var collectible in _collectibleManager.Collectibles)
            {
                if (collectible.GridPosition == finalDest.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private void AssignNearestCollectibleAsTarget(AgentEntity agent)
        {
            if (_collectibleManager.Collectibles.Count == 0)
            {
                agent.SetPath(new List<Vector2Int>());
                return;
            }

            var availableCollectibles = _collectibleManager.Collectibles.ToList();
            
            // Prioritize collectibles that are NOT already targeted by other agents
            var untargetedCollectibles = availableCollectibles.Where(c =>
            {
                Vector2Int collectiblePos = c.GridPosition;
                int agentsTargetingThis = 0;
                
                foreach (var otherAgent in _agents)
                {
                    if (otherAgent.ID == agent.ID) continue;
                    
                    var destination = otherAgent.GetFinalDestination();
                    if (destination.HasValue && destination.Value == collectiblePos)
                    {
                        agentsTargetingThis++;
                    }
                }
                
                return agentsTargetingThis == 0;
            }).ToList();

            // If all collectibles are targeted, allow sharing but prefer less crowded ones
            var candidates = untargetedCollectibles.Count > 0 ? untargetedCollectibles : availableCollectibles;

            var sorted = candidates.OrderBy(c =>
            {
                float distance = Mathf.Abs(agent.GridPosition.x - c.GridPosition.x) +
                                 Mathf.Abs(agent.GridPosition.y - c.GridPosition.y);
                
                // Add penalty for crowded collectibles
                int agentsTargeting = 0;
                foreach (var otherAgent in _agents)
                {
                    if (otherAgent.ID == agent.ID) continue;
                    var dest = otherAgent.GetFinalDestination();
                    if (dest.HasValue && dest.Value == c.GridPosition)
                    {
                        agentsTargeting++;
                    }
                }
                
                return distance + (agentsTargeting * 5f);
            }).ToList();

            foreach (var collectible in sorted)
            {
                var path = _pathfinding.FindPath(agent.GridPosition, collectible.GridPosition);
                if (path != null && path.Count > 0)
                {
                    agent.SetPath(path);
                    return;
                }
            }

            agent.SetPath(new List<Vector2Int>());
        }

        private Vector2Int GetRandomWalkablePosition()
        {
            const int minSeparationDistance = 3;
            int maxTries = 100;
            
            for (int attempt = 0; attempt < maxTries; attempt++)
            {
                int x = Random.Range(1, _gridWidth - 1);
                int y = Random.Range(1, _gridHeight - 1);
                Vector2Int candidate = new Vector2Int(x, y);
                
                // Check if walkable
                if (_gridData[x, y] == 1)
                    continue;
                
                // Check minimum separation from other agents
                bool tooClose = false;
                foreach (var existingAgent in _agents)
                {
                    float distance = Vector2Int.Distance(candidate, existingAgent.GridPosition);
                    if (distance < minSeparationDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                    return candidate;
            }
            
            // Fallback: just find any walkable position
            for (int attempt = 0; attempt < maxTries; attempt++)
            {
                int x = Random.Range(1, _gridWidth - 1);
                int y = Random.Range(1, _gridHeight - 1);
                
                if (_gridData[x, y] != 1)
                    return new Vector2Int(x, y);
            }

            return new Vector2Int(_gridWidth / 2, _gridHeight / 2);
        }
    }
}
