using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game.Debug
{
    public class NetworkColorLegend : MonoBehaviour
    {
        private void Start()
        {
            CreateLegendUI();
        }
        
        private void CreateLegendUI()
        {
            GameObject canvas = GameObject.Find("LatencySimulatorCanvas");
            if (canvas == null)
            {
                canvas = new GameObject("LatencySimulatorCanvas");
                Canvas canvasComp = canvas.AddComponent<Canvas>();
                canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasComp.sortingOrder = 100;
                
                UnityEngine.UI.CanvasScaler scaler = canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            
            GameObject panel = CreateLegendPanel(canvas.transform);
            CreateLegendContent(panel.transform);
        }
        
        private GameObject CreateLegendPanel(Transform parent)
        {
            GameObject panel = new GameObject("ColorLegendPanel");
            panel.transform.SetParent(parent);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
            rect.sizeDelta = new Vector2(300, 240);
            
            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.85f);
            
            GameObject title = new GameObject("Title");
            title.transform.SetParent(panel.transform);
            
            TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "<b><color=yellow>COLOR LEGEND</color></b>";
            titleText.fontSize = 18;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.anchoredPosition = new Vector2(0, -10);
            titleRect.sizeDelta = new Vector2(0, 30);
            
            return panel;
        }
        
        private void CreateLegendContent(Transform parent)
        {
            GameObject content = new GameObject("Content");
            content.transform.SetParent(parent);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = new Vector2(0, -50);
            contentRect.sizeDelta = new Vector2(-40, 180);
            
            TextMeshProUGUI text = content.AddComponent<TextMeshProUGUI>();
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.color = Color.white;
            
            text.text = 
                "<b><color=#00FF00>═══ BOTS ═══</color></b>\n" +
                "<color=black>●</color><color=white>●●</color> <b>BLACK</b> = Bot Server\n" +
                "<color=red>●●●</color> <b>RED</b> = Bot Client\n" +
                "\n" +
                "<b><color=#00FFFF>═══ PLAYER ═══</color></b>\n" +
                "<color=white>●●●</color> <b>WHITE</b> = Player Server\n" +
                "<color=blue>●●●</color> <b>BLUE</b> = Player Client\n" +
                "\n" +
                "<color=yellow>━━━</color> Yellow Line = Error\n" +
                "\n" +
                "<size=11><i>Enable Gizmos in Scene View</i></size>";
        }
    }
}
