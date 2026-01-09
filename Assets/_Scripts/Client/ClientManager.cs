using Client.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ServerManager serverManager;
    [SerializeField] private GameObject botPrefab;
    [SerializeField] private GameObject eggPrefab;

    [Header("Network Settings")]
    [Range(0.05f, 5f)]
    [SerializeField] private float interpolationDelay = 0.1f;

    private Queue<GameObject> botPool = new Queue<GameObject>();
    private Queue<GameObject> eggPool = new Queue<GameObject>();
    private Dictionary<int, GameObject> spawnedBots = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> spawnedEggs = new Dictionary<int, GameObject>();

    private Dictionary<int, List<BotSnapshot>> botHistoryBuffer = new Dictionary<int, List<BotSnapshot>>();
    private List<EggListSnapshot> eggHistoryBuffer = new List<EggListSnapshot>();

    private struct EggListSnapshot
    {
        public float Timestamp;
        public HashSet<int> ActiveEggIDs;
    }

    void Update()
    {
        ReceiveServerData();

        UpdateBotsVisuals();

        SyncEggsInterpolated();
    }

    private void ReceiveServerData()
    {
        float serverTime = Time.time;
        float retentionTime = serverTime - interpolationDelay - 2.0f;

        var snapshots = serverManager.GetLatestSnapshots();
        foreach (var snap in snapshots)
        {
            if (!botHistoryBuffer.ContainsKey(snap.BotID))
                botHistoryBuffer[snap.BotID] = new List<BotSnapshot>();

            var history = botHistoryBuffer[snap.BotID];

            if (history.Count == 0 || snap.Timestamp > history[history.Count - 1].Timestamp)
                history.Add(snap);

            while (history.Count > 0 && history[0].Timestamp < retentionTime)
                history.RemoveAt(0);
        }

        if (serverManager.Eggs != null)
        {
            EggListSnapshot eggSnap = new EggListSnapshot
            {
                Timestamp = serverTime,
                ActiveEggIDs = new HashSet<int>(serverManager.Eggs.Select(e => e.ID))
            };

            if (eggHistoryBuffer.Count == 0 || eggSnap.Timestamp > eggHistoryBuffer[eggHistoryBuffer.Count - 1].Timestamp)
                eggHistoryBuffer.Add(eggSnap);

            while (eggHistoryBuffer.Count > 0 && eggHistoryBuffer[0].Timestamp < retentionTime)
                eggHistoryBuffer.RemoveAt(0);
        }
    }

    private void UpdateBotsVisuals()
    {
        float renderTime = Time.time - interpolationDelay;

        foreach (var kvp in botHistoryBuffer)
        {
            int botId = kvp.Key;
            List<BotSnapshot> history = kvp.Value;

            if (!spawnedBots.ContainsKey(botId))
            {
                spawnedBots[botId] = GetBotFromPool(botId);
            }

            GameObject botObj = spawnedBots[botId];
            BotVisual visual = botObj.GetComponent<BotVisual>();

            if (InterpolationUtils.CalculateInterpolation(history, renderTime, out Vector2 newPos, out float newStamina))
            {
                botObj.transform.position = new Vector3(newPos.x, 0f, newPos.y);
                if (visual != null) visual.SetStamina(newStamina);
            }
        }
    }
    private void SyncEggsInterpolated()
    {
        float renderTime = Time.time - interpolationDelay;
        HashSet<int> activeIDsAtRenderTime = null;

        for (int i = eggHistoryBuffer.Count - 1; i >= 0; i--)
        {
            if (eggHistoryBuffer[i].Timestamp <= renderTime)
            {
                activeIDsAtRenderTime = eggHistoryBuffer[i].ActiveEggIDs;
                break;
            }
        }

        if (activeIDsAtRenderTime == null && eggHistoryBuffer.Count > 0)
            activeIDsAtRenderTime = eggHistoryBuffer[0].ActiveEggIDs;

        if (activeIDsAtRenderTime == null) return;

        var currentEggDataMap = serverManager.Eggs.ToDictionary(e => e.ID, e => e);

        foreach (int eggId in activeIDsAtRenderTime)
        {
            if (!spawnedEggs.ContainsKey(eggId))
            {
                if (currentEggDataMap.ContainsKey(eggId))
                {
                    var data = currentEggDataMap[eggId];
                    Vector3 spawnPos = new Vector3(data.GridPosition.x, 0.25f, data.GridPosition.y);

                    GameObject newEgg = GetEggFromPool(eggId, spawnPos);
                    spawnedEggs.Add(eggId, newEgg);
                }
            }
        }

        List<int> idsToRemove = new List<int>();
        foreach (var eggId in spawnedEggs.Keys)
        {
            if (!activeIDsAtRenderTime.Contains(eggId))
            {
                idsToRemove.Add(eggId);
            }
        }

        foreach (int id in idsToRemove)
        {
            ReturnEggToPool(spawnedEggs[id]);
            spawnedEggs.Remove(id);
        }
    }


    private GameObject GetBotFromPool(int id)
    {
        GameObject bot;
        if (botPool.Count > 0)
        {
            bot = botPool.Dequeue();
            bot.SetActive(true);
        }
        else
        {
            bot = Instantiate(botPrefab);
        }
        bot.name = $"Bot_{id}";
        return bot;
    }

    private void ReturnBotToPool(GameObject bot)
    {
        bot.SetActive(false);
        botPool.Enqueue(bot);
    }

    private GameObject GetEggFromPool(int id, Vector3 position)
    {
        GameObject egg;
        if (eggPool.Count > 0)
        {
            egg = eggPool.Dequeue();
            egg.SetActive(true);
        }
        else
        {
            egg = Instantiate(eggPrefab);
        }
        egg.transform.position = position;
        egg.name = $"Egg_{id}";
        return egg;
    }

    private void ReturnEggToPool(GameObject egg)
    {
        egg.SetActive(false);
        eggPool.Enqueue(egg);
    }
}