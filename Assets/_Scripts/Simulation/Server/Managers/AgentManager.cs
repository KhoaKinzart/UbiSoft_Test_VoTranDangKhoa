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
        
        private readonly Dictionary<int, float> _agentReEvaluationTimers;
        private const float RE_EVALUATION_INTERVAL = 0.5f;

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
            _agentReEvaluationTimers = new Dictionary<int, float>();
        }

        public void SpawnAgents(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2Int pos = GetRandomWalkablePosition();
                var agent = new AgentEntity(i, pos, new DefaultMovementStrategy());
                AssignNearestCollectibleAsTarget(agent);
                _agents.Add(agent);
                _agentReEvaluationTimers[i] = 0f;
            }
        }

        public void Update(float deltaTime)
        {
            foreach (var agent in _agents)
            {
                agent.Tick(deltaTime);
                _collectibleManager.CheckCollections(agent);

                if (!_agentReEvaluationTimers.ContainsKey(agent.ID))
                {
                    _agentReEvaluationTimers[agent.ID] = 0f;
                }
                
                _agentReEvaluationTimers[agent.ID] += deltaTime;
                
                bool shouldReEvaluate = false;
                
                if (!agent.IsMoving)
                {
                    shouldReEvaluate = true;
                }
                else if (IsTargetCollectibleGone(agent))
                {
                    shouldReEvaluate = true;
                }
                else if (_agentReEvaluationTimers[agent.ID] >= RE_EVALUATION_INTERVAL)
                {
                    if (HasCloserCollectible(agent))
                    {
                        shouldReEvaluate = true;
                    }
                }
                
                if (shouldReEvaluate)
                {
                    AssignNearestCollectibleAsTarget(agent);
                    _agentReEvaluationTimers[agent.ID] = 0f;
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
        
        private bool HasCloserCollectible(AgentEntity agent)
        {
            if (_collectibleManager.Collectibles.Count == 0)
                return false;
            
            Vector2Int? currentTarget = agent.GetFinalDestination();
            if (!currentTarget.HasValue)
                return false;
            
            float currentTargetDistance = Mathf.Abs(agent.GridPosition.x - currentTarget.Value.x) +
                                          Mathf.Abs(agent.GridPosition.y - currentTarget.Value.y);
            
            foreach (var collectible in _collectibleManager.Collectibles)
            {
                if (collectible.GridPosition == currentTarget.Value)
                    continue;
                
                float distance = Mathf.Abs(agent.GridPosition.x - collectible.GridPosition.x) +
                                 Mathf.Abs(agent.GridPosition.y - collectible.GridPosition.y);
                
                if (distance < currentTargetDistance - 2f)
                {
                    return true;
                }
            }
            
            return false;
        }

        private void AssignNearestCollectibleAsTarget(AgentEntity agent)
        {
            if (_collectibleManager.Collectibles.Count == 0)
            {
                agent.SetPath(new List<Vector2Int>());
                return;
            }

            var availableCollectibles = _collectibleManager.Collectibles.ToList();
            
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

            var candidates = untargetedCollectibles.Count > 0 ? untargetedCollectibles : availableCollectibles;

            var sorted = candidates.OrderBy(c =>
            {
                float distance = Mathf.Abs(agent.GridPosition.x - c.GridPosition.x) +
                                 Mathf.Abs(agent.GridPosition.y - c.GridPosition.y);
                
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
                
                if (_gridData[x, y] == 1)
                    continue;
                
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
