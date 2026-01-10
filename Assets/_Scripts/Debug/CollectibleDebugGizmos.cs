using UnityEngine;

public class CollectibleDebugGizmos : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float rotateSpeed = 90f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color pickupRangeColor = new Color(1, 1, 0, 0.15f);
    [SerializeField] private float pickupRange = 0.6f;

    private Vector3 _startPosition;
    private float _timeOffset;

    private void Awake()
    {
        if (!gameObject.CompareTag("Collectible"))
        {
            gameObject.tag = "Collectible";
        }

        _startPosition = transform.position;
        _timeOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    private void Update()
    {
        // Bob up and down
        float newY = _startPosition.y + Mathf.Sin(Time.time * bobSpeed + _timeOffset) * bobHeight;
        transform.position = new Vector3(_startPosition.x, newY, _startPosition.z);

        // Rotate
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        Vector3 pos = Application.isPlaying ? _startPosition : transform.position;

        // Pickup range (transparent yellow sphere)
        Gizmos.color = pickupRangeColor;
        Gizmos.DrawSphere(pos, pickupRange);

        // Wireframe for visibility
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawWireSphere(pos, pickupRange);

        // Center dot
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pos, 0.15f);

        // Grid position indicator
        Vector3 gridPos = new Vector3(
            Mathf.Round(pos.x),
            0.1f,
            Mathf.Round(pos.z)
        );
        Gizmos.color = new Color(1, 1, 1, 0.3f);
        Gizmos.DrawWireCube(gridPos, Vector3.one * 0.8f);

#if UNITY_EDITOR
        // Label
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.Label(
            pos + Vector3.up * 1.2f,
            $"Pickup: {pickupRange:F1}m"
        );
#endif
    }
}
