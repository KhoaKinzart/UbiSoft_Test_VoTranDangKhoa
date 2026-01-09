using UnityEngine;
using System.Collections.Generic;

public class PathfindingTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Debug Controls")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color pathColor = Color.cyan;

    private Vector2Int startCoord;
    private Vector2Int endCoord;


    private AStar aStarEngine;
    private List<Vector2Int> currentPath;


    private bool isInitialized = false;

    void Start()
    {

        startCoord = new Vector2Int(0, 0);
        endCoord = new Vector2Int(1, 1);
    }

    void Update()
    {

        if (!isInitialized)
        {

            if (gridManager != null && gridManager.GridData != null && gridManager.IsMapReady)
            {
                InitializeEngine();
            }
            return;
        }

        HandleMouseInput();
    }

    private void InitializeEngine()
    {

        aStarEngine = new AStar(gridManager.CurrentWidth, gridManager.CurrentHeight);

        aStarEngine.UpdateGridObstacles(gridManager.GridData);

        isInitialized = true;
        Debug.Log("Pathfinding Tester Ready! Click chu?t tr�i/ph?i ?? ch?n ?i?m ?i.");

        RecalculatePath();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            SetCoordinateFromMouse(ref startCoord);
            RecalculatePath();
        }
        else if (Input.GetMouseButtonDown(1)) 
        {
            SetCoordinateFromMouse(ref endCoord);
            RecalculatePath();
        }
    }

    private void SetCoordinateFromMouse(ref Vector2Int targetCoord)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            int x = Mathf.RoundToInt(hit.point.x);
            int z = Mathf.RoundToInt(hit.point.z);

            x = Mathf.Clamp(x, 0, gridManager.CurrentWidth - 1);
            z = Mathf.Clamp(z, 0, gridManager.CurrentHeight - 1);

            targetCoord = new Vector2Int(x, z);
            Debug.Log($"Selected Point: {targetCoord}");
        }
    }

    private void RecalculatePath()
    {
        if (aStarEngine == null) return;
        currentPath = aStarEngine.FindPath(startCoord, endCoord);

        if (currentPath == null)
        {
            Debug.LogWarning("Kh�ng t�m th?y ???ng ?i! (C� th? b? ch?n)");
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos || gridManager == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(startCoord.x, 0.5f, startCoord.y), Vector3.one * 0.8f);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(endCoord.x, 0.5f, endCoord.y), Vector3.one * 0.8f);

        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = pathColor;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Vector2Int nodeA = currentPath[i];
                Vector2Int nodeB = currentPath[i + 1];

                Vector3 posA = new Vector3(nodeA.x, 1f, nodeA.y); 
                Vector3 posB = new Vector3(nodeB.x, 1f, nodeB.y);

                Gizmos.DrawLine(posA, posB);
                Gizmos.DrawSphere(posA, 0.2f); 
            }
        }
    }
}