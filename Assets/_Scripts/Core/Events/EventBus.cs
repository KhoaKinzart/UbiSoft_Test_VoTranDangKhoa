using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Events
{
    public class EventBus
    {
        private static EventBus _instance;
        public static EventBus Instance => _instance ?? (_instance = new EventBus());

        private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> callback) where T : IGameEvent
        {
            Instance.SubscribeInternal(callback);
        }

        public static void Unsubscribe<T>(Action<T> callback) where T : IGameEvent
        {
            Instance.UnsubscribeInternal(callback);
        }

        public static void Publish<T>(T eventData) where T : IGameEvent
        {
            Instance.PublishInternal(eventData);
        }

        public static void Clear()
        {
            Instance._subscribers.Clear();
        }

        private void SubscribeInternal<T>(Action<T> callback) where T : IGameEvent
        {
            var eventType = typeof(T);
            
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<Delegate>();
            }

            _subscribers[eventType].Add(callback);
        }

        private void UnsubscribeInternal<T>(Action<T> callback) where T : IGameEvent
        {
            var eventType = typeof(T);
            
            if (_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType].Remove(callback);
            }
        }

        private void PublishInternal<T>(T eventData) where T : IGameEvent
        {
            var eventType = typeof(T);
            
            if (_subscribers.ContainsKey(eventType))
            {
                foreach (var subscriber in _subscribers[eventType])
                {
                    try
                    {
                        (subscriber as Action<T>)?.Invoke(eventData);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
