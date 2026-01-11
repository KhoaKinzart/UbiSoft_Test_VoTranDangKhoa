using UnityEngine;
using Game.Rendering;

namespace Game.Debug
{
    public class ChunkLoadingDebugUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChunkLoadingSystem chunkLoadingSystem;
        
        [Header("UI Settings")]
        [SerializeField] private bool showDebugUI = true;
        [SerializeField] private int fontSize = 16;
        [SerializeField] private Color textColor = Color.green;
        
        private GUIStyle _style;
        
        private void OnGUI()
        {
            if (!showDebugUI || chunkLoadingSystem == null) return;
            
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label);
                _style.fontSize = fontSize;
                _style.normal.textColor = textColor;
                _style.alignment = TextAnchor.UpperLeft;
            }
            
            int totalObstacles = GetPrivateField<int>("totalObstacles");
            int activeObstacles = GetPrivateField<int>("activeObstacles");
            int loadedChunks = GetPrivateField<int>("loadedChunks");
            int totalChunks = GetPrivateField<int>("totalChunks");
            float efficiency = GetPrivateField<float>("efficiencyPercent");
            
            string debugText = $"=== CHUNK LOADING DEBUG ===\n\n";
            debugText += $"Chunks: {loadedChunks} / {totalChunks} loaded\n";
            debugText += $"Obstacles: {activeObstacles} / {totalObstacles} active\n";
            debugText += $"Efficiency: {efficiency:F1}% saved\n\n";
            
            if (totalObstacles > 0)
            {
                float loadPercent = (activeObstacles / (float)totalObstacles) * 100f;
                debugText += $"Load: {loadPercent:F1}%\n";
                debugText += $"Saved: {totalObstacles - activeObstacles} obstacles\n\n";
            }
            
            debugText += "📦 Chunk loading is ACTIVE!\n";
            debugText += "Green cubes = Loaded chunks\n";
            debugText += "Red cubes = Unloaded chunks";
            
            GUI.Label(new Rect(10, 10, 400, 300), debugText, _style);
        }
        
        private T GetPrivateField<T>(string fieldName)
        {
            var field = typeof(ChunkLoadingSystem).GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
                return (T)field.GetValue(chunkLoadingSystem);
            
            return default(T);
        }
    }
}
