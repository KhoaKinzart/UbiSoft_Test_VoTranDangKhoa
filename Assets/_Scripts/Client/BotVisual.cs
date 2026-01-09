using UnityEngine;
using UnityEngine.UI;

public class BotVisual : MonoBehaviour
{
    [SerializeField] private Slider staminaSlider; 

    public void SetStamina(float value)
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = value;
        }
    }

    void LateUpdate()
    {
        if (staminaSlider != null)
        {
            staminaSlider.transform.parent.rotation = Camera.main.transform.rotation;
        }
    }
}