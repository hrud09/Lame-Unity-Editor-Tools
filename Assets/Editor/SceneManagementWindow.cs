using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class SceneManagementWindow : EditorWindow
{
    private List<string> assignedScenes = new List<string>();
    private List<string> unassignedScenes = new List<string>();

    [MenuItem("Window/Scene Management")]
    public static void ShowWindow()
    {
        GetWindow<SceneManagementWindow>("Scene Management");
    }

    private void OnGUI()
    {
        // Wide Build Settings button
        if (GUILayout.Button("Build Settings", GUILayout.Height(30), GUILayout.ExpandWidth(true)))
        {
            // Opens the Build Settings window
            BuildPlayerWindow.ShowBuildPlayerWindow();
        }

        GUILayout.Space(20);

        GUILayout.Label("Assigned Scenes", EditorStyles.boldLabel);
        GUILayout.BeginVertical(GUI.skin.box);
        foreach (string scene in assignedScenes)
        {
            DrawSceneEntry(scene, true);
        }
        GUILayout.EndVertical();

        GUILayout.Space(20);

        GUILayout.Label("Unassigned Scenes", EditorStyles.boldLabel);
        GUILayout.BeginVertical(GUI.skin.box);
        foreach (string scene in unassignedScenes)
        {
            DrawSceneEntry(scene, false);
        }
        GUILayout.EndVertical();
    }

    private void DrawSceneEntry(string scene, bool isAssigned)
    {
        string sceneName = Path.GetFileNameWithoutExtension(scene);

        GUILayout.BeginHorizontal();
        GUILayout.Label(sceneName, EditorStyles.label);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(isAssigned ? "Unassign" : "Assign"))
        {
            ToggleSceneAssignment(scene, isAssigned);
        }
        if (GUILayout.Button("Load"))
        {
            EditorSceneManager.OpenScene(scene);
        }
        GUILayout.EndHorizontal();
    }

    private void ToggleSceneAssignment(string scene, bool isAssigned)
    {
        EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(scene, !isAssigned);
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (isAssigned)
            scenes.RemoveAll(s => s.path == scene);
        else
            scenes.Add(newScene);
        EditorBuildSettings.scenes = scenes.ToArray();
        LoadScenes();
    }

    private void OnFocus()
    {
        LoadScenes();
    }

    private void LoadScenes()
    {
        List<string> tempAssignedScenes = new List<string>();
        List<string> tempUnassignedScenes = new List<string>();

        string[] allScenes = AssetDatabase.FindAssets("t:scene");
        foreach (string sceneGUID in allScenes)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGUID);
            if (EditorBuildSettings.scenes.Any(s => s.path == scenePath))
            {
                tempAssignedScenes.Add(scenePath);
            }
            else
            {
                tempUnassignedScenes.Add(scenePath);
            }
        }

        // Assign the scenes after the iteration is complete
        assignedScenes = tempAssignedScenes;
        unassignedScenes = tempUnassignedScenes;

        Repaint();
    }
}
