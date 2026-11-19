using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using FantasyUI.Theme;

namespace FantasyUI.Editor
{
    /// <summary>
    /// Visual Palette, Prefab Spawner, Theme Switcher, and Package Management window for the Fantasy UI Kit.
    /// Open via: Window -> Fantasy UI -> UI Library & Builder
    /// </summary>
    public class FantasyUILibraryWindow : EditorWindow
    {
        private static readonly string[] CategoryFolders =
        {
            "01_Primitives",
            "02_BarsAndCounters",
            "03_HUDModules",
            "04_PopupsAndModals",
            "05_NavigationAndMap",
            "06_FeedbackAndFX"
        };

        private static readonly string[] CategoryNames =
        {
            "Primitives", "Bars & HUD", "Modules", "Popups", "Map & Crest", "FX & Toasts"
        };

        private int _selectedTab;
        private string _searchQuery = "";
        private Vector2 _scrollPos;
        private List<GameObject> _prefabs = new List<GameObject>();
        private List<UIThemeSO> _themes = new List<UIThemeSO>();

        [MenuItem("Window/Fantasy UI/UI Library & Builder")]
        public static void OpenWindow()
        {
            var win = GetWindow<FantasyUILibraryWindow>("Fantasy UI Library");
            win.minSize = new Vector2(460, 560);
        }

        private void OnEnable() => RefreshCatalog();
        private void OnFocus() => RefreshCatalog();

        private void RefreshCatalog()
        {
            _prefabs.Clear();
            string folder = Path.Combine(FantasyUIPrefabFactory.PrefabsPath, CategoryFolders[_selectedTab]);
            if (Directory.Exists(folder))
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { folder }))
                {
                    var pf = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                    if (pf != null) _prefabs.Add(pf);
                }
            }

            _themes.Clear();
            foreach (var guid in AssetDatabase.FindAssets("t:UIThemeSO", new[] { "Assets/FantasyUIKit/Themes" }))
            {
                var t = AssetDatabase.LoadAssetAtPath<UIThemeSO>(AssetDatabase.GUIDToAssetPath(guid));
                if (t != null) _themes.Add(t);
            }
        }

        private void OnGUI()
        {
            // Header Banner
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("COMPLETE FANTASY UI KIT — LIBRARY & BUILDER", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build / Rebuild All", GUILayout.Height(26)))
            {
                FantasyUIPackageBuilder.BuildCompletePackageAndScenes();
                RefreshCatalog();
            }
            if (GUILayout.Button("Fix 9-Slice & PPU", GUILayout.Height(26)))
            {
                FantasyUIPackageBuilder.ResliceAndAutoFitAllUI();
            }
            if (GUILayout.Button("Open GameplayScene", GUILayout.Height(26)))
                EditorSceneManager.OpenScene("Assets/Scenes/Gameplayscene.unity");
            if (GUILayout.Button("Open Showcase", GUILayout.Height(26)))
                EditorSceneManager.OpenScene("Assets/FantasyUIKit/Demo/Scenes/UILibraryShowcase.unity");
            if (GUILayout.Button("Export Package", GUILayout.Height(26)))
                FantasyUIPackageBuilder.ExportUnityPackage();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // Live Theme Switcher Bar
            if (_themes.Count > 0)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label("Apply Theme:", EditorStyles.miniBoldLabel, GUILayout.Width(86));
                foreach (var theme in _themes)
                {
                    if (GUILayout.Button(theme.themeName, EditorStyles.miniButton))
                        ApplyThemeInScene(theme);
                }
                EditorGUILayout.EndHorizontal();
            }

            // Category Tabs & Search
            int nextTab = GUILayout.Toolbar(_selectedTab, CategoryNames);
            if (nextTab != _selectedTab)
            {
                _selectedTab = nextTab;
                RefreshCatalog();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Filter:", GUILayout.Width(42));
            _searchQuery = EditorGUILayout.TextField(_searchQuery);
            if (GUILayout.Button("Clear", GUILayout.Width(52))) _searchQuery = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // Prefab List
            var filtered = string.IsNullOrWhiteSpace(_searchQuery)
                ? _prefabs
                : _prefabs.Where(p => p.name.IndexOf(_searchQuery, System.StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No prefabs found in this category yet. Click 'Build / Rebuild All' above to generate the full Fantasy UI Library.", MessageType.Info);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (var pf in filtered)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Label(pf.name, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Ping", GUILayout.Width(54)))
                    EditorGUIUtility.PingObject(pf);
                if (GUILayout.Button("Spawn in Canvas", GUILayout.Width(120)))
                    SpawnIntoSceneCanvas(pf);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private static void SpawnIntoSceneCanvas(GameObject prefab)
        {
            Transform parent = null;
            if (Selection.activeTransform != null && Selection.activeTransform.GetComponentInParent<Canvas>() != null)
                parent = Selection.activeTransform;

            if (parent == null)
            {
                var canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas == null)
                {
                    var go = new GameObject("Fantasy Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                    canvas = go.GetComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    var scaler = go.GetComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.matchWidthOrHeight = 0.5f;
                }
                parent = canvas.transform;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(instance, $"Spawn {prefab.name}");
            Selection.activeGameObject = instance;
        }

        private static void ApplyThemeInScene(UIThemeSO theme)
        {
            var mgr = Object.FindFirstObjectByType<UIThemeManager>();
            if (mgr != null)
            {
                Undo.RecordObject(mgr, "Switch Fantasy UI Theme");
                mgr.SetTheme(theme);
                EditorUtility.SetDirty(mgr);
            }
            else
            {
                foreach (var binder in Object.FindObjectsByType<UIThemeBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    Undo.RecordObject(binder, "Apply Fantasy UI Theme");
                    binder.ApplyTheme(theme);
                }
            }
        }
    }
}
