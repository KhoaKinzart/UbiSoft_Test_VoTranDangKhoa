using System.Collections.Generic;
using UnityEngine;
using Game.Gameplay.Entities;
using Game.Core.Interfaces;

namespace Game.Simulation.Server.Managers
{
    public interface ICollectibleManager
    {
        IReadOnlyList<CollectibleEntity> Collectibles { get; }
        
        void SpawnCollectibles(int count);
        void CheckCollections(IEntity collector);
        void Replenish();
        List<int> GetActiveIDs();
        CollectibleEntity GetNearestCollectible(Vector2Int position);
    }
}
