using UnityEngine;

namespace Game.Core.Constants
{
    public static class GameplayConstants
    {
        public static class Network
        {
            public const int LOCAL_PLAYER_ID = 999;
        }

        public static class Movement
        {
            public const float SPRINT_MULTIPLIER = 1.3f;
            public const float DASH_MULTIPLIER = 2.0f;
            public const float BASE_SPEED = 5f;
            public const float PLAYER_RADIUS = 0.3f;
        }

        public static class Stamina
        {
            public const float MAX_STAMINA = 100f;
            public const float MIN_SPRINT_THRESHOLD = 15f;
            public const float SPRINT_COST = 25f;
            public const float DASH_COST = 30f;
            public const float REGEN_RATE = 20f;
        }

        public static class Collision
        {
            public const float PICKUP_DISTANCE = 0.6f;  
            public const float WAYPOINT_REACH_DISTANCE = 0.1f;
        }
    }
}
