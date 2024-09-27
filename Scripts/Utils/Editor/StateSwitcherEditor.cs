using System.Text;
using UnityEditor;
using UnityEngine;

namespace PlayVibe
{
    [CustomEditor(typeof(StateSwitcher))]
    public class StateSwitcherEditor : Editor
    {
        private const string enableHexColor = "#048143";
        private const string disableHexColor = "#990000";
        
        private StateSwitcher stateSwitcher;
        
        private void OnEnable()
        {
            stateSwitcher = target as StateSwitcher;
        }

        public override void OnInspectorGUI()
        {
            if (stateSwitcher == null)
            {
                base.OnInspectorGUI();
                return;
            }
            
            EditorGUILayout.HelpBox(GetHelpMessage(), MessageType.Info);
            
            GUIStyle enableButtonStyle = CreateColoredButtonStyle(ColorUtility.HexToColor(enableHexColor));
            enableButtonStyle.fontStyle = FontStyle.Bold;
            
            GUIStyle disableButtonStyle = CreateColoredButtonStyle(ColorUtility.HexToColor(disableHexColor));
            disableButtonStyle.fontStyle = FontStyle.Bold;
            
            GUIStyle textStyle = new GUIStyle(EditorStyles.label);
            textStyle.normal.textColor = Color.yellow;
            textStyle.fontStyle = FontStyle.Bold;

            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button($"Set default key OnAwake - {stateSwitcher.UseStateOnAwake}",
                    stateSwitcher.UseStateOnAwake ? enableButtonStyle : disableButtonStyle,GUILayout.Height(20)))
            {
                stateSwitcher.UseStateOnAwake = !stateSwitcher.UseStateOnAwake;
            }
            
            EditorGUILayout.Space(5);

            if (stateSwitcher.UseStateOnAwake)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Default key:", textStyle, GUILayout.Width(80));
                stateSwitcher.DefaultKey = EditorGUILayout.TextField(stateSwitcher.DefaultKey);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            
            base.OnInspectorGUI();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
        
        private GUIStyle CreateColoredButtonStyle(Color backgroundColor)
        {
            GUIStyle style = new GUIStyle(GUI.skin.button);
            style.normal.background = MakeTex(2, 2, backgroundColor);
            return style;
        }
        
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        
        private string GetHelpMessage()
        {
            var sb = new StringBuilder();
            sb.AppendLine("This script allows external control of the state of multiple objects.");
            sb.AppendLine("Its operation is as follows: the script receives a key or index, then compares each element with the current value.");
            sb.AppendLine("If there is a match, the activation event is triggered; otherwise, deactivation occurs.");
            return sb.ToString();
        }
    }
}
