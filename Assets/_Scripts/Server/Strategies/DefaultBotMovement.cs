// File: Assets/_Scripts/Server/Strategies/DefaultBotMovement.cs
using UnityEngine;

namespace Server.Strategies
{
    public class DefaultBotMovement : IBotMovement
    {
        // Các hằng số logic
        private const float MIN_STAMINA_SPRINT = 15f;
        private const float DASH_COST = 30f;
        private const float SPRINT_COST = 25f;
        private const float DASH_DURATION = 0.4f;
        private const float DASH_COOLDOWN = 3.0f;

        public void UpdateMovement(BotEntity bot, float baseSpeed, float deltaTime)
        {
            // 1. Xử lý Timer
            if (bot.DashCooldownTimer > 0) bot.DashCooldownTimer -= deltaTime;

            // 2. Hồi phục Stamina
            if (!bot.IsSprinting && !bot.IsDashing)
            {
                bot.RegenerateStamina(deltaTime);
            }

            // 3. Kiểm tra đường đi
            if (bot.CurrentPath == null || bot.CurrentPath.Count == 0)
            {
                bot.IsSprinting = false;
                bot.IsDashing = false;
                return;
            }

            Vector2 targetPos = bot.CurrentPath[0];
            float distToFinalDest = Vector2.Distance(bot.Position, bot.CurrentPath[bot.CurrentPath.Count - 1]);
            float distToNextNode = Vector2.Distance(bot.Position, targetPos);

            // 4. Logic kích hoạt Dash
            if (bot.IsDashing)
            {
                bot.DashDurationTimer -= deltaTime;
                if (bot.DashDurationTimer <= 0) bot.IsDashing = false;
            }
            else if (bot.DashCooldownTimer <= 0 && bot.CurrentStamina >= DASH_COST && distToFinalDest > 4.0f)
            {
                StartDash(bot);
            }

            // 5. Tính toán tốc độ (Sprint/Normal)
            float currentSpeed = baseSpeed;
            bot.IsSprinting = false;

            if (bot.IsDashing)
            {
                currentSpeed = baseSpeed * 2.0f;
            }
            else
            {
                bool wantToSprint = distToFinalDest > 6.0f;
                bool canSprint = bot.CurrentStamina > (bot.IsSprinting ? 0f : MIN_STAMINA_SPRINT);

                if (wantToSprint && canSprint)
                {
                    bot.IsSprinting = true;
                    currentSpeed = baseSpeed * 1.3f;
                    bot.ConsumeStamina(SPRINT_COST * deltaTime);
                }
            }

            // 6. Cập nhật vị trí
            Vector2 direction = (targetPos - bot.Position).normalized;
            bot.Position += direction * currentSpeed * deltaTime;

            // 7. Kiểm tra đến waypoint tiếp theo
            if (distToNextNode < 0.1f)
            {
                bot.Position = targetPos;
                bot.GridPosition = new Vector2Int((int)bot.Position.x, (int)bot.Position.y);
                bot.CurrentPath.RemoveAt(0);
            }
        }

        private void StartDash(BotEntity bot)
        {
            bot.IsDashing = true;
            bot.DashDurationTimer = DASH_DURATION;
            bot.DashCooldownTimer = DASH_COOLDOWN;
            bot.ConsumeStamina(DASH_COST);
        }
    }
}