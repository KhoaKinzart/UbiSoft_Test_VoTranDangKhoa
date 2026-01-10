using UnityEngine;
using Game.Core.Interfaces;
using Game.Core.Events;
using Game.Core.Constants;

namespace Game.Gameplay.Entities
{
    public class CollectibleEntity
    {
        public int ID { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public Vector2 Position => new Vector2(GridPosition.x, GridPosition.y);

        public CollectibleEntity(int id, Vector2Int position)
        {
            ID = id;
            GridPosition = position;
        }

        public void OnCollected(IEntity collector)
        {
            bool isPlayer = collector.ID == GameplayConstants.Network.LOCAL_PLAYER_ID;
            
            EventBus.Publish(new CollectibleCollectedEvent
            {
                CollectibleID = ID,
                CollectorID = collector.ID,
                Position = GridPosition,
                IsPlayer = isPlayer
            });

            string collectorType = isPlayer ? "PLAYER" : $"Bot {collector.ID}";
        }
    }
}
