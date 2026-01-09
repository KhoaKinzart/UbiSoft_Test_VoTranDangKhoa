using UnityEngine;

public class EggEntity
{
    public int ID { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    public EggEntity(int id, Vector2Int pos)
    {
        ID = id;
        GridPosition = pos;
    }
}