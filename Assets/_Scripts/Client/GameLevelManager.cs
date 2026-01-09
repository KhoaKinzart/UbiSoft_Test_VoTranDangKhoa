using UnityEngine;
using System.Collections;

public class GameLevelManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Game Data")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private int playerCount = 100;

    void Start()
    {
        StartCoroutine(InitializeGameLevel());
    }

    private IEnumerator InitializeGameLevel()
    {
        Debug.Log("1. Generating Map...");
        bool isMapReady = false;
        StartCoroutine(gridManager.GenerateMapRoutine(playerCount, () => { isMapReady = true; }));

        yield return new WaitUntil(() => isMapReady);

        Debug.Log("2. Spawning Player...");
        GameObject player = SpawnPlayer();

        Debug.Log("3. Setting up Camera...");
        SetupCamera(player);
    }

    private GameObject SpawnPlayer()
    {
        Vector3 spawnPos = gridManager.CenterPosition;
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player.name = "LocalPlayer";
        return player;
    }

    private void SetupCamera(GameObject target)
    {
        if (cameraFollow != null)
        {
            Camera.main.transform.position = target.transform.position + new Vector3(0, 10, -10);
            Camera.main.orthographicSize = 8;

            cameraFollow.SetTarget(target.transform);
        }
    }
}