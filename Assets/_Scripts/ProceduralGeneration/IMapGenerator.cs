namespace Game.ProceduralGeneration
{
    public interface IMapGenerator
    {
        int[,] Generate(int width, int height, float obstacleDensity);
        bool ValidateConnectivity(int[,] grid, int width, int height);
    }
}
