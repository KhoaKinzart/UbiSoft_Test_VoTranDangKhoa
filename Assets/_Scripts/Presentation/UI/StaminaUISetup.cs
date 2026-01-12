using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaUISetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool createUIOnStart = true;
    [SerializeField] private Vector3 uiOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector2 sliderSize = new Vector2(100f, 15f);
    
    private Canvas _worldCanvas;
    private Slider _staminaSlider;
    private TextMeshProUGUI _dashText;

    private void Start()
    {
        if (createUIOnStart)
        {
            CreateStaminaUI();
        }
    }

    public void CreateStaminaUI()
    {
        GameObject uiRoot = new GameObject("StaminaUI");
        uiRoot.transform.SetParent(transform);
        uiRoot.transform.localPosition = uiOffset;

        _worldCanvas = uiRoot.AddComponent<Canvas>();
        _worldCanvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = uiRoot.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        RectTransform canvasRect = uiRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(200, 100);
        canvasRect.localScale = Vector3.one * 0.01f;

        GameObject sliderObj = new GameObject("StaminaSlider");
        sliderObj.transform.SetParent(uiRoot.transform, false);
        
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.sizeDelta = sliderSize;
        sliderRect.anchoredPosition = Vector2.zero;

        _staminaSlider = sliderObj.AddComponent<Slider>();
        _staminaSlider.minValue = 0f;
        _staminaSlider.maxValue = 100f;
        _staminaSlider.value = 100f;
        _staminaSlider.interactable = false;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, -10);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.green;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;
        fillRect.pivot = new Vector2(0, 0.5f);

        _staminaSlider.fillRect = fillRect;
        _staminaSlider.targetGraphic = fillImage;

        GameObject textObj = new GameObject("DashText");
        textObj.transform.SetParent(uiRoot.transform, false);
        _dashText = textObj.AddComponent<TextMeshProUGUI>();
        _dashText.text = "Dash: Ready!";
        _dashText.fontSize = 14;
        _dashText.color = Color.white;
        _dashText.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(120, 30);
        textRect.anchoredPosition = new Vector2(0, -25);

        PlayerVisual playerVisual = GetComponent<PlayerVisual>();
        if (playerVisual == null)
        {
            playerVisual = gameObject.AddComponent<PlayerVisual>();
        }

        ConnectToPlayerVisual(playerVisual, fillImage);
    }

    private void ConnectToPlayerVisual(PlayerVisual playerVisual, Image fillImage)
    {
        if (playerVisual != null)
        {
            playerVisual.staminaSlider = _staminaSlider;
            playerVisual.fillImage = fillImage;
            playerVisual.dashCooldownText = _dashText;
        }
    }

    public Slider GetStaminaSlider()
    {
        return _staminaSlider;
    }

    public TextMeshProUGUI GetDashText()
    {
        return _dashText;
    }
}