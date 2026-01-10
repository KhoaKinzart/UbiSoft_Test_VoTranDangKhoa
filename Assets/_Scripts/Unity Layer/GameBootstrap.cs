using UnityEngine;
using Game.Simulation.Server;
using Game.Simulation.Pathfinding;
using Game.Core.ServiceLocator;
using System.Collections;

public class GameBootstrap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int botCount = 20;
    [SerializeField] private int collectibleCount = 10;

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private SimulationController simulationController;
    [SerializeField] private PresentationController presentationController;

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
    }
}
