using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Components;
using FantasyUI.Core;
using FantasyUI.Data;
using FantasyUI.Demo;
using FantasyUI.Theme;

namespace FantasyUI.Editor
{
    /// <summary>
    /// End-to-end automated builder for the Complete Fantasy UI Kit package:
    /// - Reimports all sprites with exact 9-slice borders
    /// - Generates bold dark-ink TMP SDF outline material + 3 Theme ScriptableObjects + EventChannel
    /// - Builds all 35+ modular prefabs
    /// - Reconstructs Assets/Scenes/Gameplayscene.unity with all 11 layered Canvases + Demo Controller
    /// - Builds Assets/FantasyUIKit/Demo/Scenes/UILibraryShowcase.unity
    /// </summary>
    [InitializeOnLoad]
    public static class FantasyUIPackageBuilder
    {
        private const int CurrentPackageRevision = 2;
        private const string ThemesDir = "Assets/FantasyUIKit/Themes";
        private const string MaterialsDir = "Assets/FantasyUIKit/Art/Materials";
        private const string DefaultThemePath = ThemesDir + "/Theme_DarkFantasyDefault.asset";
        private const string RoyalThemePath = ThemesDir + "/Theme_ParchmentRoyal.asset";
        private const string AbyssalThemePath = ThemesDir + "/Theme_AbyssalCrimson.asset";
        private const string EventChannelPath = ThemesDir + "/GameUIEventChannel.asset";
        private const string OutlineMatPath = MaterialsDir + "/FantasyDisplay_Outline.mat";
        private const string GameplayScenePath = "Assets/Scenes/Gameplayscene.unity";
        private const string ShowcaseScenePath = "Assets/FantasyUIKit/Demo/Scenes/UILibraryShowcase.unity";

        static FantasyUIPackageBuilder()
        {
            EditorApplication.delayCall += AutoBootstrapIfNeeded;
        }

        private static void AutoBootstrapIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var existingTheme = AssetDatabase.LoadAssetAtPath<UIThemeSO>(DefaultThemePath);
            if (existingTheme == null || existingTheme.packageRevision < CurrentPackageRevision)
            {
                Debug.Log("[FantasyUIKit] Building/upgrading UI library (9-slice borders + exact PPU multipliers + scenes)...");
                BuildCompletePackageAndScenes();
            }
        }

        [MenuItem("Fantasy UI/1. Build Complete UI Library & Scenes", priority = 1)]
        public static void BuildCompletePackageAndScenes()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            FantasyUISpritePostprocessor.ForceReimportAllSprites();
            EnsureLegacyUISpritesNineSliced();
            var (defaultTheme, royalTheme, abyssalTheme, channel) = BuildCoreAssets();

            FantasyUIPrefabFactory.BuildAllPrimitives(defaultTheme);
            FantasyUIPrefabFactory.BuildAllBarsAndCounters(defaultTheme, channel);
            FantasyUIPrefabFactory.BuildAllHudModules(defaultTheme);
            FantasyUIPrefabFactory.BuildAllModals(defaultTheme);
            FantasyUIPrefabFactory.BuildNavigationAndFeedback(defaultTheme, channel);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            BuildGameplayScene(defaultTheme, new[] { defaultTheme, royalTheme, abyssalTheme }, channel);
            BuildShowcaseScene(defaultTheme, new[] { defaultTheme, royalTheme, abyssalTheme });

            EditorSceneManager.OpenScene(GameplayScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[FantasyUIKit] Complete Fantasy UI Library, 35+ Prefabs, Gameplayscene.unity, and UILibraryShowcase.unity built successfully!");
        }

        [MenuItem("Fantasy UI/2. Rebuild GameplayScene Only", priority = 2)]
        public static void RebuildGameplaySceneMenu()
        {
            var (def, royal, abyss, ch) = BuildCoreAssets();
            BuildGameplayScene(def, new[] { def, royal, abyss }, ch);
            EditorSceneManager.OpenScene(GameplayScenePath);
        }

        [MenuItem("Fantasy UI/3. Build & Open Showcase Gallery Scene", priority = 3)]
        public static void BuildAndOpenShowcaseMenu()
        {
            var (def, royal, abyss, _) = BuildCoreAssets();
            BuildShowcaseScene(def, new[] { def, royal, abyss });
            EditorSceneManager.OpenScene(ShowcaseScenePath);
        }

        [MenuItem("Fantasy UI/4. Export FantasyUIKit .unitypackage...", priority = 20)]
        public static void ExportUnityPackage()
        {
            string outFile = EditorUtility.SaveFilePanel("Export Complete Fantasy UI Kit", "", "CompleteFantasyUIKit_v1.0", "unitypackage");
            if (string.IsNullOrEmpty(outFile)) return;
            AssetDatabase.ExportPackage(
                new[] { "Assets/FantasyUIKit", "Assets/2D Assets/UI Pack 1", "Assets/Fonts/Skranji", GameplayScenePath },
                outFile,
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
            Debug.Log($"[FantasyUIKit] Exported package to: {outFile}");
        }

        [MenuItem("Fantasy UI/5. Re-Slice All UI Sprites & Auto-Fit PPU Multipliers", priority = 25)]
        public static void ResliceAndAutoFitAllUI()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            FantasyUISpritePostprocessor.ForceReimportAllSprites();
            EnsureLegacyUISpritesNineSliced();

            // Refresh PPU multipliers on every FantasyUI prefab
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/FantasyUIKit/Prefabs" });
            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    FantasyUIPrefabFactory.ApplyAllSlicedPPUInHierarchy(scope.prefabContentsRoot.transform);
                }
            }

            // Refresh PPU multipliers across the open scene
            foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                FantasyUIPrefabFactory.ApplyAllSlicedPPUInHierarchy(canvas.transform);
                EditorUtility.SetDirty(canvas.gameObject);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[FantasyUIKit] Re-sliced all UI sprites and auto-fitted exact Image.pixelsPerUnitMultiplier across all prefabs & scene canvases.");
        }

        private static void EnsureLegacyUISpritesNineSliced()
        {
            const string legacyRoot = "Assets/2D Assets/Gameplay UI Sprites/UI Sprites";
            if (!Directory.Exists(legacyRoot)) return;
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { legacyRoot });
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;
                    if (importer.textureType != TextureImporterType.Sprite)
                        importer.textureType = TextureImporterType.Sprite;
                    if (importer.spriteBorder == Vector4.zero)
                    {
                        importer.GetSourceTextureWidthAndHeight(out int w, out int h);
                        if (w >= 24 && h >= 24)
                        {
                            int bx = Mathf.Clamp(Mathf.RoundToInt(w * 0.25f), 8, w / 2 - 2);
                            int by = Mathf.Clamp(Mathf.RoundToInt(h * 0.25f), 8, h / 2 - 2);
                            importer.spriteBorder = new Vector4(bx, by, bx, by);
                            importer.SaveAndReimport();
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        // =====================================================================
        // CORE ASSETS: TMP OUTLINE MATERIAL + 3 THEMES + EVENT CHANNEL
        // =====================================================================
        private static (UIThemeSO def, UIThemeSO royal, UIThemeSO abyss, UIEventChannels channel) BuildCoreAssets()
        {
            Directory.CreateDirectory(ThemesDir);
            Directory.CreateDirectory(MaterialsDir);

            TMP_FontAsset font = FantasyUIFontUpdater.EnsureSkranjiDisplayFont();
            if (font == null)
                font = TMP_Settings.defaultFontAsset;
            if (font == null)
                font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

            Material outlineMat = FantasyUIFontUpdater.EnsureAndSyncOutlineMaterial(font, OutlineMatPath);

            var defTheme = LoadOrCreate<UIThemeSO>(DefaultThemePath);
            defTheme.themeName = "Dark Fantasy Default";
            defTheme.packageRevision = CurrentPackageRevision;
            defTheme.displayFont = font;
            defTheme.displayOutlineMaterial = outlineMat;
            EditorUtility.SetDirty(defTheme);

            var royalTheme = LoadOrCreate<UIThemeSO>(RoyalThemePath);
            royalTheme.themeName = "Royal Parchment & Gold";
            royalTheme.displayFont = font;
            royalTheme.displayOutlineMaterial = outlineMat;
            royalTheme.panelTint = new Color32(255, 244, 226, 255);
            royalTheme.backdropDim = new Color32(32, 22, 46, 175);
            royalTheme.textGold = new Color32(255, 224, 92, 255);
            EditorUtility.SetDirty(royalTheme);

            var abyssTheme = LoadOrCreate<UIThemeSO>(AbyssalThemePath);
            abyssTheme.themeName = "Abyssal Crimson Boss";
            abyssTheme.displayFont = font;
            abyssTheme.displayOutlineMaterial = outlineMat;
            abyssTheme.panelTint = new Color32(238, 198, 212, 255);
            abyssTheme.backdropDim = new Color32(28, 10, 18, 215);
            abyssTheme.textGold = new Color32(255, 168, 72, 255);
            EditorUtility.SetDirty(abyssTheme);

            var channel = LoadOrCreate<UIEventChannels>(EventChannelPath);
            EditorUtility.SetDirty(channel);

            AssetDatabase.SaveAssets();
            return (defTheme, royalTheme, abyssTheme, channel);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        // =====================================================================
        // RECONSTRUCT ASSETS/SCENES/GAMEPLAYSCENE.UNITY (ALL 11 CANVASES)
        // =====================================================================
        private static void BuildGameplayScene(UIThemeSO theme, UIThemeSO[] allThemes, UIEventChannels channel)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Main UI Camera (matches existing GameplayScene setup with subtle dusk backdrop)
            var camGo = new GameObject("Main UI Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color32(46, 36, 58, 255);
            cam.transform.position = new Vector3(0, 1, -10);

            // EventSystem
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            // UI System Root
            var sysRoot = new GameObject("UI System Root");
            sysRoot.AddComponent<UIManager>();
            var themeMgr = sysRoot.AddComponent<UIThemeManager>();
            themeMgr.ConfigureThemes(theme, allThemes);
            var demoCtrl = sysRoot.AddComponent<GameplayUIDemoController>();

            // All Canvases container (Exact GameplayScene hierarchy structure!)
            var allCanvases = new GameObject("All Canvases").transform;

            // Optional Scenic Dusk Backdrop behind Gameplay UI (Order -10)
            var bgCanvas = CreateLayeredCanvas("Dusk Backdrop Canvas", allCanvases, -10, false);
            var bgImg = FantasyUIPrefabFactory.AddImage(
                FantasyUIPrefabFactory.CreateUIObject("DuskSkyGradient", bgCanvas, Vector2.zero),
                FantasyUIPrefabFactory.LoadSprite("Panels/Panel_DuskBackdrop.png"));
            FantasyUIPrefabFactory.Stretch(bgImg.rectTransform);

            // [Order 0] Minimap Canvas
            var minimapCanvas = CreateLayeredCanvas("Minimap Canvas", allCanvases, (int)UILayer.Minimap);
            var minimapInst = SpawnPrefab("03_HUDModules/HUD_CircularMinimap.prefab", minimapCanvas, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-148, -264));
            var minimapCtrl = minimapInst.GetComponent<MinimapController>();

            // [Order 1] Healthbar Show Canvas
            var hpShowCanvas = CreateLayeredCanvas("Healthbar Show Canvas", allCanvases, (int)UILayer.WorldHealthbars);
            var floatHp = SpawnPrefab("03_HUDModules/HUD_FloatingHealthbar.prefab", hpShowCanvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 190));
            floatHp.SetActive(false);

            // [Order 2] Steering Wheel Canvas
            var steerCanvas = CreateLayeredCanvas("Steering Wheel Canvas", allCanvases, (int)UILayer.GameplayHUD);
            var joyInst = SpawnPrefab("03_HUDModules/HUD_VirtualJoystick.prefab", steerCanvas, new Vector2(0, 0), new Vector2(0, 0), new Vector2(190, 260));
            var joystick = joyInst.GetComponent<VirtualJoystick>();

            // [Order 2] Weapon UI Canvas
            var weaponCanvas = CreateLayeredCanvas("Weapon UI Canvas", allCanvases, (int)UILayer.GameplayHUD);
            var weaponHolder = FantasyUIPrefabFactory.CreateUIObject("Weapon UI Holder", weaponCanvas, new Vector2(400, 320));
            weaponHolder.anchorMin = weaponHolder.anchorMax = new Vector2(1, 0);
            weaponHolder.anchoredPosition = new Vector2(-215, 245);

            var wPrimary = SpawnPrefab("03_HUDModules/HUD_WeaponSlot_Primary.prefab", weaponHolder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(85, -35)).GetComponent<WeaponSlotView>();
            var wSec1 = SpawnPrefab("03_HUDModules/HUD_WeaponSlot_Secondary.prefab", weaponHolder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-58, -50)).GetComponent<WeaponSlotView>();
            var wSec2 = SpawnPrefab("03_HUDModules/HUD_WeaponSlot_Secondary.prefab", weaponHolder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-24, 64)).GetComponent<WeaponSlotView>();
            var wSec3 = SpawnPrefab("03_HUDModules/HUD_WeaponSlot_Secondary.prefab", weaponHolder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(90, 94)).GetComponent<WeaponSlotView>();

            // [Order 2] Top HUD Canvas (Drop Shadow, Timer, Player Profile, Currencies, Bottom Versus Dock)
            var topHudCanvas = CreateLayeredCanvas("Top HUD Canvas", allCanvases, (int)UILayer.GameplayHUD);
            var topDrop = FantasyUIPrefabFactory.CreateUIObject("Drop Shadow BG Image", topHudCanvas, new Vector2(0, 128));
            topDrop.anchorMin = new Vector2(0, 1); topDrop.anchorMax = new Vector2(1, 1); topDrop.pivot = new Vector2(0.5f, 1);
            FantasyUIPrefabFactory.AddImage(topDrop, FantasyUIPrefabFactory.LoadSprite("FX/FX_DropFadeShadow.png"));

            var timerInst = SpawnPrefab("02_BarsAndCounters/HUD_TimerPill.prefab", topHudCanvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(106, -48));
            var timerTxt = timerInst.transform.Find("TimerText").GetComponent<TMP_Text>();

            var profileInst = SpawnPrefab("02_BarsAndCounters/HUD_PlayerProfile.prefab", topHudCanvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(362, -60));
            var profileHp = profileInst.GetComponentInChildren<FantasyProgressBar>();

            var goldInst = SpawnPrefab("02_BarsAndCounters/HUD_GoldBar.prefab", topHudCanvas, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-152, -46));
            var goldCtrl = goldInst.GetComponent<CurrencyCounterView>();

            var gemInst = SpawnPrefab("02_BarsAndCounters/HUD_GemBar.prefab", topHudCanvas, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-152, -110));
            var gemCtrl = gemInst.GetComponent<CurrencyCounterView>();

            // Bottom Dual-Player Versus Dock (Ref 1 & 2)
            var versusInst = SpawnPrefab("02_BarsAndCounters/HUD_DualPlayerVersusDock.prefab", topHudCanvas, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 62));
            var versusCtrl = versusInst.GetComponent<DualPlayerVersusBar>();

            // [Order 2] Interactive Buttons Canvas (Quests Rail + Quick Action Tiles: BUILD, HOME, ITEMS, PAUSE, MODE)
            var interactCanvas = CreateLayeredCanvas("Interactive Buttons Canvas", allCanvases, (int)UILayer.GameplayHUD);
            var questRailInst = SpawnPrefab("03_HUDModules/HUD_QuestRail.prefab", interactCanvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(162, -328));
            var questCtrl = questRailInst.GetComponent<QuestRailController>();

            var buildTile = SpawnPrefab("01_Primitives/Btn_QuickTile_Build.prefab", interactCanvas, new Vector2(0, 0), new Vector2(0, 0), new Vector2(82, 415));
            var modeTile = SpawnPrefab("01_Primitives/Btn_QuickTile_Home.prefab", interactCanvas, new Vector2(0, 0), new Vector2(0, 0), new Vector2(198, 415));
            var itemsTile = SpawnPrefab("01_Primitives/Btn_QuickTile_Items.prefab", interactCanvas, new Vector2(1, 0), new Vector2(1, 0), new Vector2(-82, 435));
            var pauseTile = SpawnPrefab("01_Primitives/Btn_CircleRing_Back.prefab", interactCanvas, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -52));
            pauseTile.name = "PauseButton";
            pauseTile.transform.Find("Icon").GetComponent<Image>().sprite = FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Pause.png");

            var invBadge = itemsTile.transform.Find("CountBadge/Count").GetComponent<TMP_Text>();

            // [Order 3] Enemy Mark Show Canvas
            var markCanvas = CreateLayeredCanvas("Enemy Mark Show Canvas", allCanvases, (int)UILayer.EdgeMarkers);
            var markInst = SpawnPrefab("03_HUDModules/HUD_EnemyEdgeMark.prefab", markCanvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(460, 0));
            markInst.SetActive(false);

            // [Order 5] Powerup Selection Canvas
            var powerupCanvas = CreateLayeredCanvas("Powerup Selection Canvas", allCanvases, (int)UILayer.PowerupOverlay);
            var powerupInst = SpawnPrefab("04_PopupsAndModals/Modal_PowerupSelection.prefab", powerupCanvas, Vector2.zero, Vector2.one, Vector2.zero);
            FantasyUIPrefabFactory.Stretch((RectTransform)powerupInst.transform);
            powerupInst.SetActive(false);
            var powerupModal = powerupInst.GetComponent<PowerupSelectionModal>();

            // [Order 8] Pop Up Canvas (All Game Modals)
            var popupCanvas = CreateLayeredCanvas("Pop Up Canvas", allCanvases, (int)UILayer.Popups);
            var invModal = SpawnStretchedModal<LeatherInventoryModal>("04_PopupsAndModals/Modal_LeatherInventory.prefab", popupCanvas);
            var discModal = SpawnStretchedModal<DiscoveryPopupModal>("04_PopupsAndModals/Modal_NewDiscovery.prefab", popupCanvas);
            var shipModal = SpawnStretchedModal<ShipUpgradeModal>("04_PopupsAndModals/Modal_ShipUpgrade.prefab", popupCanvas);
            var pauseModal = SpawnStretchedModal<PauseScrollModal>("04_PopupsAndModals/Modal_PauseScroll.prefab", popupCanvas);
            var lvlModal = SpawnStretchedModal<LevelCompleteModal>("04_PopupsAndModals/Modal_LevelComplete.prefab", popupCanvas);

            // [Order 10] Feedback & Banners Canvas
            var fxCanvas = CreateLayeredCanvas("Feedback & Banners Canvas", allCanvases, (int)UILayer.FeedbackBanners);
            var celebInst = SpawnPrefab("06_FeedbackAndFX/FX_CelebrationBanner.prefab", fxCanvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60));
            var celebCtrl = celebInst.GetComponent<CelebrationBannerView>();
            var toastInst = SpawnPrefab("06_FeedbackAndFX/FX_ToastStack.prefab", fxCanvas, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -175));
            var toastCtrl = toastInst.GetComponent<ToastNotificationStack>();

            // [Order 12] Temporary Tools Canvas (FPS Counter + Interactive Demo Sandbox Bar)
            var toolsCanvas = CreateLayeredCanvas("Temporary Tools Canvas", allCanvases, (int)UILayer.SystemDebug);
            SpawnPrefab("02_BarsAndCounters/HUD_FpsCounter.prefab", toolsCanvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(86, -106));
            BuildDemoSandboxBar(toolsCanvas, demoCtrl, theme);

            // Wire Demo Controller references cleanly
            demoCtrl.BindAll(
                channel, themeMgr,
                goldCtrl, gemCtrl, timerTxt, profileHp, versusCtrl, joystick,
                new[] { wPrimary, wSec1, wSec2, wSec3 },
                minimapCtrl, questCtrl, invBadge,
                itemsTile.GetComponent<Button>(), buildTile.GetComponent<Button>(),
                pauseTile.GetComponent<Button>(), modeTile.GetComponent<Button>(),
                pauseModal, lvlModal, invModal, discModal, shipModal, powerupModal,
                celebCtrl, toastCtrl,
                FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Sword.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Bow.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Cannon.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Potion.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Fish.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Boat.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Shield.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Target.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Lightning.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Icon_Gem.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Avatar_Hero1.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Avatar_Hero2.png"),
                FantasyUIPrefabFactory.LoadSprite("Icons/Avatar_Hero3.png"));

            FantasyUIPrefabFactory.ApplyAllSlicedPPUInHierarchy(allCanvases);
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void BuildDemoSandboxBar(RectTransform parent, GameplayUIDemoController demo, UIThemeSO theme)
        {
            var bar = FantasyUIPrefabFactory.CreateUIObject("DemoSandboxToolbar", parent, new Vector2(960, 50));
            bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 1f);
            bar.anchoredPosition = new Vector2(0, -118);
            FantasyUIPrefabFactory.AddImage(bar, FantasyUIPrefabFactory.LoadSprite("Panels/Panel_StoneRecessed.png"), Image.Type.Sliced, new Color(1, 1, 1, 0.92f));

            var hlg = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 6, 6);
            hlg.spacing = 6;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            AddToolbarBtn(bar, "SCROLL",   "Buttons/Btn_Capsule_Stone.png",   demo, nameof(GameplayUIDemoController.OpenPauseModal), theme);
            AddToolbarBtn(bar, "COMPLETE", "Buttons/Btn_Capsule_Emerald.png", demo, nameof(GameplayUIDemoController.OpenLevelCompleteModal), theme);
            AddToolbarBtn(bar, "BAG",      "Buttons/Btn_Capsule_Cobalt.png",  demo, nameof(GameplayUIDemoController.OpenInventoryModal), theme);
            AddToolbarBtn(bar, "LOOT",     "Buttons/Btn_Capsule_Stone.png",   demo, nameof(GameplayUIDemoController.OpenDiscoveryModal), theme);
            AddToolbarBtn(bar, "SHIP",     "Buttons/Btn_Capsule_Stone.png",   demo, nameof(GameplayUIDemoController.OpenShipUpgradeModal), theme);
            AddToolbarBtn(bar, "CARDS",    "Buttons/Btn_Capsule_Stone.png",   demo, nameof(GameplayUIDemoController.OpenPowerupModal), theme);
            AddToolbarBtn(bar, "VICTORY!", "Buttons/Btn_Capsule_Emerald.png", demo, nameof(GameplayUIDemoController.PlayVictoryBanner), theme);
            AddToolbarBtn(bar, "DEFEAT!",  "Buttons/Btn_Capsule_Crimson.png", demo, nameof(GameplayUIDemoController.PlayDefeatedBanner), theme);
            AddToolbarBtn(bar, "THEME",    "Buttons/Btn_Capsule_Cobalt.png",  demo, nameof(GameplayUIDemoController.CycleTheme), theme);
            FantasyUIPrefabFactory.ApplyAllSlicedPPUInHierarchy(bar);
        }

        private static void AddToolbarBtn(RectTransform parent, string label, string sprite, MonoBehaviour target, string methodName, UIThemeSO theme)
        {
            var rt = FantasyUIPrefabFactory.CreateUIObject($"Btn_{label}", parent, new Vector2(90, 32));
            var img = FantasyUIPrefabFactory.AddImage(rt, FantasyUIPrefabFactory.LoadSprite(sprite), Image.Type.Sliced, raycast: true);
            var btn = FantasyUIPrefabFactory.MakeButton(rt.gameObject, img);
            FantasyUIPrefabFactory.AddLabel("Txt", rt, label, 13, theme.textLight, theme);
            var mi = target.GetType().GetMethod(methodName);
            if (mi != null)
            {
                var action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target, mi);
                UnityEventTools.AddPersistentListener(btn.onClick, action);
            }
        }

        // =====================================================================
        // BUILD COMPLETE VISUAL SHOWCASE SCENE (UILibraryShowcase.unity)
        // =====================================================================
        private static void BuildShowcaseScene(UIThemeSO theme, UIThemeSO[] allThemes)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ShowcaseScenePath));
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Showcase Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var sys = new GameObject("Showcase System");
            var tm = sys.AddComponent<UIThemeManager>();
            tm.ConfigureThemes(theme, allThemes);
            var ctrl = sys.AddComponent<UIShowcaseController>();

            var canvas = CreateLayeredCanvas("Fantasy UI Kit Gallery Canvas", null, 0, false);

            // Dusk Sunset Backdrop
            var bg = FantasyUIPrefabFactory.AddImage(
                FantasyUIPrefabFactory.CreateUIObject("DuskBackdrop", canvas, Vector2.zero),
                FantasyUIPrefabFactory.LoadSprite("Panels/Panel_DuskBackdrop.png"));
            FantasyUIPrefabFactory.Stretch(bg.rectTransform);

            // Top-Left Title Crest ("COMPLETE FANTASY GAME UI KIT")
            SpawnPrefab("05_NavigationAndMap/Ornament_TitleCrest.prefab", canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(260, -145));

            // Left Column: Ribbons, Bookmarks, Dual Toggle, Currency & HP/MP Bars
            SpawnPrefab("01_Primitives/Ribbon_Status_Progress.prefab", canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(130, -320));
            SpawnPrefab("01_Primitives/Ribbon_Status_TimeUp.prefab",   canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(130, -380));
            SpawnPrefab("01_Primitives/Ribbon_Status_Collect.prefab",  canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(130, -440));

            SpawnPrefab("01_Primitives/Badge_BookmarkPlus_Crimson.prefab", canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(290, -350));
            SpawnPrefab("01_Primitives/Badge_BookmarkPlus_Emerald.prefab", canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(354, -350));
            SpawnPrefab("01_Primitives/Badge_BookmarkPlus_Purple.prefab",  canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(418, -350));

            SpawnPrefab("01_Primitives/Toggle_DualPill_OnOff.prefab", canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(350, -440));
            SpawnPrefab("02_BarsAndCounters/HUD_GoldBar.prefab",      canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(175, -530));
            SpawnPrefab("02_BarsAndCounters/HUD_GemBar.prefab",       canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(175, -600));
            SpawnPrefab("02_BarsAndCounters/HUD_Healthbar_Emerald.prefab", canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(175, -670));
            SpawnPrefab("02_BarsAndCounters/HUD_Manabar_Crimson.prefab",   canvas, new Vector2(0, 1), new Vector2(0, 1), new Vector2(175, -722));

            // Center Column: Dark Stone Button Stack + Price CTA + Corner Rings + Faceted Close X
            var btnPanel = FantasyUIPrefabFactory.CreateUIObject("ButtonShowcaseStone", canvas, new Vector2(330, 560));
            btnPanel.anchoredPosition = new Vector2(-140, 20);
            FantasyUIPrefabFactory.AddImage(btnPanel, FantasyUIPrefabFactory.LoadSprite("Panels/Panel_StonePlaque.png"), Image.Type.Sliced);
            var bvlg = btnPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            bvlg.padding = new RectOffset(28, 28, 36, 36);
            bvlg.spacing = 14;
            bvlg.childAlignment = TextAnchor.UpperCenter;
            bvlg.childControlWidth = false; bvlg.childControlHeight = false;

            SpawnPrefab("01_Primitives/Btn_IronSlab_Purple.prefab",   btnPanel, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero);
            SpawnPrefab("01_Primitives/Btn_IronSlab_Cobalt.prefab",   btnPanel, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero);
            SpawnPrefab("01_Primitives/Btn_IronSlab_Emerald.prefab",  btnPanel, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero);
            SpawnPrefab("01_Primitives/Btn_IronSlab_Crimson.prefab",  btnPanel, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero);
            SpawnPrefab("01_Primitives/Btn_PriceAction_Emerald.prefab", btnPanel, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero);

            // Right Area: Interactive Saga Level Progression Map (Ref 2)
            var mapInst = SpawnPrefab("05_NavigationAndMap/Screen_SagaLevelMap.prefab", canvas, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-410, 40));
            var sagaMap = mapInst.GetComponent<SagaLevelMapController>();

            // Bottom Versus Dock
            SpawnPrefab("02_BarsAndCounters/HUD_DualPlayerVersusDock.prefab", canvas, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 62));

            // Interactive Showcase Action Buttons (Top-Right)
            var topBar = FantasyUIPrefabFactory.CreateUIObject("ShowcaseControls", canvas, new Vector2(440, 56));
            topBar.anchorMin = topBar.anchorMax = new Vector2(1, 1);
            topBar.anchoredPosition = new Vector2(-248, -52);
            var thlg = topBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            thlg.spacing = 12; thlg.childControlWidth = true; thlg.childControlHeight = true;
            AddToolbarBtn(topBar, "CYCLE THEME", "Buttons/Btn_Capsule_Cobalt.png", ctrl, nameof(UIShowcaseController.CycleTheme), theme);
            AddToolbarBtn(topBar, "NEXT BANNER", "Buttons/Btn_Capsule_Emerald.png", ctrl, nameof(UIShowcaseController.TriggerNextBanner), theme);

            var bannerInst = SpawnPrefab("06_FeedbackAndFX/FX_CelebrationBanner.prefab", canvas, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -140));
            ctrl.BindReferences(sagaMap, bannerInst.GetComponent<CelebrationBannerView>(), tm);

            FantasyUIPrefabFactory.ApplyAllSlicedPPUInHierarchy(canvas);
            EditorSceneManager.SaveScene(scene, ShowcaseScenePath);
        }

        // =====================================================================
        // HELPERS
        // =====================================================================
        private static RectTransform CreateLayeredCanvas(string name, Transform parent, int sortingOrder, bool addSafeArea = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = LayerMask.NameToLayer("UI");
            if (parent != null) go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            if (!addSafeArea)
                return (RectTransform)go.transform;

            var safeRt = FantasyUIPrefabFactory.CreateUIObject("SafeAreaRoot", go.transform, Vector2.zero);
            FantasyUIPrefabFactory.Stretch(safeRt);
            safeRt.gameObject.AddComponent<SafeAreaFitter>();
            return safeRt;
        }

        private static GameObject SpawnPrefab(string relPrefabPath, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(FantasyUIPrefabFactory.PrefabsPath + relPrefabPath);
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            var rt = (RectTransform)inst.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            return inst;
        }

        private static T SpawnStretchedModal<T>(string relPrefabPath, Transform parent) where T : UIModal
        {
            var inst = SpawnPrefab(relPrefabPath, parent, Vector2.zero, Vector2.one, Vector2.zero);
            FantasyUIPrefabFactory.Stretch((RectTransform)inst.transform);
            inst.SetActive(false);
            return inst.GetComponent<T>();
        }
    }
}
