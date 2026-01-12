using UnityEngine;

namespace Game.Core
{
    public class GameSettings : MonoBehaviour
    {
        private static GameSettings _instance;
        
        public static GameSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameSettings");
                    _instance = go.AddComponent<GameSettings>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        public int BotCount { get; set; } = 20;
        public float GameDuration { get; set; } = 120f;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void SetGameSettings(int botCount, float gameDuration)
        {
            BotCount = botCount;
            GameDuration = gameDuration;
        }
    }
}
