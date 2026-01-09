using UnityEngine;

public class PlayerEntity
{
    // --- DATA ---
    public int ID { get; private set; }
    public Vector2 Position { get; set; }
    public Vector2Int GridPosition { get; set; }

    // --- STAMINA & STATE ---
    public float CurrentStamina { get; private set; } = 100f;
    public float MaxStamina { get; private set; } = 100f;

    // Các thông số (nên khớp với Bot để công bằng)
    private const float STAMINA_REGEN = 10f;
    private const float SPRINT_COST = 25f;
    private const float DASH_COST = 30f;
    private const float DASH_DURATION = 0.2f; // Lướt nhanh hơn chút
    private const float DASH_COOLDOWN = 2.0f;

    // Trạng thái
    public bool IsDashing { get; private set; }
    private float dashTimer;
    private float dashCooldownTimer;

    public PlayerEntity(int id, Vector2Int startPos)
    {
        this.ID = id;
        this.Position = startPos;
        this.GridPosition = startPos;
    }

    // Hàm xử lý Input nhận từ Client
    public void ProcessInput(PlayerInputPacket input, float baseSpeed, float deltaTime)
    {
        // 1. Hồi Cooldown & Stamina
        if (dashCooldownTimer > 0) dashCooldownTimer -= deltaTime;

        if (!input.IsSprint && !IsDashing)
        {
            CurrentStamina += STAMINA_REGEN * deltaTime;
            if (CurrentStamina > MaxStamina) CurrentStamina = MaxStamina;
        }

        // 2. Xử lý Dash (Lướt)
        if (IsDashing)
        {
            dashTimer -= deltaTime;
            if (dashTimer <= 0) IsDashing = false;
        }
        else if (input.IsDash && dashCooldownTimer <= 0 && CurrentStamina >= DASH_COST)
        {
            StartDash();
        }

        // 3. Tính toán di chuyển
        Vector2 moveDir = input.MovementInput.normalized;
        float currentSpeed = baseSpeed;

        if (IsDashing)
        {
            currentSpeed = baseSpeed * 3.0f; // Lướt nhanh gấp 3
        }
        else if (input.IsSprint && CurrentStamina > 0 && moveDir.magnitude > 0)
        {
            currentSpeed = baseSpeed * 1.5f; // Chạy nhanh gấp 1.5
            CurrentStamina -= SPRINT_COST * deltaTime;
            if (CurrentStamina < 0) CurrentStamina = 0;
        }

        // 4. Áp dụng vị trí mới
        if (moveDir.sqrMagnitude > 0.01f || IsDashing)
        {
            Position += moveDir * currentSpeed * deltaTime;
            GridPosition = new Vector2Int(Mathf.RoundToInt(Position.x), Mathf.RoundToInt(Position.y));
        }
    }

    private void StartDash()
    {
        IsDashing = true;
        dashTimer = DASH_DURATION;
        dashCooldownTimer = DASH_COOLDOWN;
        CurrentStamina -= DASH_COST;
    }
}