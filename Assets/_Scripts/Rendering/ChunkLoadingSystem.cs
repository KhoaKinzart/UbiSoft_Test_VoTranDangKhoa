using System.Collections.Generic;
using UnityEngine;

namespace Game.Rendering
{
    public class ChunkLoadingSystem : MonoBehaviour
    {
        [Header("Chunk Settings")]
        [SerializeField] private int chunkSize = 16;
        [SerializeField] private float loadDistance = 32f;
        [SerializeField] private float unloadDistance = 48f;
        
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform playerTransform;
        
        [Header("Performance")]
        [SerializeField] private float updateInterval = 0.5f;
        
        [Header("Debug Stats (Runtime)")]
        [SerializeField] private int totalObstacles = 0;
        [SerializeField] private int activeObstacles = 0;
        [SerializeField] private int loadedChunks = 0;
        [SerializeField] private int totalChunks = 0;
        [SerializeField] private float efficiencyPercent = 0f;
        
        private Dictionary<Vector2Int, MapChunk> _chunks;
        private HashSet<Vector2Int> _loadedChunks;
        private float _updateTimer;
        
        private int _mapWidth;
        private int _mapHeight;
        private int[,] _gridData;
        private GameObject _obstaclePrefab;
        private Transform _mapParent;
        
        public int ChunkSize => chunkSize;
        public int LoadedChunkCount => _loadedChunks.Count;
        public int TotalChunkCount => _chunks.Count;
        
        private void Awake()
        {
            _chunks = new Dictionary<Vector2Int, MapChunk>();
            _loadedChunks = new HashSet<Vector2Int>();
            
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            TryFindPlayer();
        }
        
        private void TryFindPlayer()
        {
            if (playerTransform != null) return;
            
            GameObject player = GameObject.Find("LocalPlayer");
            if (player == null)
            {
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name == "LocalPlayer" || obj.GetComponent<PlayerInput>() != null)
                    {
                        player = obj;
                        break;
                    }
                }
            }
            
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        private void Update()
        {
            _updateTimer += Time.deltaTime;
            
            if (_updateTimer >= updateInterval)
            {
                UpdateChunkLoading();
                _updateTimer = 0f;
            }
        }
        
        public void Initialize(int[,] gridData, int width, int height, GameObject obstaclePrefab, Transform mapParent)
        {
            _gridData = gridData;
            _mapWidth = width;
            _mapHeight = height;
            _obstaclePrefab = obstaclePrefab;
            _mapParent = mapParent;
            
            CreateChunks();
        }
        
        private void CreateChunks()
        {
            _chunks.Clear();
            
            int chunksX = Mathf.CeilToInt((float)_mapWidth / chunkSize);
            int chunksZ = Mathf.CeilToInt((float)_mapHeight / chunkSize);
            
            totalObstacles = 0;
            
            for (int cx = 0; cx < chunksX; cx++)
            {
                for (int cz = 0; cz < chunksZ; cz++)
                {
                    Vector2Int chunkCoord = new Vector2Int(cx, cz);
                    
                    int startX = cx * chunkSize;
                    int startZ = cz * chunkSize;
                    int endX = Mathf.Min(startX + chunkSize, _mapWidth);
                    int endZ = Mathf.Min(startZ + chunkSize, _mapHeight);
                    
                    Vector3 center = new Vector3(
                        (startX + endX - 1) * 0.5f,
                        0.5f,
                        (startZ + endZ - 1) * 0.5f
                    );
                    
                    Vector3 size = new Vector3(
                        endX - startX,
                        1f,
                        endZ - startZ
                    );
                    
                    Bounds chunkBounds = new Bounds(center, size);
                    MapChunk chunk = new MapChunk(chunkCoord, chunkBounds, _mapParent);
                    
                    for (int x = startX; x < endX; x++)
                    {
                        for (int z = startZ; z < endZ; z++)
                        {
                            if (_gridData[x, z] == 1)
                            {
                                GameObject obstacle = Instantiate(
                                    _obstaclePrefab,
                                    new Vector3(x, 0.5f, z),
                                    Quaternion.identity,
                                    _mapParent
                                );
                                
                                obstacle.SetActive(false);
                                chunk.AddObstacle(obstacle);
                                totalObstacles++;
                            }
                        }
                    }
                    
                    _chunks[chunkCoord] = chunk;
                }
            }
            
            totalChunks = _chunks.Count;
        }
        
        private void UpdateChunkLoading()
        {
            if (_chunks.Count == 0) return;
            
            if (playerTransform == null)
                TryFindPlayer();
            
            Vector3 cameraPos = mainCamera != null ? mainCamera.transform.position : Vector3.zero;
            
            if (playerTransform != null)
            {
                cameraPos = playerTransform.position;
            }
            
            HashSet<Vector2Int> chunksToLoad = new HashSet<Vector2Int>();
            
            int prevLoadedCount = _loadedChunks.Count;
            
            foreach (var kvp in _chunks)
            {
                Vector2Int chunkCoord = kvp.Key;
                MapChunk chunk = kvp.Value;
                
                float distance = Vector3.Distance(cameraPos, chunk.ChunkBounds.center);
                
                if (distance <= loadDistance)
                {
                    chunksToLoad.Add(chunkCoord);
                    
                    if (!chunk.IsLoaded)
                    {
                        chunk.Load();
                        _loadedChunks.Add(chunkCoord);
                    }
                }
                else if (distance > unloadDistance && chunk.IsLoaded)
                {
                    chunk.Unload();
                    _loadedChunks.Remove(chunkCoord);
                }
            }
            
            UpdateDebugStats();
        }
        
        private void UpdateDebugStats()
        {
            loadedChunks = _loadedChunks.Count;
            totalChunks = _chunks.Count;
            
            activeObstacles = 0;
            foreach (var chunk in _chunks.Values)
            {
                if (chunk.IsLoaded)
                {
                    activeObstacles += chunk.ObstacleCount;
                }
            }
            
            if (totalObstacles > 0)
            {
                efficiencyPercent = ((totalObstacles - activeObstacles) / (float)totalObstacles) * 100f;
            }
        }
        
        public void SetLoadDistance(float distance)
        {
            loadDistance = distance;
            unloadDistance = distance * 1.5f;
        }
        
        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }
        
        public void UnloadAllChunks()
        {
            foreach (var chunk in _chunks.Values)
            {
                chunk.Unload();
            }
            
            _loadedChunks.Clear();
        }
        
        public void LoadAllChunks()
        {
            foreach (var chunk in _chunks.Values)
            {
                chunk.Load();
                _loadedChunks.Add(chunk.ChunkCoord);
            }
        }
        
        private void OnDestroy()
        {
            if (_chunks != null)
            {
                foreach (var chunk in _chunks.Values)
                {
                    chunk.Destroy();
                }
                
                _chunks.Clear();
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_chunks == null || _chunks.Count == 0) return;
            
            foreach (var chunk in _chunks.Values)
            {
                Gizmos.color = chunk.IsLoaded ? Color.green : Color.red;
                Gizmos.DrawWireCube(chunk.ChunkBounds.center, chunk.ChunkBounds.size);
            }
            
            if (mainCamera != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(mainCamera.transform.position, loadDistance);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(mainCamera.transform.position, unloadDistance);
            }
        }
    }
}
