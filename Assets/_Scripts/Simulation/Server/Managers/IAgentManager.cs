using System.Collections.Generic;
using Game.Gameplay.Entities;
using Game.Networking.Snapshot;

namespace Game.Simulation.Server.Managers
{
    public interface IAgentManager
    {
        IReadOnlyList<AgentEntity> Agents { get; }
        
        void SpawnAgents(int count);
        void Update(float deltaTime);
        List<EntitySnapshot> GetSnapshots(float timestamp);
    }
}
