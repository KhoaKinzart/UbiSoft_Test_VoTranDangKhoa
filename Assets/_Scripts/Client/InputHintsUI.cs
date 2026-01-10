using UnityEngine;
using TMPro;

public class InputHintsUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private float displayDuration = 5f;
    
    private TextMeshProUGUI _hintsText;
    private Canvas _canvas;
    private float _displayTimer;

    private void Start()
    {
        if (showOnStart)
        {
            CreateHintsUI();
            _displayTimer = displayDuration;
        }
    }

    private void Update()
    {
        if (_displayTimer > 0)
        {
            _displayTimer -= Time.deltaTime;
            
            if (_displayTimer <= 0 && _hintsText != null)
            {
                _hintsText.gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.H) && _hintsText != null)
        {
            _hintsText.gameObject.SetActive(!_hintsText.gameObject.activeSelf);
        }
    }

    private void CreateHintsUI()
    {
        GameObject canvasObj = new GameObject("InputHintsCanvas");
        canvasObj.transform.SetParent(transform);
        
        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        GameObject textObj = new GameObject("HintsText");
        textObj.transform.SetParent(canvasObj.transform, false);
        
        _hintsText = textObj.AddComponent<TextMeshProUGUI>();
        _hintsText.fontSize = 18;
        _hintsText.color = Color.white;
        _hintsText.alignment = TextAlignmentOptions.TopLeft;
        
        _hintsText.text = 
            "<b>🎮 Controls:</b>\n" +
            "<color=yellow>WASD</color> - Move\n" +
            "<color=cyan>Left Shift</color> - Sprint (drains stamina)\n" +
            "<color=lime>Space</color> - Dash (costs 30 stamina, 2s cooldown)\n" +
            "<color=orange>H</color> - Toggle this help\n\n" +
            "<b>📊 HUD:</b>\n" +
            "<color=green>Green Bar</color> - Stamina (regenerates when not sprinting/dashing)\n" +
            "<color=red>Red Text</color> - Dash on cooldown\n" +
            "<color=lime>Green Text</color> - Dash ready!\n\n" +
            "<b>⚡ Stats:</b>\n" +
            "Sprint Speed: 1.5x\n" +
            "Dash Speed: 2.0x\n" +
            "Dash Duration: 0.3s\n" +
            "Dash Cooldown: 2.0s\n" +
            "Sprint Cost: 20/s\n" +
            "Dash Cost: 30";

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 1);
        textRect.anchorMax = new Vector2(0, 1);
        textRect.pivot = new Vector2(0, 1);
        textRect.anchoredPosition = new Vector2(10, -10);
        textRect.sizeDelta = new Vector2(400, 500);

        GameObject background = new GameObject("Background");
        background.transform.SetParent(textObj.transform, false);
        background.transform.SetAsFirstSibling();
        
        UnityEngine.UI.Image bgImage = background.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = new Vector2(10, 10);
    }

    public void Show()
    {
        if (_hintsText != null)
        {
            _hintsText.gameObject.SetActive(true);
            _displayTimer = displayDuration;
        }
    }

    public void Hide()
    {
        if (_hintsText != null)
        {
            _hintsText.gameObject.SetActive(false);
        }
    }

    public void Toggle()
    {
        if (_hintsText != null)
        {
            _hintsText.gameObject.SetActive(!_hintsText.gameObject.activeSelf);
        }
    }
}
