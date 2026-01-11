using UnityEngine;
using Game.Core.Events;
using Game.Core.Constants;

namespace Game.Gameplay.Systems
{
    public enum GameState
    {
        WaitingToStart,
        Playing,
        GameOver
    }

    public interface IGameStateManager
    {
        GameState CurrentState { get; }
        void StartGame(float gameDuration);
        void EndGame();
        void Update(float deltaTime);
    }

    public class GameStateManager : IGameStateManager
    {
        private readonly IGameTimer _timer;
        private readonly IScoreTracker _scoreTracker;

        public GameState CurrentState { get; private set; }

        public GameStateManager(IGameTimer timer, IScoreTracker scoreTracker)
        {
            _timer = timer;
            _scoreTracker = scoreTracker;
            CurrentState = GameState.WaitingToStart;
            
            EventBus.Subscribe<GameTimerFinishedEvent>(OnTimerFinished);
        }

        ~GameStateManager()
        {
            EventBus.Unsubscribe<GameTimerFinishedEvent>(OnTimerFinished);
        }

        public void StartGame(float gameDuration)
        {
            if (CurrentState != GameState.WaitingToStart)
                return;

            CurrentState = GameState.Playing;
            _timer.StartTimer(gameDuration);
            _scoreTracker.ResetAllScores();
        }

        public void EndGame()
        {
            if (CurrentState != GameState.Playing)
                return;

            CurrentState = GameState.GameOver;
            _timer.StopTimer();
            DetermineWinner();
        }

        public void Update(float deltaTime)
        {
            if (CurrentState == GameState.Playing)
            {
                _timer.Update(deltaTime);
            }
        }

        private void OnTimerFinished(GameTimerFinishedEvent evt)
        {
            EndGame();
        }

        private void DetermineWinner()
        {
            var rankedScores = _scoreTracker.GetRankedScores();
            
            if (rankedScores.Length > 0)
            {
                var winner = rankedScores[0];
                bool isPlayer = winner.entityId == GameplayConstants.Network.LOCAL_PLAYER_ID;
                
                EventBus.Publish(new GameOverEvent
                {
                    WinnerID = winner.entityId,
                    WinnerScore = winner.score,
                    IsPlayer = isPlayer
                });
            }
        }
    }
}
