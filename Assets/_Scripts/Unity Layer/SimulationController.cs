using UnityEngine;
using Game.Simulation.Server;

public class SimulationController : MonoBehaviour
{
    [Header("Simulation Settings")]
    [SerializeField] private float tickRate = 60f;

    public IGameSimulation Simulation { get; private set; }

    private float _tickTimer;
    private float _tickInterval;

    private void Awake()
    {
        _tickInterval = 1f / tickRate;
    }

    public void SetSimulation(IGameSimulation simulation)
    {
        Simulation = simulation;
    }

    private void Update()
    {
        if (Simulation == null) return;

        _tickTimer += Time.deltaTime;

        while (_tickTimer >= _tickInterval)
        {
            Simulation.Tick(_tickInterval);
            _tickTimer -= _tickInterval;
        }
    }

    public int RegisterPlayer(Vector3 worldPosition)
    {
        return Simulation?.RegisterPlayer(worldPosition) ?? -1;
    }

    public void SendPlayerInput(int playerId, Vector2 input, bool sprint, bool dash = false)
    {
        Simulation?.SendPlayerInput(playerId, input, sprint, dash);
    }
}
