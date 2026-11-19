using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace FantasyUI.Editor
{
    /// <summary>
    /// Automatically configures TextureImporter settings and exact 9-slice sprite borders
    /// from Assets/FantasyUIKit/Art/Sprites/sprite_slices.json whenever Fantasy UI sprites are imported.
    /// </summary>
    public class FantasyUISpritePostprocessor : AssetPostprocessor
    {
        private const string SpritesRoot = "Assets/FantasyUIKit/Art/Sprites/";
        private const string ManifestPath = "Assets/FantasyUIKit/Art/Sprites/sprite_slices.json";

        private static Dictionary<string, Vector4> _borderCache;
        private static DateTime _lastManifestWrite;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(SpritesRoot, StringComparison.OrdinalIgnoreCase))
                return;

            var importer = assetImporter as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;

            string rel = assetPath.Substring(SpritesRoot.Length).Replace("\\", "/");
            var borders = LoadBorders();
            if (borders.TryGetValue(rel, out var border))
            {
                importer.spriteBorder = border;
            }
        }

        public static void ForceReimportAllSprites()
        {
            _borderCache = null;
            if (!Directory.Exists(SpritesRoot)) return;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesRoot.TrimEnd('/') });
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.Refresh();
        }

        private static Dictionary<string, Vector4> LoadBorders()
        {
            if (!File.Exists(ManifestPath))
                return new Dictionary<string, Vector4>(StringComparer.OrdinalIgnoreCase);

            var writeTime = File.GetLastWriteTimeUtc(ManifestPath);
            if (_borderCache != null && writeTime == _lastManifestWrite)
                return _borderCache;

            _lastManifestWrite = writeTime;
            _borderCache = new Dictionary<string, Vector4>(StringComparer.OrdinalIgnoreCase);

            string json = File.ReadAllText(ManifestPath);
            // Parse entries: "Panels/Panel_ParchmentScroll.png": { "border": [48, 48, 48, 48], ... }
            var rx = new Regex("\"([^\"]+\\.png)\"\\s*:\\s*\\{\\s*\"border\"\\s*:\\s*\\[\\s*([0-9\\.]+)\\s*,\\s*([0-9\\.]+)\\s*,\\s*([0-9\\.]+)\\s*,\\s*([0-9\\.]+)\\s*\\]", RegexOptions.Singleline);
            foreach (Match m in rx.Matches(json))
            {
                string key = m.Groups[1].Value;
                float l = float.Parse(m.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
                float b = float.Parse(m.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                float r = float.Parse(m.Groups[4].Value, System.Globalization.CultureInfo.InvariantCulture);
                float t = float.Parse(m.Groups[5].Value, System.Globalization.CultureInfo.InvariantCulture);
                _borderCache[key] = new Vector4(l, b, r, t);
            }
            return _borderCache;
        }
    }
}
