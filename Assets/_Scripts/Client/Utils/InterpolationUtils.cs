// File: Assets/_Scripts/Client/Utils/InterpolationUtils.cs
using System.Collections.Generic;
using UnityEngine;

namespace Client.Utils
{
    public static class InterpolationUtils
    {
        public static bool CalculateInterpolation(
            List<BotSnapshot> history,
            float renderTime,
            out Vector2 resultPos,
            out float resultStamina)
        {
            resultPos = Vector2.zero;
            resultStamina = 100f;

            if (history.Count == 0) return false;

            if (history[0].Timestamp > renderTime)
            {
                resultPos = history[0].Position;
                resultStamina = history[0].Stamina;
                return true;
            }

            BotSnapshot snapA = history[0];
            BotSnapshot snapB = history[0];

            for (int i = history.Count - 1; i >= 0; i--)
            {
                if (history[i].Timestamp <= renderTime)
                {
                    snapA = history[i];
                    if (i + 1 < history.Count) snapB = history[i + 1];
                    break;
                }
            }

            if (Vector2.Distance(snapA.Position, snapB.Position) > 10.0f)
            {
                resultPos = snapB.Position; 
                resultStamina = snapB.Stamina;
            }
            else
            {
                float timeWindow = snapB.Timestamp - snapA.Timestamp;
                float t = 0;
                if (timeWindow > 0.0001f)
                {
                    t = (renderTime - snapA.Timestamp) / timeWindow;
                }

                resultPos = Vector2.Lerp(snapA.Position, snapB.Position, t);
                resultStamina = Mathf.Lerp(snapA.Stamina, snapB.Stamina, t);
            }

            return true;
        }
    }
}