using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using TMPro;
using FantasyUI.Theme;

namespace FantasyUI.Editor
{
    /// <summary>
    /// Generates TextMeshPro SDF font assets for the imported Skranji font family,
    /// synchronizes the FantasyDisplay_Outline material to Skranji's SDF atlas,
    /// updates all UIThemeSO presets and TMP Settings, and applies Skranji to every
    /// TMP_Text across all prefabs and scenes in the project.
    /// </summary>
    [InitializeOnLoad]
    public static class FantasyUIFontUpdater
    {
        public const string SkranjiBoldTtfPath    = "Assets/Fonts/Skranji/Skranji-Bold.ttf";
        public const string SkranjiRegularTtfPath = "Assets/Fonts/Skranji/Skranji-Regular.ttf";
        public const string SkranjiBoldSdfPath    = "Assets/Fonts/Skranji/Skranji-Bold SDF.asset";
        public const string SkranjiRegularSdfPath = "Assets/Fonts/Skranji/Skranji-Regular SDF.asset";
        public const string OutlineMatPath        = "Assets/FantasyUIKit/Art/Materials/FantasyDisplay_Outline.mat";
        private const string DefaultThemePath     = "Assets/FantasyUIKit/Themes/Theme_DarkFantasyDefault.asset";

        private const string PreloadAsciiChars =
            " !\"#$%&'()*+,-./0123456789:;<=>?@" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`" +
            "abcdefghijklmnopqrstuvwxyz{|}~" +
            "\u2013\u2014\u2022\u2026\u00D7\u2605\u2713";

        static FantasyUIFontUpdater()
        {
            EditorApplication.delayCall += AutoApplySkranjiIfNeeded;
        }

        private static void AutoApplySkranjiIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(SkranjiBoldTtfPath)) return;

            var existingFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SkranjiBoldSdfPath);
            var defaultTheme = AssetDatabase.LoadAssetAtPath<UIThemeSO>(DefaultThemePath);
            if (existingFont == null || (defaultTheme != null && defaultTheme.displayFont != existingFont))
            {
                Debug.Log("[FantasyUIKit] Generating Skranji TMP SDF assets and updating all TextMeshPro texts...");
                ApplySkranjiToAllTMPTexts();
            }
        }

        [MenuItem("Fantasy UI/6. Apply Skranji Font to All TMP Texts", priority = 30)]
        public static void ApplySkranjiToAllTMPTexts()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // Generate both Skranji-Bold SDF (primary display/UI font) and Skranji-Regular SDF
            TMP_FontAsset skranjiBold = EnsureSkranjiDisplayFont();
            EnsureFontAsset(SkranjiRegularTtfPath, SkranjiRegularSdfPath, isBoldFace: false);

            if (skranjiBold == null)
            {
                Debug.LogError($"[FantasyUIKit] Failed to create or load Skranji SDF font asset from '{SkranjiBoldTtfPath}'.");
                return;
            }

            Material outlineMat = EnsureAndSyncOutlineMaterial(skranjiBold, OutlineMatPath);

            // 1. Update TMP Settings default font asset
            UpdateTMPSettingsDefaultFont(skranjiBold);

            // 2. Update all UIThemeSO assets
            int themeCount = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:UIThemeSO", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var theme = AssetDatabase.LoadAssetAtPath<UIThemeSO>(path);
                if (theme == null) continue;
                theme.displayFont = skranjiBold;
                if (outlineMat != null)
                    theme.displayOutlineMaterial = outlineMat;
                EditorUtility.SetDirty(theme);
                themeCount++;
            }

            // 3. Update every TMP_Text inside all prefabs under Assets/
            int prefabCount = 0;
            int prefabTextCount = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    var texts = scope.prefabContentsRoot.GetComponentsInChildren<TMP_Text>(true);
                    if (texts == null || texts.Length == 0) continue;

                    bool modified = false;
                    foreach (var tmp in texts)
                    {
                        if (ApplyFontToText(tmp, skranjiBold, outlineMat, isPrefabInstanceInScene: false))
                        {
                            modified = true;
                            prefabTextCount++;
                        }
                    }

                    if (modified)
                        prefabCount++;
                }
            }

            AssetDatabase.SaveAssets();

            // 4. Update every TMP_Text inside all scenes under Assets/
            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            int sceneCount = 0;
            int sceneTextCount = 0;

            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
                {
                    string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath)) continue;

                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    bool sceneModified = false;

                    foreach (var root in scene.GetRootGameObjects())
                    {
                        foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
                        {
                            bool isPrefabInst = PrefabUtility.IsPartOfPrefabInstance(tmp);
                            if (ApplyFontToText(tmp, skranjiBold, outlineMat, isPrefabInstanceInScene: isPrefabInst))
                            {
                                sceneModified = true;
                                sceneTextCount++;
                            }
                        }
                    }

                    if (sceneModified)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        sceneCount++;
                    }
                }
            }
            finally
            {
                if (previousSetup != null && previousSetup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                else if (File.Exists("Assets/Scenes/Gameplayscene.unity"))
                    EditorSceneManager.OpenScene("Assets/Scenes/Gameplayscene.unity", OpenSceneMode.Single);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[FantasyUIKit] Applied Skranji-Bold SDF across {themeCount} themes, {prefabTextCount} TMP texts in {prefabCount} prefabs, and {sceneTextCount} TMP texts in {sceneCount} scenes.");
        }

        public static TMP_FontAsset EnsureSkranjiDisplayFont()
        {
            return EnsureFontAsset(SkranjiBoldTtfPath, SkranjiBoldSdfPath, isBoldFace: true);
        }

        public static TMP_FontAsset EnsureFontAsset(string ttfPath, string sdfPath, bool isBoldFace)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(sdfPath);
            if (existing != null && existing.atlasTexture != null && existing.material != null)
            {
                if (existing.characterTable == null || existing.characterTable.Count < 32)
                {
                    existing.TryAddCharacters(PreloadAsciiChars, out _);
                    EditorUtility.SetDirty(existing);
                    EditorUtility.SetDirty(existing.atlasTexture);
                    AssetDatabase.SaveAssets();
                }
                return existing;
            }

            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (sourceFont == null)
                return null;

            string dir = Path.GetDirectoryName(sdfPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (existing != null)
                AssetDatabase.DeleteAsset(sdfPath);

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true);

            if (fontAsset == null)
                return null;

            string baseName = Path.GetFileNameWithoutExtension(sdfPath);
            fontAsset.name = baseName;
            // Skranji-Bold is already thick; keep synthetic bold dilation subtle so Bold labels stay crisp.
            fontAsset.boldStyle = isBoldFace ? 0.18f : 0.75f;

            var fontSo = new SerializedObject(fontAsset);
            var clearProp = fontSo.FindProperty("m_ClearDynamicDataOnBuild");
            if (clearProp != null)
            {
                clearProp.boolValue = false;
                fontSo.ApplyModifiedPropertiesWithoutUndo();
            }

            ShaderUtilities.GetShaderPropertyIDs();
            AssetDatabase.CreateAsset(fontAsset, sdfPath);

            if (fontAsset.atlasTexture != null)
            {
                fontAsset.atlasTexture.name = baseName + " Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            }

            if (fontAsset.material != null)
            {
                fontAsset.material.name = baseName + " Material";
                fontAsset.material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            fontAsset.TryAddCharacters(PreloadAsciiChars, out _);

            EditorUtility.SetDirty(fontAsset);
            if (fontAsset.atlasTexture != null) EditorUtility.SetDirty(fontAsset.atlasTexture);
            if (fontAsset.material != null) EditorUtility.SetDirty(fontAsset.material);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(sdfPath, ImportAssetOptions.ForceSynchronousImport);

            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(sdfPath);
        }

        public static Material EnsureAndSyncOutlineMaterial(TMP_FontAsset font, string outlineMatPath)
        {
            if (font == null || font.material == null) return null;

            string dir = Path.GetDirectoryName(outlineMatPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            Material outlineMat = AssetDatabase.LoadAssetAtPath<Material>(outlineMatPath);
            if (outlineMat == null)
            {
                outlineMat = new Material(font.material);
                outlineMat.name = "FantasyDisplay_Outline";
                AssetDatabase.CreateAsset(outlineMat, outlineMatPath);
            }
            else
            {
                outlineMat.shader = font.material.shader;
            }

            // Always bind to the active font's SDF atlas & metrics
            ShaderUtilities.GetShaderPropertyIDs();
            outlineMat.SetTexture(ShaderUtilities.ID_MainTex, font.atlasTexture);
            outlineMat.SetFloat(ShaderUtilities.ID_TextureWidth, font.atlasWidth);
            outlineMat.SetFloat(ShaderUtilities.ID_TextureHeight, font.atlasHeight);
            outlineMat.SetFloat(ShaderUtilities.ID_GradientScale, font.atlasPadding + 1f);
            outlineMat.SetFloat(ShaderUtilities.ID_WeightNormal, font.normalStyle);
            outlineMat.SetFloat(ShaderUtilities.ID_WeightBold, font.boldStyle);

            outlineMat.EnableKeyword("OUTLINE_ON");
            outlineMat.EnableKeyword("UNDERLAY_ON");
            outlineMat.SetFloat("_FaceDilate", 0.08f);
            outlineMat.SetColor("_OutlineColor", new Color32(30, 21, 32, 255)); // #1E1520
            outlineMat.SetFloat("_OutlineWidth", 0.22f);
            outlineMat.SetColor("_UnderlayColor", new Color32(18, 12, 20, 225));
            outlineMat.SetFloat("_UnderlayOffsetX", 0.25f);
            outlineMat.SetFloat("_UnderlayOffsetY", -0.65f);
            outlineMat.SetFloat("_UnderlayDilate", 0.25f);
            outlineMat.SetFloat("_UnderlaySoftness", 0.12f);

            EditorUtility.SetDirty(outlineMat);
            AssetDatabase.SaveAssets();
            return outlineMat;
        }

        private static void UpdateTMPSettingsDefaultFont(TMP_FontAsset font)
        {
            var settings = TMP_Settings.instance;
            if (settings == null || font == null) return;

            var so = new SerializedObject(settings);
            var prop = so.FindProperty("m_defaultFontAsset");
            if (prop != null && prop.objectReferenceValue != font)
            {
                prop.objectReferenceValue = font;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(settings);
            }
        }

        private static bool ApplyFontToText(TMP_Text tmp, TMP_FontAsset targetFont, Material outlineMat, bool isPrefabInstanceInScene)
        {
            if (tmp == null || targetFont == null) return false;

            Material currentMat = tmp.fontSharedMaterial;
            bool useOutline = outlineMat != null && (
                currentMat == outlineMat ||
                (currentMat != null && currentMat.name.IndexOf("Outline", System.StringComparison.OrdinalIgnoreCase) >= 0) ||
                (currentMat != null && currentMat.IsKeywordEnabled("OUTLINE_ON"))
            );

            Material targetMat = useOutline ? outlineMat : targetFont.material;

            bool needsChange = tmp.font != targetFont || tmp.fontSharedMaterial != targetMat;
            if (!needsChange) return false;

            tmp.font = targetFont;
            tmp.fontSharedMaterial = targetMat;
            EditorUtility.SetDirty(tmp);

            if (isPrefabInstanceInScene)
                PrefabUtility.RecordPrefabInstancePropertyModifications(tmp);

            return true;
        }
    }
}
