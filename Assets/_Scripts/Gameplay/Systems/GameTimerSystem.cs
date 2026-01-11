using System;
using UnityEngine;
using Game.Core.Events;

namespace Game.Gameplay.Systems
{
    public interface IGameTimer
    {
        float TimeRemaining { get; }
        float TotalTime { get; }
        bool IsRunning { get; }
        bool IsFinished { get; }
        
        void StartTimer(float duration);
        void StopTimer();
        void ResetTimer();
        void Update(float deltaTime);
    }

    public class GameTimerSystem : IGameTimer
    {
        public float TimeRemaining { get; private set; }
        public float TotalTime { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsFinished { get; private set; }

        public void StartTimer(float duration)
        {
            TotalTime = duration;
            TimeRemaining = duration;
            IsRunning = true;
            IsFinished = false;
            
            EventBus.Publish(new GameTimerStartedEvent
            {
                Duration = duration
            });
        }

        public void StopTimer()
        {
            IsRunning = false;
        }

        public void ResetTimer()
        {
            TimeRemaining = TotalTime;
            IsRunning = false;
            IsFinished = false;
        }

        public void Update(float deltaTime)
        {
            if (!IsRunning || IsFinished)
                return;

            TimeRemaining -= deltaTime;

            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                IsRunning = false;
                IsFinished = true;
                
                EventBus.Publish(new GameTimerFinishedEvent
                {
                    TotalTime = TotalTime
                });
            }
        }
    }
}
