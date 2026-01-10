using UnityEngine;
using Game.Gameplay.Entities;
using Game.Networking.Snapshot;

namespace Game.Simulation.Server.Managers
{
    public interface IPlayerManager
    {
        PlayerEntity LocalPlayer { get; }
        
        int RegisterPlayer(Vector3 worldPosition);
        void SendPlayerInput(int playerId, Vector2 input, bool wantToSprint, bool wantToDash = false);
        void Update(float deltaTime, int[,] gridData, int gridWidth, int gridHeight);
        EntitySnapshot? GetSnapshot(float timestamp);
    }
}
