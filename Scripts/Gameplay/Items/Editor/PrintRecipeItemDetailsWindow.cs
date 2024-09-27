using Gameplay;
using Gameplay.Items;
using UnityEditor;
using UnityEngine;

namespace Services.Gameplay.Craft
{
    public class PrintRecipeItemDetailsWindow : EditorWindow
    {
        private static PrintRecipeBank bank;
        private static PrintRecipeBank.PrintRecipePair pair;
        private static bool isComponent;
        private static int index;
        
        private readonly Vector2 iconSize = new(100, 100);
        private readonly float iconSpacing = 10f;
        private readonly int maxColumns = 6;
        
        private Vector2 scrollPosition;
        private float windowWidth;

        public static void ShowWindow(PrintRecipeBank bank, PrintRecipeBank.PrintRecipePair pair, bool isComponent, int index, out PrintRecipeItemDetailsWindow windowRef)
        {
            PrintRecipeItemDetailsWindow.bank = bank;
            PrintRecipeItemDetailsWindow.pair = pair;
            PrintRecipeItemDetailsWindow.isComponent = isComponent;
            PrintRecipeItemDetailsWindow.index = index;

            var window = GetWindow<PrintRecipeItemDetailsWindow>();
            window.titleContent = new GUIContent("Items");
            window.minSize = new Vector2(400, 300);
            window.maxSize = new Vector2(600, 800);
            window.Show();

            windowRef = window;
        }

        public void CloseWindow()
        {
            Close();
        }

        private void OnGUI()
        {
            CalculateWindowWidth();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (bank.ItemsSettings != null)
            {
                GUILayout.BeginVertical();

                var columnCount = 0;
                var i = 0;

                foreach (var element in bank.ItemsSettings.Data)
                {
                    var icon = bank.ItemsSettings.Data[element.Key].Icon;

                    if (icon == null)
                    {
                        continue;
                    }
                    
                    if (columnCount == 0)
                    {
                        GUILayout.BeginHorizontal();
                    }

                    if (i == 0 && columnCount == 0)
                    {
                        var largerTextButtonStyle = new GUIStyle(GUI.skin.button)
                        {
                            fontSize = 18
                            
                        };
                        if (GUILayout.Button("None", largerTextButtonStyle, GUILayout.Width(iconSize.x), GUILayout.Height(iconSize.y)))
                        {
                            if (!isComponent)
                            {
                                pair.ItemKey = null;
                            }
                            else
                            {
                                pair.ComponentsKeys[index] = null;
                            }
                    
                            EditorUtility.SetDirty(bank);

                            CloseWindow();
                        }
                        
                        GUILayout.Space(iconSpacing);

                        columnCount++;
                    }

                    i++;

                    if (GUILayout.Button(icon.texture, GUILayout.Width(iconSize.x), GUILayout.Height(iconSize.y)))
                    {
                        if (!isComponent)
                        {
                            pair.ItemKey = element.Key;
                        }
                        else
                        {
                            pair.ComponentsKeys[index] = element.Key;
                        }
                        
                        EditorUtility.SetDirty(bank);

                        CloseWindow();
                    }

                    columnCount++;

                    if (columnCount >= maxColumns)
                    {
                        GUILayout.EndHorizontal();
                        columnCount = 0;
                    }
                    else
                    {
                        GUILayout.Space(iconSpacing);
                    }
                }

                if (columnCount > 0)
                {
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField("No items to display.");
            }

            EditorGUILayout.EndScrollView();
        }

        private void CalculateWindowWidth()
        {
            windowWidth = maxColumns * (iconSize.x + iconSpacing) + iconSpacing * 2;
            minSize = new Vector2(windowWidth, minSize.y);
            maxSize = new Vector2(windowWidth, maxSize.y);
        }
    }
}
