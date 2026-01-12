using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Core.Events;
using Game.Core.Interfaces;

namespace Game.Gameplay.Systems
{
    public interface IScoreTracker
    {
        int GetScore(int entityId);
        void AddScore(int entityId, int points);
        void ResetScore(int entityId);
        void ResetAllScores();
        IReadOnlyDictionary<int, int> GetAllScores();
        (int entityId, int score)[] GetRankedScores();
    }

    public class ScoreSystem : IScoreTracker
    {
        private readonly Dictionary<int, int> _scores;

        public ScoreSystem()
        {
            _scores = new Dictionary<int, int>();
            EventBus.Subscribe<CollectibleCollectedEvent>(OnCollectibleCollected);
        }

        ~ScoreSystem()
        {
            EventBus.Unsubscribe<CollectibleCollectedEvent>(OnCollectibleCollected);
        }

        public int GetScore(int entityId)
        {
            return _scores.TryGetValue(entityId, out int score) ? score : 0;
        }

        public void AddScore(int entityId, int points)
        {
            if (!_scores.ContainsKey(entityId))
            {
                _scores[entityId] = 0;
            }

            _scores[entityId] += points;
            
            EventBus.Publish(new ScoreChangedEvent
            {
                EntityID = entityId,
                NewScore = _scores[entityId],
                PointsAdded = points
            });
        }

        public void ResetScore(int entityId)
        {
            if (_scores.ContainsKey(entityId))
            {
                _scores[entityId] = 0;
            }
        }

        public void ResetAllScores()
        {
            _scores.Clear();
        }

        public IReadOnlyDictionary<int, int> GetAllScores()
        {
            return _scores;
        }

        public (int entityId, int score)[] GetRankedScores()
        {
            return _scores
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToArray();
        }

        private void OnCollectibleCollected(CollectibleCollectedEvent evt)
        {
            AddScore(evt.CollectorID, 1);
        }
    }
}
