using UnityEngine;

// Dữ liệu Input từ Client gửi lên Server
[System.Serializable]
public struct PlayerInputPacket
{
    public Vector2 MovementInput; // Vector (x, y) từ bàn phím
    public bool IsDashing;        // Trạng thái Dash (nếu có)
}

// Dữ liệu Server trả về cho Player
[System.Serializable]
public struct PlayerSnapshot
{
    public Vector2 Position;
    public float Stamina;
    public float Timestamp;
}

[System.Serializable]
public struct BotSnapshot
{
    public int BotID;
    public Vector2 Position;
    public float Stamina;
    public float Timestamp; 
}