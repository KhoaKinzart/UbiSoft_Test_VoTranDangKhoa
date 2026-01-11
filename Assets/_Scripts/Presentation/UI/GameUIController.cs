using UnityEngine;
using System.Collections.Generic;

namespace Game.Presentation.UI
{
    public class GameUIController : MonoBehaviour
    {
        [SerializeField] private GameHUD gameHUD;
        
        private Simulation.Server.IGameSimulation _simulation;
        private readonly Dictionary<int, int> _cachedScores = new Dictionary<int, int>();

        public void SetSimulation(Simulation.Server.IGameSimulation simulation)
        {
            _simulation = simulation;
        }

        private void Update()
        {
            if (_simulation == null || gameHUD == null)
                return;

            if (_simulation.IsGameOver())
                return;

            UpdateUI();
        }

        private void UpdateUI()
        {
            gameHUD.UpdateTimer(_simulation.GetTimeRemaining());
            
            var player = _simulation.GetLocalPlayer();
            if (player != null)
            {
                int playerScore = _simulation.GetScore(player.ID);
                gameHUD.UpdatePlayerScore(playerScore);
                _cachedScores[player.ID] = playerScore;
            }

            var agents = _simulation.GetAgents();
            if (agents != null)
            {
                foreach (var agent in agents)
                {
                    int agentScore = _simulation.GetScore(agent.ID);
                    _cachedScores[agent.ID] = agentScore;
                }
            }

            gameHUD.UpdateLeaderboard(_cachedScores);
        }
    }
}
