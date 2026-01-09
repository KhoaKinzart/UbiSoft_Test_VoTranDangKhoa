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
    [Tooltip("Số lần update mỗi giây (Hz).")]
    [SerializeField] private int tickRate = 20;

    [Header("References")]
    [SerializeField] private GridManager gridManager;

    // --- PLAYER VARIABLES ---
    private PlayerEntity localPlayer;
    private PlayerInputPacket currentInput;

    private List<BotEntity> bots = new List<BotEntity>();
    private List<EggEntity> eggs = new List<EggEntity>();

    public IReadOnlyList<EggEntity> Eggs => eggs;

    private AStar pathfindingEngine;
    private bool isServerReady = false;

    // API nhận Input từ Client
    public void ReceivePlayerInput(PlayerInputPacket input)
    {
        currentInput = input;
    }

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
        
        // [SỬA QUAN TRỌNG 1] Gọi hàm spawn có cả Player
        SpawnBotsAndPlayer();

        isServerReady = true;
        Debug.Log("Server Simulation Started!");
    }

    void Update()
    {
        if (!isServerReady) return;
        float dt = Time.deltaTime;

        // [SỬA QUAN TRỌNG 2] Thêm đoạn logic này để Player di chuyển
        if (localPlayer != null)
        {
            localPlayer.ProcessInput(currentInput, botSpeed, dt);
            CheckEggCollection(localPlayer.GridPosition, isPlayer: true);
        }

        // Logic cho Bots
        foreach (var bot in bots)
        {
            bot.Tick(botSpeed, dt);
            
            // Check bot ăn trứng
            if (CheckEggCollection(bot.GridPosition, isPlayer: false))
            {
                bot.SetPath(null);
            }

            if (!bot.IsMoving)
            {
                AssignNearestEggAsTarget(bot);
            }
        }

        // Logic sinh trứng ngẫu nhiên
        if (eggs.Count < eggCount / 2)
        {
            Vector2Int pos = GetRandomWalkablePosition();
            eggs.Add(new EggEntity(Random.Range(10000, 99999), pos));
        }
    }

    // [SỬA QUAN TRỌNG 3] Gửi cả vị trí Player về cho Client vẽ
    public List<BotSnapshot> GetLatestSnapshots()
    {
        List<BotSnapshot> snapshots = new List<BotSnapshot>();
        float serverTime = Time.time;

        // Thêm Player vào gói tin (ID = 0)
        if (localPlayer != null)
        {
            snapshots.Add(new BotSnapshot
            {
                BotID = localPlayer.ID,
                Position = localPlayer.Position,
                Stamina = localPlayer.CurrentStamina,
                Timestamp = serverTime
            });
        }

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
            eggs.Add(new EggEntity(i + 1000, pos));
        }
    }

    // [SỬA QUAN TRỌNG 4] Thay hàm SpawnBots cũ bằng hàm này
    private void SpawnBotsAndPlayer()
    {
        // 1. Tạo Player (ID = 0)
        localPlayer = new PlayerEntity(0, GetRandomWalkablePosition());

        // 2. Tạo Bots (ID chạy từ 1)
        for (int i = 1; i <= botCount; i++)
        {
            BotEntity newBot = new BotEntity(i, GetRandomWalkablePosition());
            AssignNearestEggAsTarget(newBot);
            bots.Add(newBot);
        }
    }

    private bool CheckEggCollection(Vector2Int unitGridPos, bool isPlayer)
    {
        for (int i = eggs.Count - 1; i >= 0; i--)
        {
            if (eggs[i].GridPosition == unitGridPos)
            {
                Vector2Int eatenEggPos = eggs[i].GridPosition;
                eggs.RemoveAt(i);

                foreach (var otherBot in bots)
                {
                    Vector2Int? dest = otherBot.GetFinalDestination();
                    if (dest.HasValue && dest.Value == eatenEggPos)
                    {
                        otherBot.SetPath(null);
                    }
                }
                
                if (isPlayer) Debug.Log("Player ate egg!");
                return true;
            }
        }
        return false;
    }

    // Các hàm giữ nguyên
    private void AssignNearestEggAsTarget(BotEntity bot)
    {
        if (eggs.Count == 0) return;
        var sortedEggs = eggs.OrderBy(e => Mathf.Abs(bot.GridPosition.x - e.GridPosition.x) + Mathf.Abs(bot.GridPosition.y - e.GridPosition.y)).ToList();
        foreach (var egg in sortedEggs)
        {
            List<Vector2Int> path = pathfindingEngine.FindPath(bot.GridPosition, egg.GridPosition);
            if (path != null && path.Count > 0) { bot.SetPath(path); return; }
        }
    }

    private Vector2Int GetRandomWalkablePosition()
    {
        int x, y; int maxTries = 100;
        do { x = Random.Range(0, gridManager.CurrentWidth); y = Random.Range(0, gridManager.CurrentHeight); maxTries--; } 
        while (gridManager.GridData[x, y] == 1 && maxTries > 0);
        return new Vector2Int(x, y);
    }

    private void OnDrawGizmos()
    {
        if (!isServerReady) return;
        Gizmos.color = Color.green;
        foreach (var egg in eggs) Gizmos.DrawSphere(new Vector3(egg.GridPosition.x, 0.5f, egg.GridPosition.y), 0.4f);
        Gizmos.color = Color.black;
        foreach (var bot in bots) Gizmos.DrawSphere(new Vector3(bot.Position.x, 0.5f, bot.Position.y), 0.3f);
        
        // Vẽ Gizmos cho Player (Màu Đỏ) để dễ debug
        if (localPlayer != null) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(new Vector3(localPlayer.Position.x, 0.5f, localPlayer.Position.y), 0.4f);
        }
    }
}