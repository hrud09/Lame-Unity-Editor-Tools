using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UsedScriptsWindow : EditorWindow
{
    private Dictionary<string, List<GameObject>> scriptToGameObjects;
    private Vector2 scrollPosition;

    // Configuration variables
    private const float SCRIPT_NAME_WIDTH = 180f; // Width for script name label
    private const float BUTTON_WIDTH = 100f; // Width for each GameObject button
    private const float BUTTON_SPACING = 2f; // Space between GameObject buttons
    private const float TITLE_WIDTH = 180f; // Width for title labels
    private const float SCROLLVIEW_MARGIN = 50f; // Margin at the bottom of the scroll view
    private const float BORDER_PADDING = 12f;

    // Text input for skipped scripts
    private string skippedScriptsInput;

    // Key for saving the skipped scripts in EditorPrefs
    private const string SKIPPED_SCRIPTS_PREF_KEY = "UsedScriptsWindow_SkippedScripts";

    [MenuItem("Window/Used Scripts")]
    public static void ShowWindow()
    {
        GetWindow<UsedScriptsWindow>("Used Scripts");
    }

    private void OnEnable()
    {
        LoadSkippedScripts();
        RefreshScriptsList();
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }

    private void OnGUI()
    {
        GUILayout.Label("Skipped Scripts (comma or space separated):", EditorStyles.boldLabel);

        // Bigger input field for skipped scripts
        skippedScriptsInput = GUILayout.TextArea(skippedScriptsInput, GUILayout.Height(40));

        if (GUILayout.Button("Save and Refresh"))
        {
            SaveSkippedScripts();
            RefreshScriptsList();
        }

        if (scriptToGameObjects == null || scriptToGameObjects.Count == 0)
        {
            GUILayout.Label("No scripts found in the scene.");
            return;
        }

        // Title row for script name and game object names
        GUILayout.BeginHorizontal();
        GUILayout.Label("SCRIPT NAME", EditorStyles.boldLabel, GUILayout.Width(TITLE_WIDTH));
        GUILayout.Label("GAME OBJECTS", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - SCROLLVIEW_MARGIN - 60)); // Leave space for the floating panel

        // Define a custom GUI style for the clickable label
        GUIStyle clickableLabelStyle = new GUIStyle(EditorStyles.label)
        {
            normal = { textColor = Color.gray }, // Color for the text
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(0, 0, 0, 0), // Remove padding
            border = new RectOffset(0, 0, 0, 0) // Remove border
        };

        foreach (var entry in scriptToGameObjects)
        {
            GUILayout.BeginHorizontal();

            // Script name with fixed width and custom style for clickable label
            Rect scriptNameRect = GUILayoutUtility.GetRect(new GUIContent(entry.Key), clickableLabelStyle, GUILayout.Width(SCRIPT_NAME_WIDTH), GUILayout.ExpandWidth(false));
            if (Event.current.type == EventType.MouseDown && scriptNameRect.Contains(Event.current.mousePosition))
            {
                SelectAllGameObjectsWithScript(entry.Key);
                Event.current.Use(); // Consume the event to prevent further processing
            }

            GUI.Label(scriptNameRect, entry.Key, clickableLabelStyle);

            GUILayout.BeginVertical(); // Wrap the buttons into multiple rows if necessary
            GUILayout.BeginHorizontal();

            float availableWidth = position.width - TITLE_WIDTH; // Available width for the buttons, leaving space for script name and padding
            float currentWidth = 0;

            foreach (var go in entry.Value)
            {
                // Shorten GameObject name by removing vowels
                string shortenedName = RemoveVowels(go.name);

                float buttonWidth = BUTTON_WIDTH + BORDER_PADDING; // Button width + space between buttons

                if (currentWidth + buttonWidth > availableWidth)
                {
                    // If the current row is full, start a new row
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    currentWidth = 0;
                }

                if (GUILayout.Button(shortenedName, GUILayout.Width(BUTTON_WIDTH)))
                {
                    SelectGameObject(go);
                }

                GUILayout.Space(BUTTON_SPACING); // Minimum space between buttons
                currentWidth += buttonWidth; // Update current width with button and spacing
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        // Floating text panel at the bottom
        GUILayout.BeginArea(new Rect(0, position.height - 40, position.width, 50), GUI.skin.box); // Create a box area with border
        GUILayout.Label("Guide: Click on the script names to select all GameObjects with that script. Click on GameObject buttons to select individual GameObjects.", EditorStyles.wordWrappedLabel);
        GUILayout.EndArea();
    }

    // Helper method to remove vowels from a string, keeping the first character
    private string RemoveVowels(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        string vowels = "AEIOUaeiou";
        char firstChar = name[0];
        string restOfString = name.Substring(1);

        // Remove vowels from the rest of the string
        string restWithoutVowels = new string(restOfString.Where(c => !vowels.Contains(c)).ToArray());

        // Recombine the first character with the modified rest of the string
        return firstChar + restWithoutVowels;
    }

    // Save the skipped scripts input to EditorPrefs
    private void SaveSkippedScripts()
    {
        EditorPrefs.SetString(SKIPPED_SCRIPTS_PREF_KEY, skippedScriptsInput);
    }

    // Load the skipped scripts from EditorPrefs
    private void LoadSkippedScripts()
    {
        // Default script names to skip (same as the original `scriptsToSkip` list)
        string defaultSkippedScripts = "Transform, Camera, Light, CanvasScaler, RigidBody, RectTransform";

        // Load from EditorPrefs if it exists, otherwise use the default values
        skippedScriptsInput = EditorPrefs.GetString(SKIPPED_SCRIPTS_PREF_KEY, defaultSkippedScripts);
    }


    private void RefreshScriptsList()
    {
        scriptToGameObjects = new Dictionary<string, List<GameObject>>();
        MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>();

        // Process skipped scripts input
        List<string> skippedScripts = skippedScriptsInput.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(s => s.Trim().ToLower()) // Trim and convert to lower case
                                                          .ToList();

        foreach (var script in allScripts)
        {
            string scriptName = script.GetType().Name;

            // Only add scripts not in the skipped list (case-insensitive)
            if (!skippedScripts.Contains(scriptName.ToLower()))
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

    private void SelectAllGameObjectsWithScript(string scriptName)
    {
        if (scriptToGameObjects.ContainsKey(scriptName))
        {
            var gameObjects = scriptToGameObjects[scriptName];
            Selection.objects = gameObjects.ToArray();
        }
    }

    // Called whenever the hierarchy changes
    private void OnHierarchyChanged()
    {
        RefreshScriptsList();
        Repaint(); // Ensure the window gets repainted to reflect the changes
    }
}
