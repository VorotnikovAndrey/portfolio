using System.Collections;
using UnityEditor;
using UnityEngine;

namespace PlayVibe
{
    [InitializeOnLoad]
    public class HColorEditor
    {
        static HColorEditor()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= HighlightOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += HighlightOnGUI;
        }

        private static void HighlightOnGUI(int id, Rect rect)
        {
            var go = EditorUtility.InstanceIDToObject(id) as GameObject;

            if (go != null)
            {
                var hColor = go.GetComponent<HColor>();

                if (hColor != null && Event.current.type == EventType.Repaint)
                {
                    var isSelected = ((IList) Selection.instanceIDs).Contains(id);
                    var backgroundColor = hColor.HColorData.BackgroundColor;
                    var textColor = hColor.HColorData.TextColor;
                    var fontStyle = hColor.HColorData.FontStyle;
                    var offset = new Rect(rect.position + new Vector2(EditorGUIUtility.singleLineHeight, 0f), rect.size);

                    if (backgroundColor.a > 0f)
                    {
                        var backgroundOffset = new Rect(rect.position + new Vector2(EditorGUIUtility.singleLineHeight, 0f), rect.size);

                        if (hColor.HColorData.BackgroundColor.a < 1f || isSelected)
                        {
                            EditorGUI.DrawRect(backgroundOffset, HColorData.DefaultBackgroundColor);
                        }

                        if (isSelected)
                        {
                            EditorGUI.DrawRect(backgroundOffset, Color.Lerp(GUI.skin.settings.selectionColor, backgroundColor, 0.3f));
                        }
                        else
                        {
                            EditorGUI.DrawRect(backgroundOffset, backgroundColor);
                        }
                    }

                    EditorGUI.LabelField(offset, go.name, new GUIStyle()
                    {
                        normal = new GUIStyleState() {textColor = textColor},
                        fontStyle = fontStyle
                    });

                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }

    }
}
