using UnityEngine;

namespace Game.Core.Events
{
    public struct CollectibleCollectedEvent : IGameEvent
    {
        public int CollectibleID;
        public int CollectorID;
        public Vector2Int Position;
        public bool IsPlayer;
    }

    public struct CollectibleSpawnedEvent : IGameEvent
    {
        public int CollectibleID;
        public Vector2Int Position;
    }

    public struct PlayerRegisteredEvent : IGameEvent
    {
        public int PlayerID;
        public Vector2Int StartPosition;
    }

    public struct AgentSpawnedEvent : IGameEvent
    {
        public int AgentID;
        public Vector2Int StartPosition;
    }

    public struct StaminaChangedEvent : IGameEvent
    {
        public int EntityID;
        public float CurrentStamina;
        public float MaxStamina;
        public bool IsSprinting;
    }

    public struct ScoreChangedEvent : IGameEvent
    {
        public int EntityID;
        public int NewScore;
        public int PointsAdded;
    }

    public struct GameTimerStartedEvent : IGameEvent
    {
        public float Duration;
    }

    public struct GameTimerFinishedEvent : IGameEvent
    {
        public float TotalTime;
    }

    public struct GameOverEvent : IGameEvent
    {
        public int WinnerID;
        public int WinnerScore;
        public bool IsPlayer;
    }
}
