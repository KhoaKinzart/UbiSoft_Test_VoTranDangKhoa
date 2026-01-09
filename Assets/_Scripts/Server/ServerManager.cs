using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class ServerManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int botCount = 20;
    [SerializeField] private int eggCount = 10;
    [SerializeField] private float botSpeed = 5f;

    [Header("Network Simulation")]
    [Tooltip("Số lần update mỗi giây (Hz). Ví dụ 20 nghĩa là 0.05s gửi 1 lần.")]
    [SerializeField] private int tickRate = 20;

    [Header("References")]
    [SerializeField] private GridManager gridManager;

    private List<BotEntity> bots = new List<BotEntity>();
    private List<EggEntity> eggs = new List<EggEntity>();

    public IReadOnlyList<EggEntity> Eggs => eggs;

    private AStar pathfindingEngine;
    private bool isServerReady = false;

    void Start()
    {
        StartCoroutine(WaitForMap());
    }

    private System.Collections.IEnumerator WaitForMap()
    {
        while (gridManager == null || !gridManager.IsMapReady)
        {
            yield return null;
        }
        InitializeServer();
    }

    private void InitializeServer()
    {
        pathfindingEngine = new AStar(gridManager.CurrentWidth, gridManager.CurrentHeight);
        pathfindingEngine.UpdateGridObstacles(gridManager.GridData);

        SpawnEggs();
        SpawnBots();

        isServerReady = true;
        Debug.Log("Server Simulation Started!");
    }

    void Update()
    {
        if (!isServerReady) return;

        foreach (var bot in bots)
        {
            bot.UpdateLogic(Time.deltaTime);
            bot.Move(botSpeed, Time.deltaTime);

            CheckEggCollection(bot);

            if (!bot.IsMoving)
            {
                AssignNearestEggAsTarget(bot);
            }
        }

        if (eggs.Count < eggCount / 2)
        {
            Vector2Int pos = GetRandomWalkablePosition();
            eggs.Add(new EggEntity(Random.Range(10000, 99999), pos));
        }
    }


    public List<BotSnapshot> GetLatestSnapshots()
    {
        List<BotSnapshot> snapshots = new List<BotSnapshot>();
        float serverTime = Time.time; 

        foreach (var bot in bots)
        {
            snapshots.Add(new BotSnapshot
            {
                BotID = bot.ID,
                Position = bot.Position,
                Stamina = bot.CurrentStamina,
                Timestamp = serverTime
            });
        }
        return snapshots;
    }


    private void SpawnEggs()
    {
        for (int i = 0; i < eggCount; i++)
        {
            Vector2Int pos = GetRandomWalkablePosition();
            eggs.Add(new EggEntity(i, pos));
        }
    }

    private void SpawnBots()
    {
        for (int i = 0; i < botCount; i++)
        {
            Vector2Int startPos;
            bool isOverlapping;
            int attempts = 0; 

            do
            {
                startPos = GetRandomWalkablePosition();
                isOverlapping = false;
                attempts++;

                foreach (var egg in eggs)
                {
                    if (egg.GridPosition == startPos)
                    {
                        isOverlapping = true;
                        break;
                    }
                }

                if (!isOverlapping)
                {
                    foreach (var b in bots)
                    {
                        if (b.GridPosition == startPos)
                        {
                            isOverlapping = true;
                            break;
                        }
                    }
                }

            } while (isOverlapping && attempts < 50); 

            BotEntity newBot = new BotEntity(i, startPos);

            AssignNearestEggAsTarget(newBot);

            bots.Add(newBot);
        }
    }

    private void CheckEggCollection(BotEntity bot)
    {
        for (int i = eggs.Count - 1; i >= 0; i--)
        {
            if (eggs[i].GridPosition == bot.GridPosition)
            {
                Vector2Int eatenEggPos = eggs[i].GridPosition;

                eggs.RemoveAt(i);

                bot.SetPath(null);

                foreach (var otherBot in bots)
                {
                    if (otherBot == bot) continue;

                    Vector2Int? dest = otherBot.GetFinalDestination();
                    if (dest.HasValue && dest.Value == eatenEggPos)
                    {
                        otherBot.SetPath(null); 
                    }
                }
                break;
            }
        }
    }

    private void AssignNearestEggAsTarget(BotEntity bot)
    {
        if (eggs.Count == 0) return;

        var sortedEggs = eggs.OrderBy(e =>
            Mathf.Abs(bot.GridPosition.x - e.GridPosition.x) +
            Mathf.Abs(bot.GridPosition.y - e.GridPosition.y)
        ).ToList();

        foreach (var egg in sortedEggs)
        {
            List<Vector2Int> path = pathfindingEngine.FindPath(bot.GridPosition, egg.GridPosition);

            if (path != null && path.Count > 0)
            {
                bot.SetPath(path);
                return;
            }
        }
    }

    private Vector2Int GetRandomWalkablePosition()
    {
        int x, y;
        int maxTries = 100; 
        do
        {
            x = Random.Range(0, gridManager.CurrentWidth);
            y = Random.Range(0, gridManager.CurrentHeight);
            maxTries--;
        }
        while (gridManager.GridData[x, y] == 1 && maxTries > 0);

        return new Vector2Int(x, y);
    }

    private void OnDrawGizmos()
    {
        if (!isServerReady) return;

        Gizmos.color = Color.green;
        if (eggs != null)
        {
            foreach (var egg in eggs)
            {
                Gizmos.DrawSphere(new Vector3(egg.GridPosition.x, 0.5f, egg.GridPosition.y), 0.4f);
            }
        }

        Gizmos.color = Color.black;
        if (bots != null)
        {
            foreach (var bot in bots)
            {
                Gizmos.DrawSphere(new Vector3(bot.Position.x, 0.5f, bot.Position.y), 0.3f);
            }
        }
    }
}