using System.Collections.Generic;
using UnityEngine;
using Server.Strategies; // Bắt buộc phải có dòng này để dùng DefaultBotMovement

public class BotEntity
{
    // --- DATA CƠ BẢN ---
    public int ID { get; private set; }
    public Vector2 Position { get; set; }
    public Vector2Int GridPosition { get; set; }
    public List<Vector2Int> CurrentPath { get; private set; } = new List<Vector2Int>();

    // --- THUỘC TÍNH SỬA LỖI "IsMoving" ---
    // Kiểm tra xem list path có phần tử nào không
    public bool IsMoving => CurrentPath != null && CurrentPath.Count > 0;

    // --- STAMINA DATA (Public để Strategy truy cập) ---
    public float CurrentStamina { get; private set; } = 100f;
    public float MaxStamina { get; private set; } = 100f;
    public float StaminaRegenRate { get; private set; } = 10f;

    // --- STATE DATA (Public để Strategy truy cập) ---
    public bool IsSprinting { get; set; }
    public bool IsDashing { get; set; }
    public float DashCooldownTimer { get; set; }
    public float DashDurationTimer { get; set; }

    // --- STRATEGY PATTERN ---
    private IBotMovement _movementStrategy;

    public BotEntity(int id, Vector2Int startPos)
    {
        this.ID = id;
        this.GridPosition = startPos;
        this.Position = startPos;

        // Khởi tạo chiến thuật di chuyển mặc định
        this._movementStrategy = new DefaultBotMovement();
    }

    public void SetPath(List<Vector2Int> path)
    {
        this.CurrentPath = path;
    }

    // --- HÀM CHÍNH (Thay thế cho Move và UpdateLogic cũ) ---
    // ServerManager sẽ gọi hàm này mỗi frame
    public void Tick(float baseSpeed, float deltaTime)
    {
        _movementStrategy.UpdateMovement(this, baseSpeed, deltaTime);
    }

    // --- HELPER METHODS ---
    public void RegenerateStamina(float deltaTime)
    {
        CurrentStamina += StaminaRegenRate * deltaTime;
        if (CurrentStamina > MaxStamina) CurrentStamina = MaxStamina;
    }

    public void ConsumeStamina(float amount)
    {
        CurrentStamina -= amount;
        if (CurrentStamina < 0) CurrentStamina = 0;
    }

    public Vector2Int? GetFinalDestination()
    {
        if (CurrentPath != null && CurrentPath.Count > 0)
        {
            return CurrentPath[CurrentPath.Count - 1];
        }
        return null;
    }
}