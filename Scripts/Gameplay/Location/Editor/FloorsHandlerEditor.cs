using UnityEditor;
using UnityEngine;
using ColorUtility = PlayVibe.ColorUtility;

namespace Gameplay.Network
{
    [CustomEditor(typeof(FloorsHandler))]
    public class FloorsHandlerEditor : Editor
    {
        private FloorsHandler view;

        private readonly string enableColor = "#48f050";
        private readonly string disableColor = "#f04848";
        
        private void OnEnable()
        {
            view = target as FloorsHandler;
        }

        public override void OnInspectorGUI()
        {
            if (view != null)
            {
                if (GUILayout.Button($"Enable All"))
                {
                    view.SetForAll(true);
                }
                
                if (GUILayout.Button($"Disable All"))
                {
                    view.SetForAll(false);
                }
                
                EditorGUILayout.BeginHorizontal();

                for (var i = 0; i < view.Count; i++)
                {
                    var color = ColorUtility.HexToColor(view.ActiveFloor == i || view.IsAllActive ? enableColor : disableColor);
                    var originalBackgroundColor = GUI.backgroundColor;

                    GUI.backgroundColor = color;

                    var buttonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fixedWidth = 35f,
                        fixedHeight = 35f
                    };

                    if (GUILayout.Button($"{i}", buttonStyle))
                    {
                        view.SetFloor(i);
                    }

                    GUI.backgroundColor = originalBackgroundColor;
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            base.OnInspectorGUI();
        }
    }
}