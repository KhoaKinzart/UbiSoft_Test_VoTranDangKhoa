using System.Collections.Generic;
using UnityEngine;
using Game.Core.Interfaces;
using Game.Core.Constants;

namespace Game.Gameplay.Entities
{
    public abstract class CharacterEntity : BaseEntity, IMovableEntity, IStaminaEntity
    {
        public float Speed { get; protected set; }
        public float CurrentStamina { get; private set; }
        public float MaxStamina { get; private set; }
        public float StaminaRegenRate { get; private set; }

        public bool IsSprinting { get; set; }
        public bool IsDashing { get; set; }

        protected CharacterEntity(int id, Vector2Int startPosition) : base(id, startPosition)
        {
            CurrentStamina = GameplayConstants.Stamina.MAX_STAMINA;
            MaxStamina = GameplayConstants.Stamina.MAX_STAMINA;
            StaminaRegenRate = GameplayConstants.Stamina.REGEN_RATE;
        }

        public virtual void Move(Vector2 direction, float deltaTime)
        {
            Position += direction * Speed * deltaTime;
            UpdateGridPosition();
        }

        public void RegenerateStamina(float deltaTime)
        {
            CurrentStamina += StaminaRegenRate * deltaTime;
            if (CurrentStamina > MaxStamina)
                CurrentStamina = MaxStamina;
        }

        public void ConsumeStamina(float amount)
        {
            CurrentStamina -= amount;
            if (CurrentStamina < 0)
                CurrentStamina = 0;
        }
    }
}
