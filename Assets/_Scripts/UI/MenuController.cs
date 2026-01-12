using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Core;

namespace Game.UI
{
    public class MenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject instructionsPanel;
        
        [Header("Main Menu Buttons")]
        [SerializeField] private Button mainStartButton;
        [SerializeField] private Button instructionsButton;
        [SerializeField] private Button exitButton;
        
        [Header("Settings Panel")]
        [SerializeField] private TMP_InputField botCountInput;
        [SerializeField] private TMP_InputField gameDurationInput;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsBackButton;
        
        [Header("Instructions Panel")]
        [SerializeField] private Button instructionsBackButton;
        
        [Header("Settings")]
        [SerializeField] private int defaultBotCount = 20;
        [SerializeField] private float defaultGameDuration = 120f;
        [SerializeField] private int minBotCount = 1;
        [SerializeField] private int maxBotCount = 100;
        [SerializeField] private float minDuration = 30f;
        [SerializeField] private float maxDuration = 600f;
        
        [Header("Scene")]
        [SerializeField] private string gameSceneName = "PlayScene";
        
        private void Start()
        {
            InitializeInputFields();
            SetupButtonListeners();
            ShowMainMenu();
        }
        
        private void SetupButtonListeners()
        {
            if (mainStartButton != null)
            {
                mainStartButton.onClick.AddListener(ShowSettings);
            }
            
            if (instructionsButton != null)
            {
                instructionsButton.onClick.AddListener(ShowInstructions);
            }
            
            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitClicked);
            }
            
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
            
            if (settingsBackButton != null)
            {
                settingsBackButton.onClick.AddListener(ShowMainMenu);
            }
            
            if (instructionsBackButton != null)
            {
                instructionsBackButton.onClick.AddListener(ShowMainMenu);
            }
            
            if (botCountInput != null)
            {
                botCountInput.onValueChanged.AddListener(OnBotCountChanged);
            }
            
            if (gameDurationInput != null)
            {
                gameDurationInput.onValueChanged.AddListener(OnDurationChanged);
            }
        }
        
        private void InitializeInputFields()
        {
            if (botCountInput != null)
            {
                botCountInput.text = defaultBotCount.ToString();
                botCountInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            }
            
            if (gameDurationInput != null)
            {
                gameDurationInput.text = defaultGameDuration.ToString();
                gameDurationInput.contentType = TMP_InputField.ContentType.DecimalNumber;
            }
        }
        
        private void ShowMainMenu()
        {
            SetPanelActive(mainMenuPanel, true);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(instructionsPanel, false);
        }
        
        private void ShowSettings()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(settingsPanel, true);
            SetPanelActive(instructionsPanel, false);
        }
        
        private void ShowInstructions()
        {
            SetPanelActive(mainMenuPanel, false);
            SetPanelActive(settingsPanel, false);
            SetPanelActive(instructionsPanel, true);
        }
        
        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
        
        private void OnBotCountChanged(string value)
        {
            if (int.TryParse(value, out int botCount))
            {
                botCount = Mathf.Clamp(botCount, minBotCount, maxBotCount);
                botCountInput.text = botCount.ToString();
            }
        }
        
        private void OnDurationChanged(string value)
        {
            if (float.TryParse(value, out float duration))
            {
                duration = Mathf.Clamp(duration, minDuration, maxDuration);
                gameDurationInput.text = duration.ToString("F0");
            }
        }
        
        private void OnPlayButtonClicked()
        {
            int botCount = defaultBotCount;
            float gameDuration = defaultGameDuration;
            
            if (botCountInput != null && int.TryParse(botCountInput.text, out int inputBotCount))
            {
                botCount = Mathf.Clamp(inputBotCount, minBotCount, maxBotCount);
            }
            
            if (gameDurationInput != null && float.TryParse(gameDurationInput.text, out float inputDuration))
            {
                gameDuration = Mathf.Clamp(inputDuration, minDuration, maxDuration);
            }
            
            GameSettings.Instance.SetGameSettings(botCount, gameDuration);
            
            SceneManager.LoadScene(gameSceneName);
        }
        
        private void OnExitClicked()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        private void OnDestroy()
        {
            if (mainStartButton != null)
            {
                mainStartButton.onClick.RemoveListener(ShowSettings);
            }
            
            if (instructionsButton != null)
            {
                instructionsButton.onClick.RemoveListener(ShowInstructions);
            }
            
            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(OnExitClicked);
            }
            
            if (playButton != null)
            {
                playButton.onClick.RemoveListener(OnPlayButtonClicked);
            }
            
            if (settingsBackButton != null)
            {
                settingsBackButton.onClick.RemoveListener(ShowMainMenu);
            }
            
            if (instructionsBackButton != null)
            {
                instructionsBackButton.onClick.RemoveListener(ShowMainMenu);
            }
        }
    }
}
