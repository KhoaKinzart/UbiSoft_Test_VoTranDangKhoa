using UnityEngine;
using Game.Simulation.Server;
using Game.Simulation.Pathfinding;
using Game.Core.ServiceLocator;
using Game.Core.Constants;
using Game.Presentation.UI;
using System.Collections;

public class GameBootstrap : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 120f;
    [SerializeField] private int botCount = 20;
    [SerializeField] private int collectibleCount = 10;
    [SerializeField] private bool autoStartGame = true;

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private SimulationController simulationController;
    [SerializeField] private PresentationController presentationController;
    [SerializeField] private GameUIController gameUIController;

    private void Start()
    {
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        bool mapReady = false;
        StartCoroutine(gridManager.GenerateMapRoutine(botCount + 1, () => { mapReady = true; }));

        yield return new WaitUntil(() => mapReady);

        yield return new WaitForSeconds(0.1f);

        InitializeServices();
        InitializeSimulation();
        InitializePresentation();
        
        if (autoStartGame)
        {
            yield return new WaitForSeconds(1f);
            StartGame();
        }
    }

    private void InitializeServices()
    {
        var pathfinding = new AStarPathfindingService(
            gridManager.CurrentWidth,
            gridManager.CurrentHeight
        );
        pathfinding.UpdateGridObstacles(gridManager.GridData);

        ServiceLocator.Register<IPathfindingService>(pathfinding);
    }

    private void InitializeSimulation()
    {
        var pathfinding = ServiceLocator.Get<IPathfindingService>();
        var simulation = new GameSimulation(
            gridManager.GridData,
            gridManager.CurrentWidth,
            gridManager.CurrentHeight,
            pathfinding
        );

        simulation.Initialize(botCount, collectibleCount);
        simulationController.SetSimulation(simulation);
    }

    private void InitializePresentation()
    {
        presentationController.SetSimulation(simulationController.Simulation);
        
        if (gameUIController != null)
        {
            gameUIController.SetSimulation(simulationController.Simulation);
        }
    }

    private void StartGame()
    {
        if (simulationController.Simulation != null)
        {
            simulationController.Simulation.StartGame(gameDuration);
        }
    }
}
