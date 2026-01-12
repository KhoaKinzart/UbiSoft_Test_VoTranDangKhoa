using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Core;

namespace Game.UI
{
    public class PauseManager : MonoBehaviour
    {
        [Header("Pause Panel")]
        [SerializeField] private GameObject pausePanel;
        
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        [Header("Components to Disable on Pause")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Camera mainCamera;
        
        [Header("Settings")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private string mainMenuSceneName = "StartMenu";
        
        private bool isPaused = false;
        private bool wasRenderingEnabled;
        
        private void Start()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            
            SetupButtonListeners();
            
            if (playerInput == null)
            {
                playerInput = FindObjectOfType<PlayerInput>();
            }
            
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
        
        private void SetupButtonListeners()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(Resume);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                TogglePause();
            }
        }
        
        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
        
        public void Pause()
        {
            if (isPaused) return;
            
            isPaused = true;
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
            
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }
            
            if (mainCamera != null)
            {
                wasRenderingEnabled = mainCamera.enabled;
                mainCamera.enabled = false;
            }
        }
        
        public void Resume()
        {
            if (!isPaused) return;
            
            isPaused = false;
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
            
            if (playerInput != null)
            {
                playerInput.enabled = true;
            }
            
            if (mainCamera != null)
            {
                mainCamera.enabled = wasRenderingEnabled;
            }
        }
        
        public void ReturnToMainMenu()
        {
            Resume();
            
            CleanupGameObjects();
            
            SceneManager.LoadScene(mainMenuSceneName);
        }
        
        private void CleanupGameObjects()
        {
            if (GameSettings.Instance != null)
            {
                Destroy(GameSettings.Instance.gameObject);
            }
            
            var simulationController = FindObjectOfType<SimulationController>();
            if (simulationController != null)
            {
                Destroy(simulationController.gameObject);
            }
            
            var presentationController = FindObjectOfType<PresentationController>();
            if (presentationController != null)
            {
                Destroy(presentationController.gameObject);
            }
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        private void OnDestroy()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(Resume);
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
            }
        }
        
        public bool IsPaused => isPaused;
    }
}
