using System.Collections.Generic;
using UnityEngine; 

public class BotEntity
{
 
    public int ID { get; private set; }
    public Vector2 Position { get; set; }        
    public Vector2Int GridPosition { get; set; } 

  
    private List<Vector2Int> currentPath;
    public bool IsMoving => currentPath != null && currentPath.Count > 0;


    public float CurrentStamina { get; private set; } = 100f;
    public float MaxStamina { get; private set; } = 100f;
    public float StaminaRegenRate { get; private set; } = 10f; 
    public float SprintCostPerSec { get; private set; } = 25f;  
    public float DashCost { get; private set; } = 30f;         


    private float minStaminaToStartSprint = 15f;

  
    public bool IsSprinting { get; private set; }
    public bool IsDashing { get; private set; } 

 
    private float dashCooldownTimer = 0f;
    private float dashDurationTimer = 0f;
    private const float DASH_COOLDOWN = 3.0f;
    private const float DASH_DURATION = 0.4f; 


    public BotEntity(int id, Vector2Int startPos)
    {
        this.ID = id;
        this.GridPosition = startPos;
        this.Position = startPos;
        this.currentPath = new List<Vector2Int>();
    }

    public void SetPath(List<Vector2Int> path)
    {
        this.currentPath = path;
    }


    public void UpdateLogic(float deltaTime)
    {

        if (dashCooldownTimer > 0) dashCooldownTimer -= deltaTime;

  
        if (!IsSprinting && !IsDashing)
        {
            if (CurrentStamina < MaxStamina)
            {
                CurrentStamina += StaminaRegenRate * deltaTime;
                if (CurrentStamina > MaxStamina) CurrentStamina = MaxStamina;
            }
        }
    }


    public void Move(float baseSpeed, float deltaTime)
    {
  
        if (currentPath == null || currentPath.Count == 0)
        {
            IsSprinting = false;
            IsDashing = false;
            return;
        }

        Vector2 targetPos = currentPath[0];
        float distToNextNode = Vector2.Distance(Position, targetPos);

     
        float distToFinalDest = Vector2.Distance(Position, currentPath[currentPath.Count - 1]);

        if (IsDashing)
        {
           
            dashDurationTimer -= deltaTime;
            if (dashDurationTimer <= 0)
            {
                IsDashing = false;
            }
        }
        else
        {
   
            if (dashCooldownTimer <= 0 &&
                CurrentStamina >= DashCost &&
                distToFinalDest > 4.0f)
            {
                PerformDash();
            }
        }


        float currentSpeed = baseSpeed;
        IsSprinting = false; 

        if (IsDashing)
        {
       
            currentSpeed = baseSpeed * 4.0f;

        }
        else
        {

            bool wantToSprint = distToFinalDest > 6.0f;
            bool canSprint = CurrentStamina > (IsSprinting ? 0f : minStaminaToStartSprint);

            if (wantToSprint && canSprint)
            {
                IsSprinting = true;
                currentSpeed = baseSpeed * 1.8f; 

              
                CurrentStamina -= SprintCostPerSec * deltaTime;
                if (CurrentStamina < 0) CurrentStamina = 0;
            }
        }

     
        Vector2 direction = (targetPos - Position).normalized;

       
        Position += direction * currentSpeed * deltaTime;

   
        if (distToNextNode < 0.1f) 
        {
            Position = targetPos;
            GridPosition = new Vector2Int((int)Position.x, (int)Position.y);

            currentPath.RemoveAt(0);
        }
    }

    private void PerformDash()
    {
        IsDashing = true;
        dashDurationTimer = DASH_DURATION;
        dashCooldownTimer = DASH_COOLDOWN;
        CurrentStamina -= DashCost;
    }

    public Vector2Int? GetFinalDestination()

    {

        if (currentPath != null && currentPath.Count > 0)

        {

            return currentPath[currentPath.Count - 1]; 

        }

        return null;

    }
}