using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Simulation.Server;
using Game.Networking.Snapshot;
using Game.Gameplay.Entities;

public class PresentationController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private GameObject collectiblePrefab;
    
    [Header("Hierarchy Organization")]
    [SerializeField] private Transform dynamicContainer;

    [Header("Interpolation")]
    [SerializeField] private float interpolationDelay = 0.55f; 
    [SerializeField] private float positionSmoothSpeed = 15f;

    [Header("Network Simulation")]
    [SerializeField] private bool simulateLatency = true;
    [SerializeField] private float minNetworkDelay = 0.1f;
    [SerializeField] private float maxNetworkDelay = 0.5f;

    private IGameSimulation _simulation;
    private GameObject _localPlayerObject;

    private Dictionary<int, GameObject> _spawnedBots = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> _spawnedCollectibles = new Dictionary<int, GameObject>();

    private Dictionary<int, List<EntitySnapshot>> _botHistoryBuffer = new Dictionary<int, List<EntitySnapshot>>();
    private Dictionary<int, Vector3> _botCurrentPositions = new Dictionary<int, Vector3>();
    private List<CollectibleSnapshot> _collectibleHistory = new List<CollectibleSnapshot>();

    private HashSet<int> _optimisticCollectedIDs = new HashSet<int>();

    private struct CollectibleSnapshot
    {
        public float Timestamp;
        public HashSet<int> ActiveIDs;
    }

    public void SetSimulation(IGameSimulation simulation)
    {
        _simulation = simulation;
        Invoke(nameof(InitializeCollectibles), 0.5f);
    }

    private void Start()
    {
        StartCoroutine(NetworkUpdateRoutine());
    }

    public void OnOptimisticCollect(int id)
    {
        if (!_optimisticCollectedIDs.Contains(id))
        {
            _optimisticCollectedIDs.Add(id);
        }

        if (_spawnedCollectibles.ContainsKey(id))
        {
            Destroy(_spawnedCollectibles[id]);
            _spawnedCollectibles.Remove(id);
        }
    }

    private void InitializeCollectibles()
    {
        var collectibles = _simulation.GetCollectibles();
        foreach (var collectible in collectibles)
        {
            SpawnCollectible(collectible);
        }
    }

    private void SpawnCollectible(CollectibleEntity collectible)
    {
        if (_optimisticCollectedIDs.Contains(collectible.ID)) return;

        if (!_spawnedCollectibles.ContainsKey(collectible.ID))
        {
            var obj = Instantiate(collectiblePrefab, dynamicContainer);
            obj.name = $"Collectible_{collectible.ID}";
            obj.transform.position = new Vector3(
                collectible.GridPosition.x,
                0.5f,
                collectible.GridPosition.y
            );

            if (obj.GetComponent<CollectibleDebugGizmos>() == null)
            {
                obj.AddComponent<CollectibleDebugGizmos>();
            }

            var data = obj.GetComponent<CollectibleVisualData>();
            if (data == null) data = obj.AddComponent<CollectibleVisualData>();
            data.ID = collectible.ID;

            _spawnedCollectibles.Add(collectible.ID, obj);
        }
    }

    private void Update()
    {
        if (_simulation == null) return;
        
        if (_simulation.IsGameOver())
        {
            return;
        }
        
        UpdateBotVisuals();
        UpdateCollectibleVisuals();
    }

    private IEnumerator NetworkUpdateRoutine()
    {
        while (true)
        {
            if (_simulation != null && !_simulation.IsGameOver())
            {
                ReceiveSnapshots();
            }

            if (simulateLatency)
            {
                float delay = Random.Range(minNetworkDelay, maxNetworkDelay);
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }
        }
    }

    private void ReceiveSnapshots()
    {
        float serverTime = Time.time;
        float retentionTime = serverTime - interpolationDelay - 1.0f; 

        var snapshots = _simulation.GetSnapshots();
        foreach (var snap in snapshots)
        {
            if (!_botHistoryBuffer.ContainsKey(snap.EntityID))
                _botHistoryBuffer[snap.EntityID] = new List<EntitySnapshot>();

            var history = _botHistoryBuffer[snap.EntityID];

            if (history.Count == 0 || snap.Timestamp > history[history.Count - 1].Timestamp)
                history.Add(snap);

            while (history.Count > 0 && history[0].Timestamp < retentionTime)
                history.RemoveAt(0);
        }

        var activeIDs = _simulation.GetActiveCollectibleIDs();
        var collectibleSnap = new CollectibleSnapshot
        {
            Timestamp = serverTime,
            ActiveIDs = new HashSet<int>(activeIDs)
        };

        if (_collectibleHistory.Count == 0 || collectibleSnap.Timestamp > _collectibleHistory[_collectibleHistory.Count - 1].Timestamp)
            _collectibleHistory.Add(collectibleSnap);

        while (_collectibleHistory.Count > 0 && _collectibleHistory[0].Timestamp < retentionTime)
            _collectibleHistory.RemoveAt(0);
    }

    private void UpdateBotVisuals()
    {
        float renderTime = Time.time - interpolationDelay;

        foreach (var kvp in _botHistoryBuffer)
        {
            int entityId = kvp.Key;
            List<EntitySnapshot> history = kvp.Value;

            if (entityId == 999) 
            {
                if (_localPlayerObject == null)
                    _localPlayerObject = GameObject.Find("LocalPlayer");

                if (_localPlayerObject != null)
                {
                    if (_localPlayerObject.GetComponent<ClientCollector>() == null)
                        _localPlayerObject.AddComponent<ClientCollector>();

                    if (_localPlayerObject.GetComponent<NetworkDebugGizmos>() == null)
                        _localPlayerObject.AddComponent<NetworkDebugGizmos>();

                    if (history.Count > 0)
                    {
                        var latestSnap = history[history.Count - 1];
                        
                        var gizmo = _localPlayerObject.GetComponent<NetworkDebugGizmos>();
                        if (gizmo != null) gizmo.UpdateServerPosition(latestSnap.Position);
                        
                        PlayerVisual playerVisual = _localPlayerObject.GetComponent<PlayerVisual>();
                        if (playerVisual != null)
                        {
                            playerVisual.SetStamina(latestSnap.Stamina);
                            var player = _simulation?.GetLocalPlayer();
                            if (player != null)
                            {
                                playerVisual.SetDashState(player.IsDashing);
                                playerVisual.SetDashCooldown(player.DashCooldownTimer);
                            }
                        }
                    }
                }
            }
            else 
            {
                if (!_spawnedBots.ContainsKey(entityId))
                {
                    _spawnedBots[entityId] = Instantiate(botPrefab, dynamicContainer);
                    _spawnedBots[entityId].name = $"Bot_{entityId}";
                    
                    float yOffset = (entityId % 5) * 0.05f;
                    _spawnedBots[entityId].transform.position = new Vector3(0, yOffset, 0);
                    
                    _spawnedBots[entityId].AddComponent<Game.Debugging.AgentDebugGizmos>();
                    _spawnedBots[entityId].AddComponent<NetworkDebugGizmos>();
                }

                GameObject botObj = _spawnedBots[entityId];
                BotVisual visual = botObj.GetComponent<BotVisual>();
                
                if (history.Count > 0)
                {
                    var latestSnap = history[history.Count - 1];
                    var netGizmo = botObj.GetComponent<NetworkDebugGizmos>();
                    if (netGizmo != null) netGizmo.UpdateServerPosition(latestSnap.Position);
                }

                float baseYOffset = (entityId % 5) * 0.05f;
                if (GetInterpolatedPosition(history, renderTime, out Vector2 targetPos, out float newStamina))
                {
                    Vector3 targetPosition3D = new Vector3(targetPos.x, baseYOffset, targetPos.y);

                    if (!_botCurrentPositions.ContainsKey(entityId))
                    {
                        _botCurrentPositions[entityId] = targetPosition3D;
                        botObj.transform.position = targetPosition3D;
                    }
                    else
                    {
                        Vector3 smoothed = Vector3.Lerp(
                            botObj.transform.position,
                            targetPosition3D,
                            positionSmoothSpeed * Time.deltaTime
                        );
                        botObj.transform.position = smoothed;
                        _botCurrentPositions[entityId] = smoothed;
                    }

                    UpdateAgentDebugGizmos(botObj, entityId, baseYOffset);

                    if (visual != null) visual.SetStamina(newStamina);
                }
            }
        }
    }

    private void UpdateAgentDebugGizmos(GameObject botObj, int entityId, float yOffset)
    {
        var agents = _simulation.GetAgents();
        foreach (var agent in agents)
        {
            if (agent.ID == entityId)
            {
                var debugGizmo = botObj.GetComponent<Game.Debugging.AgentDebugGizmos>();
                if (debugGizmo != null)
                {
                    var destination = agent.GetFinalDestination();
                    if (destination.HasValue)
                        debugGizmo.SetTarget(new Vector3(destination.Value.x, yOffset, destination.Value.y), entityId);
                    else
                        debugGizmo.ClearTarget();
                }
                break;
            }
        }
    }

    private void UpdateCollectibleVisuals()
    {
        float renderTime = Time.time - interpolationDelay;
        HashSet<int> activeIDsAtRenderTime = null;

        for (int i = _collectibleHistory.Count - 1; i >= 0; i--)
        {
            if (_collectibleHistory[i].Timestamp <= renderTime)
            {
                activeIDsAtRenderTime = _collectibleHistory[i].ActiveIDs;
                break;
            }
        }

        if (activeIDsAtRenderTime == null && _collectibleHistory.Count > 0)
            activeIDsAtRenderTime = _collectibleHistory[0].ActiveIDs;

        if (activeIDsAtRenderTime == null) return;

        var collectibles = _simulation.GetCollectibles();

        foreach (var collectible in collectibles)
        {
            if (activeIDsAtRenderTime.Contains(collectible.ID))
            {
                if (_optimisticCollectedIDs.Contains(collectible.ID)) continue;

                SpawnCollectible(collectible);
            }
            else
            {
                if (_optimisticCollectedIDs.Contains(collectible.ID))
                {
                    _optimisticCollectedIDs.Remove(collectible.ID);
                }
            }
        }

        List<int> idsToRemove = new List<int>();
        foreach (var id in _spawnedCollectibles.Keys)
        {
            if (!activeIDsAtRenderTime.Contains(id))
            {
                idsToRemove.Add(id);
            }
        }

        foreach (int id in idsToRemove)
        {
            Destroy(_spawnedCollectibles[id]);
            _spawnedCollectibles.Remove(id);
        }
    }

    private bool GetInterpolatedPosition(List<EntitySnapshot> history, float renderTime, out Vector2 position, out float stamina)
    {
        position = Vector2.zero;
        stamina = 100f;

        if (history.Count == 0) return false;

        if (history.Count == 1)
        {
            position = history[0].Position;
            stamina = history[0].Stamina;
            return true;
        }

        if (renderTime <= history[0].Timestamp)
        {
            position = history[0].Position;
            stamina = history[0].Stamina;
            return true;
        }

        if (renderTime >= history[history.Count - 1].Timestamp)
        {
            var last = history[history.Count - 1];
            if (history.Count >= 2)
            {
                var secondLast = history[history.Count - 2];
                float dt = last.Timestamp - secondLast.Timestamp;
                if (dt > 0)
                {
                    Vector2 velocity = (last.Position - secondLast.Position) / dt;
                    float extrapolationTime = Mathf.Min(renderTime - last.Timestamp, 0.25f);
                    position = last.Position + velocity * extrapolationTime;
                    stamina = last.Stamina;
                    return true;
                }
            }
            position = last.Position;
            stamina = last.Stamina;
            return true;
        }

        for (int i = 0; i < history.Count - 1; i++)
        {
            if (history[i].Timestamp <= renderTime && renderTime <= history[i + 1].Timestamp)
            {
                float duration = history[i + 1].Timestamp - history[i].Timestamp;
                float t = duration > 0.001f ? (renderTime - history[i].Timestamp) / duration : 0f;
                t = Mathf.Clamp01(t);

                position = Vector2.Lerp(history[i].Position, history[i + 1].Position, t);
                stamina = Mathf.Lerp(history[i].Stamina, history[i + 1].Stamina, t);
                return true;
            }
        }

        position = history[history.Count - 1].Position;
        stamina = history[history.Count - 1].Stamina;
        return true;
    }
}