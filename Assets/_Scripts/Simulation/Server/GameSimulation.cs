using System.Collections.Generic;
using UnityEngine;
using Game.Gameplay.Entities;
using Game.Simulation.Pathfinding;
using Game.Networking.Snapshot;
using Game.Simulation.Server.Managers;
using Game.Gameplay.Systems;

namespace Game.Simulation.Server
{
    public class GameSimulation : IGameSimulation
    {
        private readonly int[,] _gridData;
        private readonly int _gridWidth;
        private readonly int _gridHeight;
        
        private readonly IPlayerManager _playerManager;
        private readonly ICollectibleManager _collectibleManager;
        private readonly IAgentManager _agentManager;
        private readonly IScoreTracker _scoreTracker;
        private readonly IGameTimer _gameTimer;
        private readonly IGameStateManager _gameStateManager;

        public GameSimulation(int[,] gridData, int width, int height, IPathfindingService pathfinding)
        {
            _gridData = gridData;
            _gridWidth = width;
            _gridHeight = height;
            
            _playerManager = new PlayerManager();
            _collectibleManager = new CollectibleManager(gridData, width, height);
            _agentManager = new AgentManager(gridData, width, height, pathfinding, _collectibleManager);
            
            _scoreTracker = new ScoreSystem();
            _gameTimer = new GameTimerSystem();
            _gameStateManager = new GameStateManager(_gameTimer, _scoreTracker);
        }

        public void Initialize(int botCount, int collectibleCount)
        {
            _collectibleManager.SpawnCollectibles(collectibleCount);
            _agentManager.SpawnAgents(botCount);
        }

        public void Tick(float deltaTime)
        {
            _gameStateManager.Update(deltaTime);
            
            if (IsGameOver())
            {
                return;
            }
            
            _playerManager.Update(deltaTime, _gridData, _gridWidth, _gridHeight);
            
            if (_playerManager.LocalPlayer != null)
            {
                _collectibleManager.CheckCollections(_playerManager.LocalPlayer);
            }
            
            _agentManager.Update(deltaTime);
            _collectibleManager.Replenish();
        }

        public int RegisterPlayer(Vector3 worldPosition)
        {
            return _playerManager.RegisterPlayer(worldPosition);
        }

        public void SendPlayerInput(int playerId, Vector2 input, bool wantToSprint, bool wantToDash = false)
        {
            _playerManager.SendPlayerInput(playerId, input, wantToSprint, wantToDash);
        }

        public List<EntitySnapshot> GetSnapshots()
        {
            var snapshots = new List<EntitySnapshot>();
            float timestamp = Time.time;

            var playerSnapshot = _playerManager.GetSnapshot(timestamp);
            if (playerSnapshot.HasValue)
            {
                snapshots.Add(playerSnapshot.Value);
            }

            snapshots.AddRange(_agentManager.GetSnapshots(timestamp));

            return snapshots;
        }

        public List<int> GetActiveCollectibleIDs()
        {
            return _collectibleManager.GetActiveIDs();
        }

        public IReadOnlyList<CollectibleEntity> GetCollectibles()
        {
            return _collectibleManager.Collectibles;
        }

        public IReadOnlyList<AgentEntity> GetAgents()
        {
            return _agentManager.Agents;
        }

        public PlayerEntity GetLocalPlayer()
        {
            return _playerManager.LocalPlayer;
        }

        public int GetScore(int entityId)
        {
            return _scoreTracker.GetScore(entityId);
        }

        public float GetTimeRemaining()
        {
            return _gameTimer.TimeRemaining;
        }

        public bool IsGameOver()
        {
            return _gameStateManager.CurrentState == GameState.GameOver;
        }

        public void StartGame(float duration)
        {
            _gameStateManager.StartGame(duration);
        }
    }
}
