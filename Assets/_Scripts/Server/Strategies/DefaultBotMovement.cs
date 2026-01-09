
using UnityEngine;

namespace Server.Strategies
{
    public class DefaultBotMovement : IBotMovement
    {

        private const float MIN_STAMINA_SPRINT = 15f;
        private const float DASH_COST = 30f;
        private const float SPRINT_COST = 25f;
        private const float DASH_DURATION = 0.4f;
        private const float DASH_COOLDOWN = 3.0f;

        public void UpdateMovement(BotEntity bot, float baseSpeed, float deltaTime)
        {
       
            if (bot.DashCooldownTimer > 0) bot.DashCooldownTimer -= deltaTime;

        
            if (!bot.IsSprinting && !bot.IsDashing)
            {
                bot.RegenerateStamina(deltaTime);
            }

   
            if (bot.CurrentPath == null || bot.CurrentPath.Count == 0)
            {
                bot.IsSprinting = false;
                bot.IsDashing = false;
                return;
            }

            Vector2 targetPos = bot.CurrentPath[0];
            float distToFinalDest = Vector2.Distance(bot.Position, bot.CurrentPath[bot.CurrentPath.Count - 1]);
            float distToNextNode = Vector2.Distance(bot.Position, targetPos);

         
            if (bot.IsDashing)
            {
                bot.DashDurationTimer -= deltaTime;
                if (bot.DashDurationTimer <= 0) bot.IsDashing = false;
            }
            else if (bot.DashCooldownTimer <= 0 && bot.CurrentStamina >= DASH_COST && distToFinalDest > 4.0f)
            {
                StartDash(bot);
            }

      
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


            Vector2 direction = (targetPos - bot.Position).normalized;
            bot.Position += direction * currentSpeed * deltaTime;

           
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