
namespace Server.Strategies
{
    public interface IBotMovement
    {
        void UpdateMovement(BotEntity bot, float baseSpeed, float deltaTime);
    }
}