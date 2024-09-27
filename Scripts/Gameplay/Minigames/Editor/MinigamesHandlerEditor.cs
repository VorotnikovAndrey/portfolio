using System.Linq;
using PlayVibe;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Player.Minigames.Editor
{
    [CustomEditor(typeof(MinigamesHandler))]
    public class MinigamesHandlerEditor : UnityEditor.Editor
    {
        private MinigamesHandler handler;

        private void OnEnable()
        {
            handler = target as MinigamesHandler;
        }

        public override void OnInspectorGUI()
        {
            if (handler != null)
            {
                if (GUILayout.Button("Find and validate all Minigames", GUILayout.Height(35f)))
                {
                    var array = FindObjectsOfType<MinigameInteractiveObject>().ToList();

                    handler.SetArray(array);

                    for (var i = 0; i < array.Count; i++)
                    {
                        var element = array[i];
                        
                        element.SetId(i);
                        
                        EditorUtility.SetDirty(array[i]);
                    }
                    
                    EditorUtility.SetDirty(handler);
                }
            }
            
            base.OnInspectorGUI();
        }
    }
}