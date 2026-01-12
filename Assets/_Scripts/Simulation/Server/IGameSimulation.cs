using System.Collections.Generic;
using UnityEngine;
using Game.Networking.Snapshot;
using Game.Gameplay.Entities;

namespace Game.Simulation.Server
{
    public interface IGameSimulation
    {
        void Initialize(int botCount, int collectibleCount);
        void Tick(float deltaTime);
        int RegisterPlayer(Vector3 worldPosition);
        void SendPlayerInput(int playerId, Vector2 input, bool wantToSprint, bool wantToDash = false);
        List<EntitySnapshot> GetSnapshots();
        List<int> GetActiveCollectibleIDs();
        
        IReadOnlyList<CollectibleEntity> GetCollectibles();
        IReadOnlyList<AgentEntity> GetAgents();
        PlayerEntity GetLocalPlayer();
        
        int GetScore(int entityId);
        float GetTimeRemaining();
        bool IsGameOver();
        void StartGame(float duration);
    }
}
