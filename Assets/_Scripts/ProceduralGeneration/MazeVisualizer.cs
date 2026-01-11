using UnityEngine;

public class MazeVisualizer : MonoBehaviour
{
    [Header("Visualization")]
    [SerializeField] private bool showMazeStructure = true;
    [SerializeField] private bool showWalkableCells = true;
    [SerializeField] private bool showWalls = true;
    [SerializeField] private bool showGrid = true;

    [Header("Colors")]
    [SerializeField] private Color walkableColor = new Color(0, 1, 0, 0.3f);
    [SerializeField] private Color wallColor = new Color(1, 0, 0, 0.3f);
    [SerializeField] private Color gridColor = new Color(1, 1, 1, 0.1f);

    [Header("Settings")]
    [SerializeField] private float cellHeight = 0.5f;
    [SerializeField] private float wallHeight = 1f;

    private GridManager _gridManager;

    private void Start()
    {
        _gridManager = GetComponent<GridManager>();
        if (_gridManager == null)
        {
            _gridManager = FindObjectOfType<GridManager>();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showMazeStructure || _gridManager == null) return;
        if (_gridManager.GridData == null || !_gridManager.IsMapReady) return;

        int width = _gridManager.CurrentWidth;
        int height = _gridManager.CurrentHeight;

        if (!showGrid)
        {
            Gizmos.color = gridColor;
            for (int x = 0; x <= width; x++)
            {
                Gizmos.DrawLine(new Vector3(x, 0, 0), new Vector3(x, 0, height));
            }
            for (int z = 0; z <= height; z++)
            {
                Gizmos.DrawLine(new Vector3(0, 0, z), new Vector3(width, 0, z));
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(x + 0.5f, 0, z + 0.5f);

                if (_gridManager.GridData[x, z] == 0 && showWalkableCells)
                {
                    Gizmos.color = walkableColor;
                    Gizmos.DrawCube(pos + Vector3.up * cellHeight * 0.5f, new Vector3(0.9f, cellHeight, 0.9f));
                }
                else if (_gridManager.GridData[x, z] == 1 && showWalls)
                {
                    Gizmos.color = wallColor;
                    Gizmos.DrawCube(pos + Vector3.up * wallHeight * 0.5f, new Vector3(0.95f, wallHeight, 0.95f));
                }
            }
        }

        DrawStatistics(width, height);
    }

    private void DrawStatistics(int width, int height)
    {
        int walkableCount = 0;
        int wallCount = 0;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (_gridManager.GridData[x, z] == 0)
                    walkableCount++;
                else
                    wallCount++;
            }
        }

        float walkablePercent = (float)walkableCount / (width * height) * 100f;

#if UNITY_EDITOR
        Vector3 labelPos = new Vector3(width / 2f, 3f, height + 2f);
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(labelPos,
            $"<b>MAZE STATISTICS</b>\n" +
            $"Size: {width} x {height}\n" +
            $"Walkable: {walkableCount} ({walkablePercent:F1}%)\n" +
            $"Walls: {wallCount} ({100 - walkablePercent:F1}%)\n" +
            $"Total Cells: {width * height}",
            new GUIStyle()
            {
                fontSize = 12,
                normal = new GUIStyleState() { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            });
#endif
    }

    [ContextMenu("Analyze Maze Complexity")]
    public void AnalyzeMazeComplexity()
    {
        if (_gridManager == null || _gridManager.GridData == null)
        {
            return;
        }

        int width = _gridManager.CurrentWidth;
        int height = _gridManager.CurrentHeight;

        int walkableCount = 0;
        int deadEnds = 0;
        int corridors = 0;
        int junctions = 0;

        for (int x = 1; x < width - 1; x++)
        {
            for (int z = 1; z < height - 1; z++)
            {
                if (_gridManager.GridData[x, z] == 0)
                {
                    walkableCount++;

                    int neighborCount = CountWalkableNeighbors(x, z);

                    if (neighborCount == 1) deadEnds++;
                    else if (neighborCount == 2) corridors++;
                    else if (neighborCount >= 3) junctions++;
                }
            }
        }
    }

    private int CountWalkableNeighbors(int x, int z)
    {
        int count = 0;
        int width = _gridManager.CurrentWidth;
        int height = _gridManager.CurrentHeight;

        if (x > 0 && _gridManager.GridData[x - 1, z] == 0) count++;
        if (x < width - 1 && _gridManager.GridData[x + 1, z] == 0) count++;
        if (z > 0 && _gridManager.GridData[x, z - 1] == 0) count++;
        if (z < height - 1 && _gridManager.GridData[x, z + 1] == 0) count++;

        return count;
    }
}