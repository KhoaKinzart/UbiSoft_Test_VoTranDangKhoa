using UnityEngine;
using Game.Core.Constants;

namespace Game.Gameplay.Entities
{
    public class PlayerEntity : CharacterEntity
    {
        public Vector2 LastInput { get; set; }
        public bool WantToSprint { get; set; }
        public bool WantToDash { get; set; }
        
        public float DashCooldownTimer { get; set; }
        public float DashDurationTimer { get; set; }

        private const float DASH_DURATION = 0.3f;
        private const float DASH_COOLDOWN = 2.0f;

        public PlayerEntity(int id, Vector2Int startPosition) : base(id, startPosition)
        {
            Speed = 5f;
        }

        public void Tick(float baseSpeed, float deltaTime, int[,] gridData, int gridWidth, int gridHeight)
        {
            UpdateTimers(deltaTime);

            if (!IsSprinting && !IsDashing)
            {
                RegenerateStamina(deltaTime);
            }

            if (LastInput.magnitude < 0.1f)
            {
                IsSprinting = false;
                
                if (WantToDash)
                {
                    WantToDash = false;
                }
                
                return;
            }

            HandleDash(deltaTime);
            float currentSpeed = CalculateCurrentSpeed(baseSpeed, deltaTime);
            Vector2 movement = LastInput * currentSpeed * deltaTime;
            Vector2 newPosition = Position + movement;

            if (CanMoveTo(newPosition, gridData, gridWidth, gridHeight))
            {
                Position = newPosition;
            }
            else
            {
                Vector2 slideX = Position + new Vector2(movement.x, 0);
                if (CanMoveTo(slideX, gridData, gridWidth, gridHeight))
                {
                    Position = slideX;
                }
                else
                {
                    Vector2 slideY = Position + new Vector2(0, movement.y);
                    if (CanMoveTo(slideY, gridData, gridWidth, gridHeight))
                    {
                        Position = slideY;
                    }
                }
            }

            GridPosition = new Vector2Int(
                Mathf.RoundToInt(Position.x),
                Mathf.RoundToInt(Position.y)
            );
        }

        private void UpdateTimers(float deltaTime)
        {
            if (DashCooldownTimer > 0)
                DashCooldownTimer -= deltaTime;

            if (IsDashing)
            {
                DashDurationTimer -= deltaTime;
                if (DashDurationTimer <= 0)
                    IsDashing = false;
            }
        }

        private void HandleDash(float deltaTime)
        {
            if (!WantToDash)
                return;
        
            if (CanDash())
            {
                StartDash();
                WantToDash = false;
            }
            else
            {
                WantToDash = false;
            }
        }

        private bool CanDash()
        {
            return !IsDashing &&
                   DashCooldownTimer <= 0 &&
                   CurrentStamina >= GameplayConstants.Stamina.DASH_COST &&
                   LastInput.magnitude > 0.1f;
        }

        private void StartDash()
        {
            IsDashing = true;
            DashDurationTimer = DASH_DURATION;
            DashCooldownTimer = DASH_COOLDOWN;
            ConsumeStamina(GameplayConstants.Stamina.DASH_COST);
        }

        private float CalculateCurrentSpeed(float baseSpeed, float deltaTime)
        {
            if (IsDashing)
            {
                return baseSpeed * GameplayConstants.Movement.DASH_MULTIPLIER;
            }

            bool canSprint = CurrentStamina > (IsSprinting ? 0f : GameplayConstants.Stamina.MIN_SPRINT_THRESHOLD);

            if (WantToSprint && canSprint)
            {
                IsSprinting = true;
                ConsumeStamina(GameplayConstants.Stamina.SPRINT_COST * deltaTime);
                return baseSpeed * GameplayConstants.Movement.SPRINT_MULTIPLIER;
            }

            IsSprinting = false;
            return baseSpeed;
        }

        private bool CanMoveTo(Vector2 targetPos, int[,] gridData, int gridWidth, int gridHeight)
        {
            const float PLAYER_RADIUS = 0.3f;

            if (!IsPositionWalkable(targetPos, gridData, gridWidth, gridHeight))
                return false;

            Vector2[] checkPoints = new Vector2[]
            {
        targetPos + new Vector2(PLAYER_RADIUS, 0),
        targetPos + new Vector2(-PLAYER_RADIUS, 0),
        targetPos + new Vector2(0, PLAYER_RADIUS),
        targetPos + new Vector2(0, -PLAYER_RADIUS)
            };

            foreach (Vector2 point in checkPoints)
            {
                if (!IsPositionWalkable(point, gridData, gridWidth, gridHeight))
                    return false;
            }

            return true;
        }


        private bool IsPositionWalkable(Vector2 pos, int[,] gridData, int gridWidth, int gridHeight)
        {
            int gridX = Mathf.FloorToInt(pos.x + 0.5f);
            int gridY = Mathf.FloorToInt(pos.y + 0.5f);

            if (gridX < 0 || gridX >= gridWidth || gridY < 0 || gridY >= gridHeight)
                return false;

            return gridData[gridX, gridY] == 0;
        }
    }
}
