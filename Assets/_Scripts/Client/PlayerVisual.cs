using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerVisual : MonoBehaviour
{
    [Header("Stamina UI")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI dashCooldownText;
    
    [Header("Colors")]
    [SerializeField] private Color fullStaminaColor = Color.green;
    [SerializeField] private Color lowStaminaColor = Color.red;
    [SerializeField] private Color dashingColor = Color.cyan;
    
    [Header("Dash Feedback")]
    [SerializeField] private GameObject dashEffectPrefab;
    [SerializeField] private float dashEffectDuration = 0.5f;
    
    private bool _isDashing;
    private float _dashCooldown;
    
    public void SetStamina(float value)
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = value;
            
            if (fillImage != null)
            {
                if (_isDashing)
                {
                    fillImage.color = dashingColor;
                }
                else
                {
                    fillImage.color = Color.Lerp(lowStaminaColor, fullStaminaColor, value / 100f);
                }
            }
        }
    }
    
    public void SetDashState(bool isDashing)
    {
        _isDashing = isDashing;
        
        if (isDashing && dashEffectPrefab != null)
        {
            GameObject effect = Instantiate(dashEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, dashEffectDuration);
        }
    }
    
    public void SetDashCooldown(float cooldown)
    {
        _dashCooldown = cooldown;
        
        if (dashCooldownText != null)
        {
            if (cooldown > 0)
            {
                dashCooldownText.text = $"Dash: {cooldown:F1}s";
                dashCooldownText.color = Color.red;
            }
            else
            {
                dashCooldownText.text = "Dash: Ready!";
                dashCooldownText.color = Color.green;
            }
        }
    }
    
    void LateUpdate()
    {
        if (staminaSlider != null && Camera.main != null)
        {
            staminaSlider.transform.parent.rotation = Camera.main.transform.rotation;
        }
    }
}
