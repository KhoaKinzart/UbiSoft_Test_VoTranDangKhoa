using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Game.Core.Events;
using Game.Core.Constants;

namespace Game.Presentation.UI
{
    public class ScoreboardEntry
    {
        public int EntityID;
        public string Name;
        public int Score;
        public bool IsPlayer;
    }

    public class ScoreboardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scoreboardText;
        [SerializeField] private int maxEntriesToShow = 10;
        [SerializeField] private bool showRealTime = true;

        private readonly Dictionary<int, int> _scores = new Dictionary<int, int>();
        private bool _isDirty = false;

        private void OnEnable()
        {
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        }

        private void Update()
        {
            if (_isDirty && showRealTime)
            {
                UpdateDisplay();
                _isDirty = false;
            }
        }

        private void OnScoreChanged(ScoreChangedEvent evt)
        {
            _scores[evt.EntityID] = evt.NewScore;
            _isDirty = true;
        }

        public void SetScores(Dictionary<int, int> scores)
        {
            _scores.Clear();
            foreach (var kvp in scores)
            {
                _scores[kvp.Key] = kvp.Value;
            }
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (scoreboardText == null) return;

            var entries = _scores
                .Select(kvp => new ScoreboardEntry
                {
                    EntityID = kvp.Key,
                    Name = GetEntityName(kvp.Key),
                    Score = kvp.Value,
                    IsPlayer = kvp.Key == GameplayConstants.Network.LOCAL_PLAYER_ID
                })
                .OrderByDescending(e => e.Score)
                .Take(maxEntriesToShow)
                .ToList();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>LEADERBOARD</b>");
            sb.AppendLine("─────────────────");

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                string medal = GetMedalEmoji(i);
                string color = entry.IsPlayer ? "#00FF00" : "#FFFFFF";
                
                sb.AppendLine($"{medal} <color={color}>{entry.Name}</color>: {entry.Score}");
            }

            scoreboardText.text = sb.ToString();
        }

        private string GetEntityName(int entityId)
        {
            if (entityId == GameplayConstants.Network.LOCAL_PLAYER_ID)
                return "YOU";
            
            return $"Bot {entityId}";
        }

        private string GetMedalEmoji(int rank)
        {
            switch (rank)
            {
                case 0: return "🥇";
                case 1: return "🥈";
                case 2: return "🥉";
                default: return $"{rank + 1}.";
            }
        }
    }
}
