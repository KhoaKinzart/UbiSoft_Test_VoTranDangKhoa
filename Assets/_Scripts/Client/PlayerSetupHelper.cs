using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(PlayerInput))]
public class PlayerSetupHelper : MonoBehaviour
{
    [Header("Auto Setup Options")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool addStaminaUI = true;
    [SerializeField] private bool addInputHints = true;
    
    [Header("UI Settings")]
    [SerializeField] private Vector3 staminaUIOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Vector2 sliderSize = new Vector2(100f, 15f);

    private void Start()
    {
        if (setupOnStart)
        {
            SetupPlayer();
        }
    }

    [ContextMenu("Setup Player (Full)")]
    public void SetupPlayer()
    {
        if (addStaminaUI)
        {
            SetupStaminaUI();
        }

        if (addInputHints)
        {
            SetupInputHints();
        }
    }

    private void SetupStaminaUI()
    {
        if (GetComponent<PlayerVisual>() != null)
        {
            return;
        }

        StaminaUISetup uiSetup = GetComponent<StaminaUISetup>();
        if (uiSetup == null)
        {
            uiSetup = gameObject.AddComponent<StaminaUISetup>();
        }

        uiSetup.CreateStaminaUI();

        PlayerVisual playerVisual = GetComponent<PlayerVisual>();
        if (playerVisual != null)
        {
            Slider slider = uiSetup.GetStaminaSlider();
            TextMeshProUGUI dashText = uiSetup.GetDashText();
        }
    }

    private void SetupInputHints()
    {
        if (GetComponent<InputHintsUI>() == null)
        {
            gameObject.AddComponent<InputHintsUI>();
        }
    }

    [ContextMenu("Remove All UI")]
    public void RemoveAllUI()
    {
        PlayerVisual visual = GetComponent<PlayerVisual>();
        if (visual != null) DestroyImmediate(visual);

        StaminaUISetup setup = GetComponent<StaminaUISetup>();
        if (setup != null) DestroyImmediate(setup);

        InputHintsUI hints = GetComponent<InputHintsUI>();
        if (hints != null) DestroyImmediate(hints);

        Transform staminaUI = transform.Find("StaminaUI");
        if (staminaUI != null) DestroyImmediate(staminaUI.gameObject);
    }
}
