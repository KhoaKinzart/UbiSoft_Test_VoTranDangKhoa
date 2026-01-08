using UnityEngine;
using System.Collections;
using System;

public class GridManager : MonoBehaviour
{
    [SerializeField] private MapConfig config; 
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private GameObject obstaclePrefab;

    public int[,] GridData { get; private set; }
    public int CurrentWidth { get; private set; }
    public int CurrentHeight { get; private set; }
    public Vector3 CenterPosition { get; private set; } 

    public IEnumerator GenerateMapRoutine(int playerCount, Action onComplete)
    {
        CalculateSize(playerCount);

        foreach (Transform child in transform) Destroy(child.gameObject);
        yield return null;

        GridData = new int[CurrentWidth, CurrentHeight];
        CreateBigGround();

        int centerX = CurrentWidth / 2;
        int centerZ = CurrentHeight / 2;
        CenterPosition = new Vector3(centerX, 1f, centerZ); 

        int cellsProcessed = 0;
        for (int x = 0; x < CurrentWidth; x++)
        {
            for (int z = 0; z < CurrentHeight; z++)
            {
                ProcessCell(x, z, centerX, centerZ);

                cellsProcessed++;
                if (cellsProcessed >= config.cellsPerFrame)
                {
                    cellsProcessed = 0;
                    yield return null;
                }
            }
        }

        onComplete?.Invoke();
    }

    private void CalculateSize(int playerCount)
    {
        int size = Mathf.RoundToInt(config.baseMapSize + (playerCount * config.expansionPerPlayer));
        if (size % 2 == 0) size++;
        CurrentWidth = size;
        CurrentHeight = size;
    }

    private void CreateBigGround()
    {
        GameObject bigGround = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bigGround.name = "BigGround";
        bigGround.transform.parent = transform;
        bigGround.transform.position = new Vector3((CurrentWidth - 1) / 2f, -0.5f, (CurrentHeight - 1) / 2f);
        bigGround.transform.localScale = new Vector3(CurrentWidth, 1f, CurrentHeight);

        if (groundPrefab != null)
            bigGround.GetComponent<MeshRenderer>().material = groundPrefab.GetComponent<MeshRenderer>().sharedMaterial;
    }

    private void ProcessCell(int x, int z, int cx, int cz)
    {
        bool isEdge = (x == 0 || z == 0 || x == CurrentWidth - 1 || z == CurrentHeight - 1);
        bool isSafe = (Mathf.Abs(x - cx) <= config.safeZoneSize && Mathf.Abs(z - cz) <= config.safeZoneSize);

        if (isEdge) SpawnObstacle(x, z);
        else if (isSafe) GridData[x, z] = 0;
        else if (UnityEngine.Random.value < config.obstacleProbability) SpawnObstacle(x, z);
        else GridData[x, z] = 0;
    }

    private void SpawnObstacle(int x, int z)
    {
        Instantiate(obstaclePrefab, new Vector3(x, 0.5f, z), Quaternion.identity, transform);
        GridData[x, z] = 1;
    }
}