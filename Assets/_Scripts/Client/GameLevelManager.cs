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
        // Gọi GridManager chạy và đợi nó xong (callback)
        bool isMapReady = false;
        StartCoroutine(gridManager.GenerateMapRoutine(playerCount, () => { isMapReady = true; }));

        // Đợi biến isMapReady thành true
        yield return new WaitUntil(() => isMapReady);

        Debug.Log("2. Spawning Player...");
        GameObject player = SpawnPlayer();

        Debug.Log("3. Setting up Camera...");
        SetupCamera(player);
    }

    private GameObject SpawnPlayer()
    {
        // Lấy vị trí tâm từ GridManager
        Vector3 spawnPos = gridManager.CenterPosition;
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player.name = "LocalPlayer";
        return player;
    }

    private void SetupCamera(GameObject target)
    {
        if (cameraFollow != null)
        {
            // Reset vị trí Camera để tránh giật
            Camera.main.transform.position = target.transform.position + new Vector3(0, 10, -10);
            Camera.main.orthographicSize = 8;

            // Gán mục tiêu
            cameraFollow.SetTarget(target.transform);
        }
    }
}