// File: Assets/_Scripts/Server/Strategies/IBotMovement.cs
namespace Server.Strategies
{
    public interface IBotMovement
    {
        // Hàm này nhận vào BotEntity để thao tác dữ liệu trên đó
        void UpdateMovement(BotEntity bot, float baseSpeed, float deltaTime);
    }
}