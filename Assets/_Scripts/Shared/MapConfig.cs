using UnityEngine;

[CreateAssetMenu(fileName = "NewMapConfig", menuName = "Game/Map Config")]
public class MapConfig : ScriptableObject
{
    [Header("Size Settings")]
    public int baseMapSize = 15;
    public float expansionPerPlayer = 1.5f;

    [Header("Generation Rules")]
    [Range(0f, 1f)] public float obstacleProbability = 0.2f;
    public int safeZoneSize = 3;
    public int cellsPerFrame = 2000; 
}