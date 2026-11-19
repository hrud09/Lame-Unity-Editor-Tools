using UnityEditor;
using UnityEngine;
using FantasyUI.Theme;

namespace FantasyUI.Editor
{
    [CustomEditor(typeof(UIThemeSO))]
    public class UIThemeSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var theme = (UIThemeSO)target;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"Fantasy UI Theme: {theme.themeName}", EditorStyles.boldLabel);
            DrawColorSwatches(theme);
            if (GUILayout.Button("Apply Theme Live To Open Scene", GUILayout.Height(28)))
            {
                var mgr = FindFirstObjectByType<UIThemeManager>();
                if (mgr != null) mgr.SetTheme(theme);
                foreach (var b in FindObjectsByType<UIThemeBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    b.ApplyTheme(theme);
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(6);
            base.OnInspectorGUI();
        }

        private static void DrawColorSwatches(UIThemeSO t)
        {
            Color[] swatches =
            {
                t.inkOutline, t.stoneBase, t.parchmentCore, t.woodLight,
                t.leatherPlum, t.emeraldPrimary, t.crimsonPrimary, t.royalPurple,
                t.cobaltBlue, t.goldBright, t.gemCyan
            };
            Rect row = GUILayoutUtility.GetRect(10, 20, GUILayout.ExpandWidth(true));
            float w = row.width / swatches.Length;
            for (int i = 0; i < swatches.Length; i++)
            {
                EditorGUI.DrawRect(new Rect(row.x + i * w + 1, row.y + 2, w - 2, row.height - 4), swatches[i]);
            }
        }
    }
}
