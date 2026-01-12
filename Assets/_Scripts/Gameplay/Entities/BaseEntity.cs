using UnityEngine;
using Game.Core.Interfaces;

namespace Game.Gameplay.Entities
{
    public abstract class BaseEntity : IEntity
    {
        public int ID { get; protected set; }
        public Vector2 Position { get; set; }
        public Vector2Int GridPosition { get; set; }

        protected BaseEntity(int id, Vector2Int startPosition)
        {
            ID = id;
            GridPosition = startPosition;
            Position = startPosition;
        }

        public virtual void UpdateGridPosition()
        {
            GridPosition = new Vector2Int(
                Mathf.RoundToInt(Position.x),
                Mathf.RoundToInt(Position.y)
            );
        }
    }
}
