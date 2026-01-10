using UnityEngine;
using System.Collections;
using System;
using Game.ProceduralGeneration;

public class GridManager : MonoBehaviour
{
    [SerializeField] private MapConfig config;
    [SerializeField] private GameObject groundPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    
    [Header("Map Generation")]
    [SerializeField] private MapGenerationType generationType = MapGenerationType.ConnectedRandom;

    public int[,] GridData { get; private set; }
    public int CurrentWidth { get; private set; }
    public int CurrentHeight { get; private set; }
    public Vector3 CenterPosition { get; private set; }
    public bool IsMapReady { get; private set; } = false;

    private IMapGenerator _mapGenerator;

    private void Awake()
    {
        SetupGenerator();
    }

    private void SetupGenerator()
    {
        switch (generationType)
        {
            case MapGenerationType.ConnectedRandom:
                _mapGenerator = new ConnectedMapGenerator();
                break;
            case MapGenerationType.CellularAutomata:
                _mapGenerator = new CellularAutomataGenerator();
                break;
            default:
                _mapGenerator = new ConnectedMapGenerator();
                break;
        }
    }

    public IEnumerator GenerateMapRoutine(int playerCount, Action onComplete)
    {
        IsMapReady = false;
        CalculateSize(playerCount);

        foreach (Transform child in transform) Destroy(child.gameObject);
        yield return null;

        GridData = _mapGenerator.Generate(CurrentWidth, CurrentHeight, config.obstacleProbability);
        CreateBigGround();

        int centerX = CurrentWidth / 2;
        int centerZ = CurrentHeight / 2;
        CenterPosition = new Vector3(centerX, 1f, centerZ);

        int obstacleCount = 0;
        int cellsProcessed = 0;
        for (int x = 0; x < CurrentWidth; x++)
        {
            for (int z = 0; z < CurrentHeight; z++)
            {
                if (GridData[x, z] == 1)
                {
                    SpawnObstacle(x, z);
                    obstacleCount++;
                }

                cellsProcessed++;
                if (cellsProcessed >= config.cellsPerFrame)
                {
                    cellsProcessed = 0;
                    yield return null;
                }
            }
        }
        
        IsMapReady = true;
        
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

    private void SpawnObstacle(int x, int z)
    {
        Instantiate(obstaclePrefab, new Vector3(x, 0.5f, z), Quaternion.identity, transform);
    }
}

public enum MapGenerationType
{
    ConnectedRandom,
    CellularAutomata
}