using Game.Gameplay.Entities;

namespace Game.Simulation.Movement
{
    public interface IMovementStrategy
    {
        void Execute(AgentEntity agent, float deltaTime);
    }
}
