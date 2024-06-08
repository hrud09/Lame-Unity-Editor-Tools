using UnityEditor;
using UnityEngine;
using System.IO;

public class ScreenshotTakerEditorWindow : EditorWindow
{

    private string folderName = "Screenshots"; // Update the folder name here
    private string fileName = "screenshot";
    private int screenshotSource = 0;
    private int takeNumber = 0; // New variable for screenshot number

    private GUIStyle sectionStyle;
    private GUIStyle outlineStyle;
   
    private int selectedResulationTypeIndex = 0;
    private int selectedResolutionIndex = 0;
    private Resolution[] resolutions;
    private string x = "100";
    private string y = "100";

    [MenuItem("Window/Capture Screen")]
    private static void ShowWindow()
    {
        ScreenshotTakerEditorWindow window = GetWindow<ScreenshotTakerEditorWindow>();
        window.titleContent = new GUIContent("Take Screenshots");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }

    private void OnEnable()
    {
        sectionStyle = new GUIStyle();
        sectionStyle.normal.background = MakeTex(1, 1, new Color(0.7f, 0.7f, 0.7f, 0.4f));
        //sectionStyle.padding = new RectOffset(10, 10, 10, 10);

        outlineStyle = new GUIStyle();
        outlineStyle.normal.background = MakeTex(1, 1, new Color(0.3f, 0.3f, 0.3f, 1f));
        outlineStyle.padding = new RectOffset(1, 1, 1, 1);

        resolutions = Screen.resolutions;

        // Load the screenshot take number from PlayerPrefs when the window is enabled
        takeNumber = PlayerPrefs.GetInt("ScreenshotTakeNumber", 1);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("CAPTURE SCREEN", EditorStyles.boldLabel);
        DrawSection(() =>
        {
            // Folder Name field
            folderName = EditorGUILayout.TextField("Folder Name :", folderName);

            EditorGUILayout.Space();

            // Screenshot Take Number field
            takeNumber = EditorGUILayout.IntField("Take :", takeNumber);

            EditorGUILayout.Space();

            // File Name field with screenshot take number appended
            fileName = EditorGUILayout.TextField("File Name :", $"screenShot_{takeNumber}");

            EditorGUILayout.Space();

            // Screenshot Source radio buttons
            EditorGUILayout.LabelField("Source :", EditorStyles.boldLabel);
            screenshotSource = GUILayout.SelectionGrid(screenshotSource, new string[] { "Scene", "Game" }, 2, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Width(200), GUILayout.Height(20));

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Resolution Type :", EditorStyles.boldLabel);
            selectedResulationTypeIndex = GUILayout.SelectionGrid(selectedResulationTypeIndex, new string[] { "Auto", "Manual" }, 2, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Width(200), GUILayout.Height(20));

            EditorGUILayout.Space();

            if (selectedResulationTypeIndex == 0)
            {

                // Resolution selection dropdown
                EditorGUILayout.LabelField("Resolution:", EditorStyles.boldLabel);
                selectedResolutionIndex = EditorGUILayout.Popup(selectedResolutionIndex, GetResolutionOptions());

                EditorGUILayout.Space();
            }
            else if (selectedResulationTypeIndex == 1)
            {
                x = EditorGUILayout.TextField("X: ", x);
                y = EditorGUILayout.TextField("Y: ", y);

                EditorGUILayout.Space();
            }


            // Take Screenshot button
            if (GUILayout.Button("Capture Screen", GUILayout.Height(30)))
            {
                string folderFullPath = Path.Combine(Application.dataPath, "..", folderName);
                Directory.CreateDirectory(folderFullPath);

                string filePath = Path.Combine(folderFullPath, $"{fileName}.png");

                if (screenshotSource == 0)
                {
                    // Take screenshot from Scene View
                    TakeSceneViewScreenshot(filePath);
                }
                else if (screenshotSource == 1)
                {
                    // Take screenshot from Game View
                    ScreenCapture.CaptureScreenshot(filePath, 1);
                }

                AssetDatabase.Refresh();
                Debug.Log("Screenshot saved at: " + filePath);

                // Increment the screenshot take number after each screenshot taken
                takeNumber++;

                // Save the updated screenshot take number to PlayerPrefs
                PlayerPrefs.SetInt("ScreenshotTakeNumber", takeNumber);
            }

            EditorGUILayout.Space();

            // Open Folder button
            if (GUILayout.Button("Open Folder", GUILayout.Height(30)))
            {
                string folderFullPath = Path.Combine(Application.dataPath, "..", folderName);
                EditorUtility.RevealInFinder(folderFullPath);

            }
        });
    }

    private void DrawSection(System.Action content)
    {
        EditorGUILayout.BeginVertical(sectionStyle);
        EditorGUILayout.BeginVertical(outlineStyle);
        EditorGUILayout.Space();
        content.Invoke();
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndVertical();
    }

    private Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = color;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    private string[] GetResolutionOptions()
    {
        string[] options = new string[resolutions.Length];
        for (int i = 0; i < resolutions.Length; i++)
        {
            options[i] = $"{resolutions[i].width}x{resolutions[i].height}";
        }
        return options;
    }

    private void TakeSceneViewScreenshot(string filePath)
    {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null)
        {
            int width = 0;
            int height = 0;
            if (selectedResulationTypeIndex == 0)
            {
                width = resolutions[selectedResolutionIndex].width;
                height = resolutions[selectedResolutionIndex].height;
            }
            else
            {
                width = int.Parse(x);
                height = int.Parse(y);
            }
            RenderTexture rt = new RenderTexture(width, height, 24);
            Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

            sceneView.camera.targetTexture = rt;
            sceneView.camera.Render();
            RenderTexture.active = rt;
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            sceneView.camera.targetTexture = null;
            RenderTexture.active = null;
            DestroyImmediate(rt);

            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            DestroyImmediate(screenshot);
        }
        else
        {
            Debug.LogWarning("No active Scene View found.");
        }
    }

    private void OnDisable()
    {
        // Save the screenshot take number to PlayerPrefs when the window is disabled or the project is closed
        PlayerPrefs.SetInt("ScreenshotTakeNumber", takeNumber);
    }
}
