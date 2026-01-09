using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private ServerManager serverManager;

    void Update()
    {
        // 1. Đọc Input từ bàn phím
        float h = Input.GetAxisRaw("Horizontal");
        if (h != 0) Debug.Log("Có nhận phím bấm: " + h);
        float v = Input.GetAxisRaw("Vertical");
        bool isSprint = Input.GetKey(KeyCode.LeftShift);
        bool isDash = Input.GetKeyDown(KeyCode.Space);

        // 2. Đóng gói
        PlayerInputPacket packet = new PlayerInputPacket
        {
            MovementInput = new Vector2(h, v).normalized,
            IsSprint = isSprint,
            IsDash = isDash
        };

        // 3. Gửi lên Server
        // Trong thực tế sẽ qua mạng, ở đây gọi hàm trực tiếp giả lập
        if (serverManager != null)
        {
            serverManager.ReceivePlayerInput(packet);
        }
    }
}