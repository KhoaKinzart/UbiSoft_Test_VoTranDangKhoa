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
    }

    [ContextMenu("Run Map Generation Test")]
    public void RunTest()
    {
        if (_isTesting)
        {
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

        for (int i = 0; i < testIterations; i++)
        {
            
            yield return _gridManager.GenerateMapRoutine(1, null);
            
            totalTests++;
            int obstacleCount = CountObstacles();

            if (obstacleCount == 0)
            {
                emptyMaps++;
            }
            else if (obstacleCount < 10)
            {
                lowObstacleMaps++;
            }
            else
            {
                goodMaps++;
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
    }

    [ContextMenu("Quick Single Test")]
    public void QuickTest()
    {
        if (_gridManager != null && _gridManager.GridData != null)
        {
            int count = CountObstacles();
        }
    }
}
