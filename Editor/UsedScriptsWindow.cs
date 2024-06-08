using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UsedScriptsWindow : EditorWindow
{
    private Dictionary<string, List<GameObject>> scriptToGameObjects;
    private Vector2 scrollPosition;
    private Dictionary<string, bool> foldoutStates;

    // List of script names or types to skip
    private List<string> scriptsToSkip = new List<string>
    {
        "Transform",
        "Camera",
        "Light",
        "CanvasScaler"
        // Add other built-in or specific script names/types to skip
    };

    [MenuItem("Window/Used Scripts")]
    public static void ShowWindow()
    {
        GetWindow<UsedScriptsWindow>("Used Scripts");
    }

    private void OnEnable()
    {
        RefreshScriptsList();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            RefreshScriptsList();
        }

        if (scriptToGameObjects == null || scriptToGameObjects.Count == 0)
        {
            GUILayout.Label("No scripts found in the scene.");
            return;
        }

        GUILayout.Label("Scripts used in the current scene:", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 50));

        foreach (var entry in scriptToGameObjects)
        {
            // Initialize foldout state if not already present
            if (!foldoutStates.ContainsKey(entry.Key))
            {
                foldoutStates[entry.Key] = false;
            }

            // Draw the foldout
            foldoutStates[entry.Key] = EditorGUILayout.Foldout(foldoutStates[entry.Key], entry.Key, true);

            // If foldout is expanded, show the associated GameObjects
            if (foldoutStates[entry.Key])
            {
                EditorGUI.indentLevel++;
                float availableWidth = position.width - 30; // 30 for scrollbar and margins
                float minButtonWidth = 100;
                float spacing = 10;
                int buttonsPerRow = Mathf.Max(1, Mathf.FloorToInt((availableWidth + spacing) / (minButtonWidth + spacing)));

                int buttonCount = entry.Value.Count;
                for (int i = 0; i < buttonCount; i += buttonsPerRow)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = 0; j < buttonsPerRow && i + j < buttonCount; j++)
                    {
                        var go = entry.Value[i + j];
                        if (GUILayout.Button(go.name, GUILayout.Width((availableWidth - spacing * (buttonsPerRow - 1)) / buttonsPerRow)))
                        {
                            SelectGameObject(go);
                        }
                        if (j < buttonsPerRow - 1)
                        {
                            GUILayout.Space(spacing);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void RefreshScriptsList()
    {
        scriptToGameObjects = new Dictionary<string, List<GameObject>>();
        foldoutStates = new Dictionary<string, bool>();
        MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>();

        foreach (var script in allScripts)
        {
            string scriptName = script.GetType().Name;

            if (!scriptsToSkip.Contains(scriptName))
            {
                if (!scriptToGameObjects.ContainsKey(scriptName))
                {
                    scriptToGameObjects[scriptName] = new List<GameObject>();
                }
                scriptToGameObjects[scriptName].Add(script.gameObject);
            }
        }

        // Sort the dictionary by script names
        scriptToGameObjects = scriptToGameObjects.OrderBy(entry => entry.Key).ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    private void SelectGameObject(GameObject go)
    {
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);
    }
}
