using UnityEngine;
using TMPro;

public class DashDebugUI : MonoBehaviour
{
    [SerializeField] private bool showDebugInfo = true;
    
    private TextMeshProUGUI _debugText;
    private SimulationController _simulation;
    
    private void Start()
    {
        _simulation = FindObjectOfType<SimulationController>();
        CreateDebugUI();
    }
    
    private void CreateDebugUI()
    {
        if (!showDebugInfo) return;
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        GameObject textObj = new GameObject("DashDebugText");
        textObj.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(10, -100);
        rect.sizeDelta = new Vector2(300, 150);
        
        _debugText = textObj.AddComponent<TextMeshProUGUI>();
        _debugText.fontSize = 14;
        _debugText.color = Color.yellow;
        _debugText.alignment = TextAlignmentOptions.TopLeft;
    }
    
    private void Update()
    {
        if (!showDebugInfo || _debugText == null || _simulation?.Simulation == null)
            return;
        
        var player = _simulation.Simulation.GetLocalPlayer();
        if (player == null)
        {
            _debugText.text = "<color=red>No player</color>";
            return;
        }
        
        bool spacePressed = Input.GetKey(KeyCode.Space);
        bool spaceDown = Input.GetKeyDown(KeyCode.Space);
        
        string dashStatus = player.IsDashing ? 
            "<color=cyan><b>⚡ DASHING!</b></color>" : 
            "<color=white>Idle</color>";
        
        string cooldownStatus = player.DashCooldownTimer > 0 ?
            $"<color=red>Cooldown: {player.DashCooldownTimer:F1}s</color>" :
            "<color=lime>✅ Ready!</color>";
        
        float dashCost = 30f;
        bool hasStamina = player.CurrentStamina >= dashCost;
        string staminaStatus = hasStamina ?
            $"<color=lime>{player.CurrentStamina:F0}/{dashCost}</color>" :
            $"<color=red>{player.CurrentStamina:F0}/{dashCost} ❌</color>";
        
        bool isMoving = player.LastInput.magnitude > 0.1f;
        string movingStatus = isMoving ?
            "<color=lime>✅ Moving</color>" :
            "<color=orange>⚠️ Not moving</color>";
        
        bool shouldRegen = !player.IsSprinting && !player.IsDashing;
        string regenStatus = shouldRegen ?
            "<color=lime>✅ +20/s</color>" :
            "<color=orange>⏸️ Paused</color>";
        
        string sprintStatus = player.IsSprinting ?
            "<color=yellow>🏃 Sprinting (-25/s)</color>" :
            "Walking";
        
        _debugText.text = $"<b>DASH DEBUG</b>\n" +
                         $"━━━━━━━━━━━━━━━━\n" +
                         $"Stamina: {staminaStatus}\n" +
                         $"Regen: {regenStatus}\n" +
                         $"State: {sprintStatus}\n" +
                         $"━━━━━━━━━━━━━━━━\n" +
                         $"Dash: {dashStatus}\n" +
                         $"{cooldownStatus}\n" +
                         $"Movement: {movingStatus}\n" +
                         $"Space: {(spaceDown ? "<color=lime>✅ PRESSED</color>" : (spacePressed ? "<color=cyan>⬇️ HELD</color>" : "Released"))}\n" +
                         $"\n" +
                         $"<size=10><i>SPACE = Dash | SHIFT = Sprint</i></size>";
    }
}
