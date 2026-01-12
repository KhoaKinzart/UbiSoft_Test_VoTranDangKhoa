using UnityEngine;
using TMPro;
using Game.Core.Events;
using Game.Core.Constants;

namespace Game.Presentation.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private TextMeshProUGUI leaderboardText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;

        private void OnEnable()
        {
            EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Subscribe<GameOverEvent>(OnGameOver);
            EventBus.Subscribe<GameTimerStartedEvent>(OnTimerStarted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
            EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
            EventBus.Unsubscribe<GameTimerStartedEvent>(OnTimerStarted);
        }

        private void Start()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        public void UpdateTimer(float timeRemaining)
        {
            if (timerText == null) return;

            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
            
            if (timeRemaining < 10f)
            {
                timerText.color = Color.red;
            }
            else if (timeRemaining < 30f)
            {
                timerText.color = Color.yellow;
            }
            else
            {
                timerText.color = Color.white;
            }
        }

        public void UpdatePlayerScore(int score)
        {
            if (playerScoreText == null) return;
            playerScoreText.text = $"Your Score: {score}";
        }

        public void UpdateLeaderboard(System.Collections.Generic.Dictionary<int, int> scores)
        {
            if (leaderboardText == null) return;

            var sortedScores = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<int, int>>(scores);
            sortedScores.Sort((a, b) => b.Value.CompareTo(a.Value));

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Leaderboard:");
            
            int rank = 1;
            foreach (var kvp in sortedScores)
            {
                string name = kvp.Key == GameplayConstants.Network.LOCAL_PLAYER_ID 
                    ? "You" 
                    : $"Bot {kvp.Key}";
                
                sb.AppendLine($"{rank}. {name}: {kvp.Value}");
                rank++;
                
                if (rank > 5) break;
            }

            leaderboardText.text = sb.ToString();
        }

        private void OnScoreChanged(ScoreChangedEvent evt)
        {
            if (evt.EntityID == GameplayConstants.Network.LOCAL_PLAYER_ID)
            {
                UpdatePlayerScore(evt.NewScore);
            }
        }

        private void OnGameOver(GameOverEvent evt)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (gameOverText != null)
            {
                string winnerName = evt.IsPlayer ? "YOU WIN!" : $"Bot {evt.WinnerID} Wins!";
                gameOverText.text = $"{winnerName}\n\nFinal Score: {evt.WinnerScore} eggs";
                gameOverText.color = evt.IsPlayer ? Color.green : Color.red;
            }
        }

        private void OnTimerStarted(GameTimerStartedEvent evt)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }
    }
}
