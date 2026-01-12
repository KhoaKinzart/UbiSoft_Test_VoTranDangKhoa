using UnityEngine;

namespace Game.Networking.Snapshot
{
    [System.Serializable]
    public struct EntitySnapshot
    {
        public int EntityID;
        public Vector2 Position;
        public float Stamina;
        public float Timestamp;
    }
}
