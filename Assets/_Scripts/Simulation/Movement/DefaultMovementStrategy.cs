using UnityEngine;
using Game.Gameplay.Entities;
using Game.Core.Constants;

namespace Game.Simulation.Movement
{
    public class DefaultMovementStrategy : IMovementStrategy
    {
        private const float DASH_DURATION = 0.3f;
        private const float DASH_COOLDOWN = 5.0f;
        private const float SPRINT_DISTANCE_THRESHOLD = 8.0f;
        private const float DASH_DISTANCE_THRESHOLD = 6.0f;
        private const float WAYPOINT_REACH_THRESHOLD = 0.2f;

        public void Execute(AgentEntity agent, float deltaTime)
        {
            UpdateTimers(agent, deltaTime);
            RegenerateStaminaIfNeeded(agent, deltaTime);

            if (!agent.IsMoving)
            {
                ResetMovementState(agent);
                return;
            }

            Vector2 targetPos = agent.CurrentPath[0];
            float distToFinalDest = Vector2.Distance(agent.Position, agent.CurrentPath[agent.CurrentPath.Count - 1]);
            float distToNextNode = Vector2.Distance(agent.Position, targetPos);

            HandleDash(agent, distToFinalDest, deltaTime);
            float currentSpeed = CalculateSpeed(agent, distToFinalDest, deltaTime);
            MoveTowardsTarget(agent, targetPos, currentSpeed, deltaTime);
            CheckWaypointReached(agent, targetPos, distToNextNode);
        }

        private void UpdateTimers(AgentEntity agent, float deltaTime)
        {
            if (agent.DashCooldownTimer > 0)
                agent.DashCooldownTimer -= deltaTime;
        }

        private void RegenerateStaminaIfNeeded(AgentEntity agent, float deltaTime)
        {
            if (!agent.IsSprinting && !agent.IsDashing)
            {
                agent.RegenerateStamina(deltaTime);
            }
        }

        private void ResetMovementState(AgentEntity agent)
        {
            agent.IsSprinting = false;
            agent.IsDashing = false;
        }

        private void HandleDash(AgentEntity agent, float distToFinalDest, float deltaTime)
        {
            if (agent.IsDashing)
            {
                agent.DashDurationTimer -= deltaTime;
                if (agent.DashDurationTimer <= 0)
                    agent.IsDashing = false;
            }
            else if (ShouldDash(agent, distToFinalDest))
            {
                StartDash(agent);
            }
        }

        private bool ShouldDash(AgentEntity agent, float distToFinalDest)
        {
            return agent.DashCooldownTimer <= 0 &&
                   agent.CurrentStamina >= GameplayConstants.Stamina.DASH_COST &&
                   distToFinalDest > DASH_DISTANCE_THRESHOLD;
        }

        private void StartDash(AgentEntity agent)
        {
            agent.IsDashing = true;
            agent.DashDurationTimer = DASH_DURATION;
            agent.DashCooldownTimer = DASH_COOLDOWN;
            agent.ConsumeStamina(GameplayConstants.Stamina.DASH_COST);
        }

        private float CalculateSpeed(AgentEntity agent, float distToFinalDest, float deltaTime)
        {
            float baseSpeed = agent.Speed;

            if (agent.IsDashing)
            {
                return baseSpeed * 1.6f;
            }

            bool wantToSprint = distToFinalDest > SPRINT_DISTANCE_THRESHOLD;
            bool canSprint = agent.CurrentStamina > (agent.IsSprinting ? 0f : GameplayConstants.Stamina.MIN_SPRINT_THRESHOLD);

            if (wantToSprint && canSprint)
            {
                agent.IsSprinting = true;
                agent.ConsumeStamina(GameplayConstants.Stamina.SPRINT_COST * deltaTime);
                return baseSpeed * 1.15f;
            }

            agent.IsSprinting = false;
            return baseSpeed;
        }

        private void MoveTowardsTarget(AgentEntity agent, Vector2 targetPos, float speed, float deltaTime)
        {
            Vector2 direction = (targetPos - agent.Position).normalized;
            float moveDistance = speed * deltaTime;
            float distToTarget = Vector2.Distance(agent.Position, targetPos);

            if (moveDistance > distToTarget)
            {
                agent.Position = targetPos;
            }
            else
            {
                agent.Position += direction * moveDistance;
            }
        }

        private void CheckWaypointReached(AgentEntity agent, Vector2 targetPos, float distance)
        {
            if (distance < WAYPOINT_REACH_THRESHOLD)
            {
                agent.Position = targetPos;
                agent.UpdateGridPosition();
                agent.CurrentPath.RemoveAt(0);
            }
        }
    }
}
