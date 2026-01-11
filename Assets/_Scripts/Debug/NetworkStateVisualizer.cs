using UnityEngine;
using Game.Simulation.Server;

namespace Game.Debug
{
    public class NetworkStateVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SimulationController simulationController;
        
        [Header("Visualization Settings")]
        [SerializeField] private bool showServerState = true;
        [SerializeField] private bool showClientState = true;
        [SerializeField] private bool showConnectionLine = true;
        [SerializeField] private float gizmoRadius = 0.5f;
        
        [Header("Colors")]
        [SerializeField] private Color serverColor = new Color(1f, 0.3f, 0.3f, 0.9f);
        [SerializeField] private Color clientColor = new Color(0.3f, 1f, 0.3f, 0.9f);
        [SerializeField] private Color lineColor = new Color(1f, 1f, 0f, 0.6f);
        
        private Vector3 _serverPosition;
        private Vector3 _clientPosition;
        private int _entityId = -1;
        
        private Color _actualServerColor;
        private Color _actualClientColor;
        
        private void Start()
        {
            if (simulationController == null)
            {
                simulationController = FindObjectOfType<SimulationController>();
            }
            
            string objName = gameObject.name;
            if (objName.Contains("Bot_"))
            {
                string idStr = objName.Replace("Bot_", "");
                if (int.TryParse(idStr, out int id))
                {
                    _entityId = id;
                }
                
                _actualServerColor = Color.black;
                _actualClientColor = Color.red;
            }
            else if (objName == "LocalPlayer")
            {
                _entityId = 999;
                
                _actualServerColor = Color.white;
                _actualClientColor = Color.blue;
            }
        }
        
        private void LateUpdate()
        {
            _clientPosition = transform.position;
            
            if (simulationController != null && simulationController.Simulation != null)
            {
                if (_entityId == 999)
                {
                    var player = simulationController.Simulation.GetLocalPlayer();
                    if (player != null)
                    {
                        _serverPosition = new Vector3(player.Position.x, transform.position.y, player.Position.y);
                    }
                }
                else if (_entityId >= 0)
                {
                    var agents = simulationController.Simulation.GetAgents();
                    foreach (var agent in agents)
                    {
                        if (agent.ID == _entityId)
                        {
                            _serverPosition = new Vector3(agent.Position.x, transform.position.y, agent.Position.y);
                            break;
                        }
                    }
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (showServerState)
            {
                Gizmos.color = _actualServerColor;
                Gizmos.DrawWireSphere(_serverPosition, gizmoRadius);
                
                Gizmos.color = new Color(_actualServerColor.r, _actualServerColor.g, _actualServerColor.b, 0.3f);
                Gizmos.DrawLine(_serverPosition, _serverPosition + Vector3.up * 2f);
                
#if UNITY_EDITOR
                UnityEditor.Handles.color = _actualServerColor;
                UnityEditor.Handles.Label(_serverPosition + Vector3.up * 2.2f, "SERVER");
#endif
            }
            
            if (showClientState)
            {
                Gizmos.color = _actualClientColor;
                Gizmos.DrawSphere(_clientPosition, gizmoRadius * 0.7f);
                
#if UNITY_EDITOR
                UnityEditor.Handles.color = _actualClientColor;
                UnityEditor.Handles.Label(_clientPosition + Vector3.up * 1.8f, "CLIENT");
#endif
            }
            
            if (showConnectionLine && showServerState && showClientState)
            {
                Gizmos.color = lineColor;
                Gizmos.DrawLine(_serverPosition, _clientPosition);
                
                float distance = Vector3.Distance(_serverPosition, _clientPosition);
                Vector3 midPoint = (_serverPosition + _clientPosition) / 2f;
                
#if UNITY_EDITOR
                UnityEditor.Handles.color = lineColor;
                UnityEditor.Handles.Label(midPoint + Vector3.up * 0.5f, $"Δ {distance:F3}m");
#endif
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (simulationController == null) return;
            
            Gizmos.color = Color.cyan;
            float latency = simulationController.GetCurrentLatency();
            
#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, 
                $"Latency: {latency * 1000f:F0}ms\nEntity ID: {_entityId}");
#endif
        }
    }
}
