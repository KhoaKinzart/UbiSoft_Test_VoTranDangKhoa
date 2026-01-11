using UnityEngine;

namespace Game.Debugging
{
    public class AgentDebugGizmos : MonoBehaviour
    {
        [Header("Debug Visualization")]
        [SerializeField] private bool showTargetLine = true;
        [SerializeField] private bool showAgentID = true;
        [SerializeField] private Color lineColor = Color.cyan;
        
        private Vector3 _targetPosition;
        private int _agentID;
        private bool _hasTarget;

        public void SetTarget(Vector3 target, int agentID)
        {
            _targetPosition = target;
            _agentID = agentID;
            _hasTarget = true;
        }

        public void ClearTarget()
        {
            _hasTarget = false;
        }

        private void OnDrawGizmos()
        {
            if (!showTargetLine || !_hasTarget) return;

            Gizmos.color = lineColor;
            Gizmos.DrawLine(transform.position, _targetPosition);
            
            Gizmos.DrawWireSphere(_targetPosition, 0.3f);
        }

        private void OnGUI()
        {
            if (!showAgentID) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 0.5f);
            
            if (screenPos.z > 0)
            {
                GUI.color = Color.white;
                GUI.Label(new Rect(screenPos.x - 20, Screen.height - screenPos.y - 10, 40, 20), 
                    $"Bot {_agentID}", 
                    new GUIStyle() 
                    { 
                        fontSize = 10, 
                        alignment = TextAnchor.MiddleCenter,
                        normal = new GUIStyleState() { textColor = Color.cyan }
                    });
            }
        }
    }
}
