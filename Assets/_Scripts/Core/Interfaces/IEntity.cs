using UnityEngine;

namespace Game.Core.Interfaces
{
    public interface IEntity
    {
        int ID { get; }
        Vector2 Position { get; set; }
        Vector2Int GridPosition { get; set; }
    }

    public interface IMovableEntity : IEntity
    {
        float Speed { get; }
        void Move(Vector2 direction, float deltaTime);
    }

    public interface IStaminaEntity : IEntity
    {
        float CurrentStamina { get; }
        float MaxStamina { get; }
        void RegenerateStamina(float deltaTime);
        void ConsumeStamina(float amount);
    }

    public interface ICollectible : IEntity
    {
        void OnCollected(IEntity collector);
    }
}
