using UnityEngine;
using Game.Simulation.Collision;
using Game.Core.ServiceLocator;
using Game.Simulation.Pathfinding;

public class PlayerInput : MonoBehaviour
{
    private SimulationController _simulationController;
    private CharacterController _controller;
    private int _playerId = -1;
    private bool _isInitialized = false;

    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private float smoothSpeed = 10f;

    private Vector2 _serverPosition;

    public void Initialize(SimulationController controller)
    {
        _simulationController = controller;
        _isInitialized = true;
        RegisterPlayer();
    }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void RegisterPlayer()
    {
        if (_simulationController == null)
        {
            return;
        }

        _playerId = _simulationController.RegisterPlayer(transform.position);
    }

    private void Update()
    {
        if (!_isInitialized || _simulationController == null || _playerId == -1)
            return;

        // Get server position
        var snapshots = _simulationController.Simulation?.GetSnapshots();
        if (snapshots != null)
        {
            foreach (var snap in snapshots)
            {
                if (snap.EntityID == _playerId)
                {
                    _serverPosition = snap.Position;
                    break;
                }
            }
        }

        // Send input to server ONLY
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool dash = Input.GetKeyDown(KeyCode.Space);
        
        _simulationController.SendPlayerInput(_playerId, input, sprint, dash);

        // Smooth move client to server position (NO PREDICTION)
        Vector3 targetPos = new Vector3(_serverPosition.x, transform.position.y, _serverPosition.y);
        Vector3 smoothedPos = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // Use CharacterController for physics
        Vector3 movement = smoothedPos - transform.position;
        _controller.Move(movement);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !_isInitialized) return;

        Vector3 clientPos = transform.position;
        Vector3 serverPos = new Vector3(_serverPosition.x, 0.5f, _serverPosition.y);
        float desyncDistance = Vector2.Distance(new Vector2(clientPos.x, clientPos.z), _serverPosition);

        // Client position (green)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(clientPos, 0.5f);
        Gizmos.DrawLine(clientPos, clientPos + Vector3.up * 2f);

        // Server position (red) - AUTHORITATIVE for pickup
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(serverPos, 0.6f);
        Gizmos.DrawLine(serverPos, serverPos + Vector3.up * 3f);

        // Server PICKUP RANGE (red transparent sphere)
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawSphere(serverPos, 0.6f); // PICKUP_DISTANCE = 0.6m

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawWireSphere(serverPos, 0.6f);

        // Server collision radius (smaller)
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.DrawWireSphere(serverPos, 0.3f); // PLAYER_RADIUS

        // Distance line
        Gizmos.color = desyncDistance > 0.3f ? Color.red : Color.yellow;
        Gizmos.DrawLine(clientPos, serverPos);

        // Find nearest collectible and check if in range
        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        if (collectibles.Length > 0)
        {
            GameObject nearest = null;
            float minDist = float.MaxValue;

            foreach (var col in collectibles)
            {
                if (col == null) continue;
                float dist = Vector2.Distance(_serverPosition, new Vector2(col.transform.position.x, col.transform.position.z));
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = col;
                }
            }

            if (nearest != null)
            {
                bool inRange = minDist <= 0.6f;
                Gizmos.color = inRange ? Color.green : Color.gray;
                Gizmos.DrawLine(serverPos, nearest.transform.position);

                if (inRange)
                {
                    // Highlight when in pickup range
                    Gizmos.color = new Color(0, 1, 0, 0.5f);
                    Gizmos.DrawLine(clientPos, nearest.transform.position);
                }

#if UNITY_EDITOR
                UnityEditor.Handles.color = inRange ? Color.green : Color.red;
                UnityEditor.Handles.Label(nearest.transform.position + Vector3.up * 2f,
                    $"{minDist:F2}m\n{(inRange ? "✅ PICKUP!" : "❌ Too far")}");
#endif
            }
        }

#if UNITY_EDITOR
        Color labelColor = desyncDistance > 0.3f ? Color.red : Color.green;
        UnityEditor.Handles.color = labelColor;
        UnityEditor.Handles.Label(clientPos + Vector3.up * 2.5f,
            $"<b>CLIENT (Visual)</b>\nDesync: {desyncDistance:F2}m");

        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.Label(serverPos + Vector3.up * 3.5f,
            $"<b>SERVER (Authority)</b>\n({_serverPosition.x:F1}, {_serverPosition.y:F1})\nPickup: 0.6m");
#endif
    }

}
