using UnityEngine;

[System.Serializable]
public struct PlayerInputPacket
{
    public Vector2 MovementInput; // Vector hướng di chuyển (x, y)
    public bool IsSprint;         // Có giữ phím Shift không?
    public bool IsDash;           // Có bấm phím Space không?
}