using UnityEngine;
using Game.Gameplay.Entities;
using Game.Networking.Snapshot;
using Game.Core.Constants;

namespace Game.Simulation.Server.Managers
{
    public class PlayerManager : IPlayerManager
    {
        public PlayerEntity LocalPlayer { get; private set; }

        public int RegisterPlayer(Vector3 worldPosition)
        {
            Vector2Int gridPos = new Vector2Int(
                Mathf.RoundToInt(worldPosition.x),
                Mathf.RoundToInt(worldPosition.z)
            );

            LocalPlayer = new PlayerEntity(GameplayConstants.Network.LOCAL_PLAYER_ID, gridPos);
            
            return GameplayConstants.Network.LOCAL_PLAYER_ID;
        }

        public void SendPlayerInput(int playerId, Vector2 input, bool wantToSprint, bool wantToDash = false)
        {
            if (LocalPlayer != null && LocalPlayer.ID == playerId)
            {
                LocalPlayer.LastInput = input;
                LocalPlayer.WantToSprint = wantToSprint;
                
                if (wantToDash && !LocalPlayer.WantToDash)
                {
                    LocalPlayer.WantToDash = true;
                }
            }
        }

        public void Update(float deltaTime, int[,] gridData, int gridWidth, int gridHeight)
        {
            if (LocalPlayer == null) return;

            LocalPlayer.Tick(5f, deltaTime, gridData, gridWidth, gridHeight);
        }

        public EntitySnapshot? GetSnapshot(float timestamp)
        {
            if (LocalPlayer == null) return null;

            return new EntitySnapshot
            {
                EntityID = LocalPlayer.ID,
                Position = LocalPlayer.Position,
                Stamina = LocalPlayer.CurrentStamina,
                Timestamp = timestamp
            };
        }
    }
}
