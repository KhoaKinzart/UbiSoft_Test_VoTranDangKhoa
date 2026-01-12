using UnityEngine;
using UnityEngine.UI;

public class BotVisual : MonoBehaviour
{
    [SerializeField] private Slider staminaSlider;
    
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void SetStamina(float value)
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = value;
        }
    }

    void LateUpdate()
    {
        if (staminaSlider != null && mainCamera != null)
        {
            staminaSlider.transform.parent.rotation = mainCamera.transform.rotation;
        }
    }
}