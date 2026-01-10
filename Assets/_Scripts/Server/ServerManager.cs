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

    private List<BotEntity> bots = new List<BotEntity>();
    private List<EggEntity> eggs = new List<EggEntity>();

    public IReadOnlyList<EggEntity> Eggs => eggs;

    private AStar pathfindingEngine;
    private bool isServerReady = false;

    // --- PLAYER VARIABLES (Đã cập nhật đầy đủ) ---
    private Vector2 playerPosition;
    private PlayerInputPacket currentInput; // Lưu input để xử lý trong Update
    private bool isPlayerActive = false;
    private float playerStamina = 100f; // Thêm stamina cho player nếu cần

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
        SpawnPlayer(); // <--- GỌI HÀM SPAWN PLAYER

        isServerReady = true;
        Debug.Log("Server Simulation Started!");
    }

    private void SpawnPlayer()
    {
        Vector2Int startPos = GetRandomWalkablePosition();
        playerPosition = new Vector2(startPos.x, startPos.y);
        isPlayerActive = true;
        playerStamina = 100f;
    }

    void Update()
    {
        if (!isServerReady) return;

        float dt = Time.deltaTime;

        // 1. XỬ LÝ PLAYER (Thêm logic ăn trứng cho Player)
        if (isPlayerActive)
        {
            UpdatePlayerMovement(dt);
            
            // Check ăn trứng cho Player (Sử dụng hàm overload mới)
            Vector2Int playerGridPos = new Vector2Int(Mathf.RoundToInt(playerPosition.x), Mathf.RoundToInt(playerPosition.y));
            CheckEggCollection(playerGridPos, true); 
        }

        // 2. XỬ LÝ BOTS
        foreach (var bot in bots)
        {
            bot.Tick(botSpeed, dt);

            // Check ăn trứng cho Bot
            CheckEggCollection(bot.GridPosition, false);

            if (!bot.IsMoving)
            {
                AssignNearestEggAsTarget(bot);
            }
        }

        // 3. SINH TRỨNG
        if (eggs.Count < eggCount / 2)
        {
            Vector2Int pos = GetRandomWalkablePosition();
            eggs.Add(new EggEntity(Random.Range(10000, 99999), pos));
        }
    }

    // Xử lý di chuyển trong Update loop để đồng bộ
    private void UpdatePlayerMovement(float dt)
    {
        if (currentInput.MovementInput == Vector2.zero) return;

        // Tính toán vị trí dự kiến
        Vector2 moveStep = currentInput.MovementInput * botSpeed * dt;
        Vector2 potentialPos = playerPosition + moveStep;

        // Check va chạm
        if (IsValidMovePosition(potentialPos))
        {
            playerPosition = potentialPos;
        }
    }

    // Client gọi hàm này để gửi Input (Chỉ lưu lại, không di chuyển ngay)
    public void HandleClientInput(PlayerInputPacket input)
    {
        if (!isServerReady) return;
        currentInput = input;
    }

    public PlayerSnapshot GetPlayerSnapshot()
    {
        return new PlayerSnapshot
        {
            Position = playerPosition,
            Stamina = playerStamina,
            Timestamp = Time.time
        };
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

    // --- HÀM LOGIC GAME ---

    // Hàm check ăn trứng (Đã sửa để dùng chung cho cả Bot và Player)
    private void CheckEggCollection(Vector2Int entityGridPos, bool isPlayer)
    {
        for (int i = eggs.Count - 1; i >= 0; i--)
        {
            if (eggs[i].GridPosition == entityGridPos)
            {
                Vector2Int eatenEggPos = eggs[i].GridPosition;
                eggs.RemoveAt(i);

                if (isPlayer) Debug.Log("Player ATE an EGG!");

                // Thông báo cho các bot khác hủy đường đi nếu đang nhắm đến trứng này
                foreach (var otherBot in bots)
                {
                    // Nếu là bot đang check thì bỏ qua (chỉ áp dụng nếu entity là bot)
                    if (!isPlayer && otherBot.GridPosition == entityGridPos) continue;

                    Vector2Int? dest = otherBot.GetFinalDestination();
                    if (dest.HasValue && dest.Value == eatenEggPos)
                    {
                        otherBot.SetPath(null); // Bot dừng lại tính đường mới
                    }
                }
                break;
            }
        }
    }

    private bool IsValidMovePosition(Vector2 pos)
    {
        int gridX = Mathf.RoundToInt(pos.x);
        int gridY = Mathf.RoundToInt(pos.y);

        if (gridX < 0 || gridX >= gridManager.CurrentWidth || 
            gridY < 0 || gridY >= gridManager.CurrentHeight)
            return false;

        if (gridManager.GridData[gridX, gridY] == 1)
            return false;

        return true;
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
                    if (egg.GridPosition == startPos) { isOverlapping = true; break; }
                }

                if (!isOverlapping)
                {
                    foreach (var b in bots)
                    {
                        if (b.GridPosition == startPos) { isOverlapping = true; break; }
                    }
                }
            } while (isOverlapping && attempts < 50);

            BotEntity newBot = new BotEntity(i, startPos);
            AssignNearestEggAsTarget(newBot);
            bots.Add(newBot);
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
        foreach (var egg in eggs)
        {
            Gizmos.DrawSphere(new Vector3(egg.GridPosition.x, 0.5f, egg.GridPosition.y), 0.4f);
        }

        Gizmos.color = Color.black;
        foreach (var bot in bots)
        {
            Gizmos.DrawSphere(new Vector3(bot.Position.x, 0.5f, bot.Position.y), 0.3f);
        }

        // Vẽ Gizmo cho Player để dễ debug
        if (isPlayerActive)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(new Vector3(playerPosition.x, 0.5f, playerPosition.y), 0.4f);
        }
    }
}