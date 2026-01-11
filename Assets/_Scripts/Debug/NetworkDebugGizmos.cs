using UnityEngine;

public class NetworkDebugGizmos : MonoBehaviour
{
    private Vector3 _latestServerPosition;
    private bool _hasData = false;

    // Hàm này sẽ được PresentationController gọi khi nhận gói tin mới
    public void UpdateServerPosition(Vector2 serverPosGrid)
    {
        // Chuyển đổi toạ độ Grid sang World (giả sử y = 0.5f hoặc y bot hiện tại)
        _latestServerPosition = new Vector3(serverPosGrid.x, transform.position.y, serverPosGrid.y);
        _hasData = true;
    }

    private void OnDrawGizmos()
    {
        if (!_hasData) return;

        // 1. Vẽ vị trí thực của Client (Cái bạn nhìn thấy) - MÀU XANH LÁ
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);

        // 2. Vẽ vị trí Server gửi về (Cái nhận được sau độ trễ) - MÀU ĐỎ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_latestServerPosition, 0.4f);

        // 3. Vẽ đường nối để thấy độ trễ
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _latestServerPosition);
    }
}