using System.Collections.Generic;
using UnityEngine;

namespace Game.Rendering
{
    public class MapChunk
    {
        public Vector2Int ChunkCoord { get; private set; }
        public Bounds ChunkBounds { get; private set; }
        public bool IsLoaded { get; private set; }
        
        private readonly List<GameObject> _obstacles;
        private readonly Transform _parent;
        
        public MapChunk(Vector2Int chunkCoord, Bounds bounds, Transform parent)
        {
            ChunkCoord = chunkCoord;
            ChunkBounds = bounds;
            _parent = parent;
            _obstacles = new List<GameObject>();
            IsLoaded = false;
        }
        
        public void AddObstacle(GameObject obstacle)
        {
            _obstacles.Add(obstacle);
        }
        
        public void Load()
        {
            if (IsLoaded) return;
            
            foreach (var obstacle in _obstacles)
            {
                if (obstacle != null)
                    obstacle.SetActive(true);
            }
            
            IsLoaded = true;
        }
        
        public void Unload()
        {
            if (!IsLoaded) return;
            
            foreach (var obstacle in _obstacles)
            {
                if (obstacle != null)
                    obstacle.SetActive(false);
            }
            
            IsLoaded = false;
        }
        
        public void Destroy()
        {
            foreach (var obstacle in _obstacles)
            {
                if (obstacle != null)
                    Object.Destroy(obstacle);
            }
            
            _obstacles.Clear();
        }
        
        public int ObstacleCount => _obstacles.Count;
    }
}
