using System.Linq;
using PlayVibe;
using UnityEditor;
using UnityEngine;

namespace Gameplay.Network
{
    [CustomEditor(typeof(ViewsHandler))]
    public class LevelDataEditor : Editor
    {
        private ViewsHandler viewsHandler;

        private void OnEnable()
        {
            viewsHandler = target as ViewsHandler;
        }

        public override void OnInspectorGUI()
        {
            if (viewsHandler != null)
            {
                if (GUILayout.Button("Find and validate", GUILayout.Height(35f)))
                {
                    viewsHandler.MapItemBoxes = FindObjectsOfType<MapItemboxInteractiveObject>().ToList();
                    viewsHandler.Recyclers = FindObjectsOfType<RecyclerInteractiveObject>().ToList();
                    viewsHandler.SecurityObservers = FindObjectsOfType<SecurityObserver>().ToList();

                    for (var i = 0; i < viewsHandler.MapItemBoxes.Count; i++)
                    {
                        viewsHandler.MapItemBoxes[i].SetNetworkKey(i);
                        EditorUtility.SetDirty(viewsHandler.MapItemBoxes[i]);
                    }
                    
                    for (var i = 0; i < viewsHandler.Recyclers.Count; i++)
                    {
                        viewsHandler.Recyclers[i].SetNetworkKey(i);
                        EditorUtility.SetDirty(viewsHandler.Recyclers[i]);
                    }
                    
                    EditorUtility.SetDirty(viewsHandler);
                }
            }
            
            base.OnInspectorGUI();
        }
    }
}