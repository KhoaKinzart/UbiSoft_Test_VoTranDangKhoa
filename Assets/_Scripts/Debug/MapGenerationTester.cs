using UnityEngine;
using System.Collections;

public class MapGenerationTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private int testIterations = 20;
    [SerializeField] private float delayBetweenTests = 0.5f;
    
    [Header("Statistics")]
    [SerializeField] private int totalTests = 0;
    [SerializeField] private int emptyMaps = 0;
    [SerializeField] private int lowObstacleMaps = 0;
    [SerializeField] private int goodMaps = 0;
    
    private GridManager _gridManager;
    private bool _isTesting = false;

    private void Start()
    {
        _gridManager = FindObjectOfType<GridManager>();
        if (_gridManager == null)
        {
            Debug.LogError("GridManager not found!");
        }
    }

    [ContextMenu("Run Map Generation Test")]
    public void RunTest()
    {
        if (_isTesting)
        {
            Debug.LogWarning("Test already running!");
            return;
        }

        StartCoroutine(TestMapGeneration());
    }

    private IEnumerator TestMapGeneration()
    {
        _isTesting = true;
        totalTests = 0;
        emptyMaps = 0;
        lowObstacleMaps = 0;
        goodMaps = 0;

        Debug.Log($"<color=cyan>🧪 Starting map generation test ({testIterations} iterations)...</color>");

        for (int i = 0; i < testIterations; i++)
        {
            Debug.Log($"<color=yellow>--- Test {i + 1}/{testIterations} ---</color>");
            
            yield return _gridManager.GenerateMapRoutine(1, null);
            
            totalTests++;
            int obstacleCount = CountObstacles();

            if (obstacleCount == 0)
            {
                emptyMaps++;
                Debug.LogError($"<color=red>❌ EMPTY MAP DETECTED! (Test {i + 1})</color>");
            }
            else if (obstacleCount < 10)
            {
                lowObstacleMaps++;
                Debug.LogWarning($"<color=orange>⚠️ Low obstacles: {obstacleCount} (Test {i + 1})</color>");
            }
            else
            {
                goodMaps++;
                Debug.Log($"<color=lime>✅ Good map: {obstacleCount} obstacles (Test {i + 1})</color>");
            }

            yield return new WaitForSeconds(delayBetweenTests);
        }

        PrintReport();
        _isTesting = false;
    }

    private int CountObstacles()
    {
        if (_gridManager.GridData == null) return 0;

        int count = 0;
        for (int x = 0; x < _gridManager.CurrentWidth; x++)
        {
            for (int z = 0; z < _gridManager.CurrentHeight; z++)
            {
                if (_gridManager.GridData[x, z] == 1)
                    count++;
            }
        }
        return count;
    }

    private void PrintReport()
    {
        float emptyPercent = (float)emptyMaps / totalTests * 100f;
        float lowPercent = (float)lowObstacleMaps / totalTests * 100f;
        float goodPercent = (float)goodMaps / totalTests * 100f;

        Debug.Log("\n" +
            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
            "<color=cyan><b>📊 MAP GENERATION TEST REPORT</b></color>\n" +
            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
            $"Total Tests: <color=white><b>{totalTests}</b></color>\n" +
            $"\n" +
            $"<color=lime>✅ Good Maps:</color> {goodMaps} ({goodPercent:F1}%)\n" +
            $"<color=orange>⚠️ Low Obstacles:</color> {lowObstacleMaps} ({lowPercent:F1}%)\n" +
            $"<color=red>❌ Empty Maps:</color> {emptyMaps} ({emptyPercent:F1}%)\n" +
            $"\n" +
            (emptyMaps > 0 
                ? "<color=red><b>⚠️ ISSUE DETECTED!</b> Empty maps found. Check fallback logic.</color>\n"
                : "<color=lime><b>✅ ALL GOOD!</b> No empty maps detected.</color>\n") +
            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
    }

    [ContextMenu("Quick Single Test")]
    public void QuickTest()
    {
        if (_gridManager != null && _gridManager.GridData != null)
        {
            int count = CountObstacles();
            Debug.Log($"<color=cyan>Current map has {count} obstacles</color>");
        }
        else
        {
            Debug.LogWarning("No map data available!");
        }
    }
}
