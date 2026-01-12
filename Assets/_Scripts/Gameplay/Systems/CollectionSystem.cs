using System.Collections.Generic;
using UnityEngine;
using Game.Gameplay.Entities;
using Game.Core.Interfaces;
using Game.Core.Constants;

namespace Game.Gameplay.Systems
{
    public class CollectionSystem
    {
        public void CheckCollections(IEntity collector, List<CollectibleEntity> collectibles)
        {
            if (collector == null || collectibles == null)
                return;

            for (int i = collectibles.Count - 1; i >= 0; i--)
            {
                CollectibleEntity collectible = collectibles[i];
                
                if (collectible == null)
                {
                    collectibles.RemoveAt(i);
                    continue;
                }

                if (IsInPickupRange(collector.Position, collectible.Position))
                {
                    collectible.OnCollected(collector);
                    collectibles.RemoveAt(i);
                    break;
                }
            }
        }

        private bool IsInPickupRange(Vector2 collectorPos, Vector2 collectiblePos)
        {
            float distance = Vector2.Distance(collectorPos, collectiblePos);
            return distance <= GameplayConstants.Collision.PICKUP_DISTANCE;
        }
    }
}
