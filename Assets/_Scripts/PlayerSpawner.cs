using UnityEngine;
using System.Collections;
using Client.Utils;

public class PlayerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private SimulationController simulationController;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Player Setup")]
    [SerializeField] private GameObject playerPrefab;

    private GameObject _localPlayer;

    private void Start()
    {
        StartCoroutine(SpawnPlayerWhenReady());
    }

    private IEnumerator SpawnPlayerWhenReady()
    {
        while (gridManager == null || !gridManager.IsMapReady)
        {
            yield return null;
        }

        while (simulationController == null || simulationController.Simulation == null)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        Vector3 spawnPos = gridManager.CenterPosition;
        _localPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        _localPlayer.name = "LocalPlayer";

        CharacterController controller = _localPlayer.GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = _localPlayer.AddComponent<CharacterController>();
            controller.center = new Vector3(0, 1, 0);
            controller.radius = 0.3f;
            controller.height = 2f;
        }

        PlayerInput input = _localPlayer.AddComponent<PlayerInput>();
        input.Initialize(simulationController);

        BotVisual visual = _localPlayer.GetComponent<BotVisual>();
        if (visual == null)
        {
            visual = _localPlayer.AddComponent<BotVisual>();
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(_localPlayer.transform);
        }
    }
}
