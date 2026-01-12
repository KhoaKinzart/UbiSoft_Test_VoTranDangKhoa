using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Core.Events;
using UnityEngine.SceneManagement;

namespace Game.Presentation.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI winnerText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Settings")]
        [SerializeField] private Color winColor = Color.green;
        [SerializeField] private Color loseColor = Color.red;
        [SerializeField] private string mainMenuSceneName = "StartMenu";

        private void OnEnable()
        {
            EventBus.Subscribe<GameOverEvent>(OnGameOver);
            EventBus.Subscribe<GameTimerStartedEvent>(OnGameStarted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameOverEvent>(OnGameOver);
            EventBus.Unsubscribe<GameTimerStartedEvent>(OnGameStarted);
        }

        private void Start()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnGameOver(GameOverEvent evt)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (winnerText != null)
            {
                string winnerName = evt.IsPlayer ? "YOU WIN!" : $"Bot {evt.WinnerID} Wins!";
                winnerText.text = winnerName;
                winnerText.color = evt.IsPlayer ? winColor : loseColor;
            }

            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {evt.WinnerScore} eggs collected";
            }
        }

        private void OnGameStarted(GameTimerStartedEvent evt)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void OnRestartClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnMainMenuClicked()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            }
        }
    }
}
