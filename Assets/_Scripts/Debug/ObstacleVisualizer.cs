using UnityEngine;

public class ObstacleVisualizer : MonoBehaviour
{
    [Header("Visualization")]
    [SerializeField] private bool showObstacles = true;
    [SerializeField] private Color obstacleColor = Color.black;
    [SerializeField] private float obstacleHeight = 1f;
    
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
        if (!showObstacles || _gridManager == null) return;
        if (_gridManager.GridData == null) return;

        Gizmos.color = obstacleColor;

        int width = _gridManager.CurrentWidth;
        int height = _gridManager.CurrentHeight;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (_gridManager.GridData[x, z] == 1)
                {
                    Vector3 pos = new Vector3(x, obstacleHeight / 2f, z);
                    Gizmos.DrawCube(pos, Vector3.one * 0.9f);
                    
                    Gizmos.color = new Color(obstacleColor.r, obstacleColor.g, obstacleColor.b, 0.3f);
                    Gizmos.DrawWireCube(pos, Vector3.one);
                    Gizmos.color = obstacleColor;
                }
            }
        }
    }
}
