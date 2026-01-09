using UnityEngine;
using System.Collections.Generic;

public class PathfindingTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Debug Controls")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color pathColor = Color.cyan;

    // L?u ?i?m b?t ??u và k?t thúc (T?a ?? Grid)
    private Vector2Int startCoord;
    private Vector2Int endCoord;

    // Engine tìm ???ng
    private AStar aStarEngine;
    private List<Vector2Int> currentPath;

    // Bi?n ki?m tra
    private bool isInitialized = false;

    void Start()
    {
        // M?c ??nh ch?n ?i?m (0,0) và (1,1) ?? test
        startCoord = new Vector2Int(0, 0);
        endCoord = new Vector2Int(1, 1);
    }

    void Update()
    {
        // 1. Ch? GridManager sinh map xong m?i kh?i t?o A*
        if (!isInitialized)
        {
            // THÊM: && gridManager.IsMapReady
            if (gridManager != null && gridManager.GridData != null && gridManager.IsMapReady)
            {
                InitializeEngine();
            }
            return;
        }

        // 2. Nh?n Input chu?t ?? test cho l? (Trái: Start, Ph?i: End)
        HandleMouseInput();
    }

    private void InitializeEngine()
    {
        // Kh?i t?o A* v?i kích th??c th?t c?a Map
        aStarEngine = new AStar(gridManager.CurrentWidth, gridManager.CurrentHeight);

        // N?p d? li?u v?t c?n vào A*
        aStarEngine.UpdateGridObstacles(gridManager.GridData);

        isInitialized = true;
        Debug.Log("Pathfinding Tester Ready! Click chu?t trái/ph?i ?? ch?n ?i?m ?i.");

        RecalculatePath();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // Chu?t trái -> Ch?n ?i?m b?t ??u
        {
            SetCoordinateFromMouse(ref startCoord);
            RecalculatePath();
        }
        else if (Input.GetMouseButtonDown(1)) // Chu?t ph?i -> Ch?n ?ích ??n
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
            // ??i t? World Position (Unity) sang Grid Coordinate (A*)
            int x = Mathf.RoundToInt(hit.point.x);
            int z = Mathf.RoundToInt(hit.point.z);

            // K?p trong ph?m vi b?n ??
            x = Mathf.Clamp(x, 0, gridManager.CurrentWidth - 1);
            z = Mathf.Clamp(z, 0, gridManager.CurrentHeight - 1);

            targetCoord = new Vector2Int(x, z);
            Debug.Log($"Selected Point: {targetCoord}");
        }
    }

    private void RecalculatePath()
    {
        if (aStarEngine == null) return;

        // G?i thu?t toán A* c?a chúng ta
        currentPath = aStarEngine.FindPath(startCoord, endCoord);

        if (currentPath == null)
        {
            Debug.LogWarning("Không tìm th?y ???ng ?i! (Có th? b? ch?n)");
        }
    }

    // --- PH?N QUAN TR?NG: V? GIZMOS ---
    void OnDrawGizmos()
    {
        if (!showGizmos || gridManager == null) return;

        // 1. V? ?i?m B?t ??u (C?c màu Xanh Lá)
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(startCoord.x, 0.5f, startCoord.y), Vector3.one * 0.8f);

        // 2. V? ?i?m ?ích (C?c màu ??)
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(endCoord.x, 0.5f, endCoord.y), Vector3.one * 0.8f);

        // 3. V? ???ng ?i (Line màu Cyan)
        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = pathColor;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Vector2Int nodeA = currentPath[i];
                Vector2Int nodeB = currentPath[i + 1];

                Vector3 posA = new Vector3(nodeA.x, 1f, nodeA.y); // V? cao lên Y=1 cho d? th?y
                Vector3 posB = new Vector3(nodeB.x, 1f, nodeB.y);

                Gizmos.DrawLine(posA, posB);
                Gizmos.DrawSphere(posA, 0.2f); // V? thêm c?c tròn nh? ? m?i b??c
            }
        }
    }
}