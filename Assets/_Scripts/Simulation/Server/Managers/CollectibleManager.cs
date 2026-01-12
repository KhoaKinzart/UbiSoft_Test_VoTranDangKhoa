using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Gameplay.Entities;
using Game.Gameplay.Systems;
using Game.Core.Interfaces;

namespace Game.Simulation.Server.Managers
{
    public class CollectibleManager : ICollectibleManager
    {
        private readonly List<CollectibleEntity> _collectibles;
        private readonly CollectionSystem _collectionSystem;
        private readonly int[,] _gridData;
        private readonly int _gridWidth;
        private readonly int _gridHeight;
        
        private int _maxCollectibles;
        private int _nextCollectibleId;

        public IReadOnlyList<CollectibleEntity> Collectibles => _collectibles;

        public CollectibleManager(int[,] gridData, int gridWidth, int gridHeight)
        {
            _collectibles = new List<CollectibleEntity>();
            _collectionSystem = new CollectionSystem();
            _gridData = gridData;
            _gridWidth = gridWidth;
            _gridHeight = gridHeight;
            _nextCollectibleId = 0;
        }

        public void SpawnCollectibles(int count)
        {
            _maxCollectibles = count;
            
            for (int i = 0; i < count; i++)
            {
                Vector2Int pos = GetRandomWalkablePosition();
                _collectibles.Add(new CollectibleEntity(_nextCollectibleId++, pos));
            }
        }

        public void CheckCollections(IEntity collector)
        {
            _collectionSystem.CheckCollections(collector, _collectibles);
        }

        public void Replenish()
        {
            int needed = _maxCollectibles - _collectibles.Count;

            for (int i = 0; i < needed; i++)
            {
                Vector2Int pos = GetRandomWalkablePosition();
                var newCollectible = new CollectibleEntity(_nextCollectibleId++, pos);
                _collectibles.Add(newCollectible);
            }
        }

        public List<int> GetActiveIDs()
        {
            return _collectibles.Select(c => c.ID).ToList();
        }

        public CollectibleEntity GetNearestCollectible(Vector2Int position)
        {
            if (_collectibles.Count == 0) return null;

            return _collectibles.OrderBy(c =>
                Mathf.Abs(position.x - c.GridPosition.x) +
                Mathf.Abs(position.y - c.GridPosition.y)
            ).FirstOrDefault();
        }

        private Vector2Int GetRandomWalkablePosition()
        {
            int x, y;
            int maxTries = 100;
            do
            {
                x = Random.Range(1, _gridWidth - 1);
                y = Random.Range(1, _gridHeight - 1);
                maxTries--;
            }
            while (_gridData[x, y] == 1 && maxTries > 0);

            return new Vector2Int(x, y);
        }
    }
}
