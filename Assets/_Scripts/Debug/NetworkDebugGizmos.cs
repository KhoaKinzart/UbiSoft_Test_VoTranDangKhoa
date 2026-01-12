using UnityEngine;

public class NetworkDebugGizmos : MonoBehaviour
{
    private Vector3 _latestServerPosition;
    private bool _hasData = false;

    public void UpdateServerPosition(Vector2 serverPosGrid)
    {
        _latestServerPosition = new Vector3(serverPosGrid.x, transform.position.y, serverPosGrid.y);
        _hasData = true;
    }

    private void OnDrawGizmos()
    {
        if (!_hasData) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_latestServerPosition, 0.4f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _latestServerPosition);
    }
}