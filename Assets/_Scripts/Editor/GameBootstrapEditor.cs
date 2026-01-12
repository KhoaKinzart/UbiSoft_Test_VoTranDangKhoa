using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(GameBootstrap))]
public class GameBootstrapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GameBootstrap bootstrap = (GameBootstrap)target;
        
        SerializedProperty botCountProp = serializedObject.FindProperty("botCount");
        int botCount = botCountProp != null ? botCountProp.intValue : 20;
        
        int totalPlayers = botCount + 1;
        int totalEggs = totalPlayers * 2;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 12;
        titleStyle.normal.textColor = new Color(0.3f, 0.8f, 0.3f);
        
        EditorGUILayout.LabelField("🥚 Auto-Calculated Collectibles", titleStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"  Total Players:", GUILayout.Width(120));
        EditorGUILayout.LabelField($"{totalPlayers} (1 Player + {botCount} Bots)", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"  Eggs Formula:", GUILayout.Width(120));
        EditorGUILayout.LabelField($"{totalPlayers} × 2", EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        GUIStyle eggStyle = new GUIStyle(EditorStyles.boldLabel);
        eggStyle.normal.textColor = new Color(1f, 0.6f, 0f);
        EditorGUILayout.LabelField($"  Total Eggs:", eggStyle, GUILayout.Width(120));
        EditorGUILayout.LabelField($"{totalEggs}", eggStyle);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
    
        EditorGUILayout.EndVertical();
    }
}
