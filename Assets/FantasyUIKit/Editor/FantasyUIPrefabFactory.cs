using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Animation;
using FantasyUI.Components;
using FantasyUI.Core;
using FantasyUI.Data;
using FantasyUI.Theme;

namespace FantasyUI.Editor
{
    /// <summary>
    /// Modular factory constructing all 35+ game-ready Dark-Fantasy UI prefabs matching Reference Images 1-4.
    /// </summary>
    public static class FantasyUIPrefabFactory
    {
        public const string RootPath = "Assets/FantasyUIKit";
        public const string SpritesPath = RootPath + "/Art/Sprites/";
        public const string PrefabsPath = RootPath + "/Prefabs/";

        public static Sprite LoadSprite(string relPath)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritesPath + relPath);
        }

        public static GameObject SavePrefab(GameObject root, string relativePrefabPath)
        {
            string fullPath = PrefabsPath + relativePrefabPath;
            string dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            ApplyAllSlicedPPUInHierarchy(root.transform);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, fullPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        public static RectTransform CreateUIObject(string name, Transform parent, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = size;
            return rt;
        }

        public static void Stretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        public static Image AddImage(RectTransform rt, Sprite sprite, Image.Type type = Image.Type.Simple, Color? color = null, bool raycast = false)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.type = type;
            img.color = color ?? Color.white;
            img.raycastTarget = raycast;

            if (type == Image.Type.Sliced && sprite != null)
            {
                Vector2 sz = UI9SliceAutoPPU.GetEffectiveSize(rt);
                if (sz.x > 1f && sz.y > 1f)
                    img.pixelsPerUnitMultiplier = UI9SliceAutoPPU.ComputeOptimalMultiplier(sprite, sz);
                var scaler = rt.gameObject.GetComponent<UI9SliceAutoPPU>() ?? rt.gameObject.AddComponent<UI9SliceAutoPPU>();
                scaler.ApplyNow();
            }
            else if (type == Image.Type.Simple && sprite != null)
            {
                img.preserveAspect = false;
            }
            return img;
        }

        public static void ApplyAllSlicedPPUInHierarchy(Transform root)
        {
            if (root == null) return;
            Canvas.ForceUpdateCanvases();
            foreach (var lg in root.GetComponentsInChildren<LayoutGroup>(true))
            {
                if (lg.transform is RectTransform lrt)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(lrt);
            }

            foreach (var img in root.GetComponentsInChildren<Image>(true))
            {
                if (img.sprite == null) continue;
                bool hasBorder = img.sprite.border.sqrMagnitude > 0.01f;
                if (img.type == Image.Type.Sliced && hasBorder)
                {
                    var scaler = img.GetComponent<UI9SliceAutoPPU>() ?? img.gameObject.AddComponent<UI9SliceAutoPPU>();
                    scaler.ApplyNow();
                }
            }
        }

        public static TMP_Text AddLabel(string name, Transform parent, string text, float fontSize, Color color,
            UIThemeSO theme, TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = true, bool bindOutline = true)
        {
            var rt = CreateUIObject(name, parent, Vector2.zero);
            Stretch(rt);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            tmp.fontStyle = bold ? FontStyles.Bold | FontStyles.UpperCase : FontStyles.Normal;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            if (theme != null && theme.displayFont != null)
                tmp.font = theme.displayFont;
            if (bindOutline && theme != null && theme.displayOutlineMaterial != null)
                tmp.fontSharedMaterial = theme.displayOutlineMaterial;
            return tmp;
        }

        public static Button MakeButton(GameObject go, Image targetGraphic, bool addJuice = true)
        {
            if (targetGraphic != null) targetGraphic.raycastTarget = true;
            var btn = go.GetComponent<Button>() ?? go.AddComponent<Button>();
            btn.targetGraphic = targetGraphic;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.7f);
            btn.colors = colors;
            if (addJuice && go.GetComponent<UIButtonJuice>() == null)
                go.AddComponent<UIButtonJuice>();
            return btn;
        }

        // =====================================================================
        // 01_PRIMITIVES
        // =====================================================================
        public static void BuildAllPrimitives(UIThemeSO theme)
        {
            BuildIronSlabButton("Emerald", "PLAY", theme);
            BuildIronSlabButton("Crimson", "QUIT GAME", theme);
            BuildIronSlabButton("Purple", "SETTINGS", theme);
            BuildIronSlabButton("Cobalt", "SHARE", theme);
            BuildIronSlabButton("Stone", "CONTINUE", theme);

            BuildPriceActionButton(theme);
            BuildCircleRingButton("Back", "Buttons/Btn_CircleRing_Emerald.png", "Icons/Icon_ArrowLeft.png", theme);
            BuildCircleRingButton("Close", "Buttons/Btn_CircleRing_Crimson.png", "Icons/Icon_CloseX.png", theme);
            BuildFacetedCloseXButton(theme);
            BuildQuickTileButton("Build", "BUILD", "Icons/Icon_Hammer.png", false, theme);
            BuildQuickTileButton("Items", "ITEMS", "Icons/Icon_Backpack.png", true, theme);
            BuildQuickTileButton("Home", "", "Icons/Icon_House.png", false, theme);
            BuildSocialConnectButton(theme);

            BuildBookmarkPlusBadge("Purple", "Buttons/Btn_Bookmark_Purple.png");
            BuildBookmarkPlusBadge("Emerald", "Buttons/Btn_Bookmark_Emerald.png");
            BuildBookmarkPlusBadge("Crimson", "Buttons/Btn_Bookmark_Crimson.png");

            BuildDualPillToggle(theme);
            BuildStatusRibbon("Progress", "PROGRESS!", "Ribbons/Ribbon_Status_Purple.png", theme);
            BuildStatusRibbon("TimeUp", "TIME IS UP!", "Ribbons/Ribbon_Status_Crimson.png", theme);
            BuildStatusRibbon("Collect", "COLLECT NOW!", "Ribbons/Ribbon_Status_Emerald.png", theme);
        }

        private static GameObject BuildIronSlabButton(string variant, string defaultLabel, UIThemeSO theme)
        {
            var rt = CreateUIObject($"Btn_IronSlab_{variant}", null, new Vector2(260, 76));
            var bg = AddImage(rt, LoadSprite($"Buttons/Btn_IronSlab_{variant}.png"), Image.Type.Sliced, raycast: true);
            var btn = MakeButton(rt.gameObject, bg);
            var lbl = AddLabel("Label", rt, defaultLabel, 28, theme.textLight, theme);
            lbl.rectTransform.offsetMin = new Vector2(46, 6);
            lbl.rectTransform.offsetMax = new Vector2(-46, -6);
            var fb = rt.gameObject.AddComponent<FantasyButton>();
            fb.BindReferences(btn, lbl, bg);
            return SavePrefab(rt.gameObject, $"01_Primitives/Btn_IronSlab_{variant}.prefab");
        }

        private static GameObject BuildPriceActionButton(UIThemeSO theme)
        {
            var rt = CreateUIObject("Btn_PriceAction_Emerald", null, new Vector2(280, 78));
            var bg = AddImage(rt, LoadSprite("Buttons/Btn_Capsule_Emerald.png"), Image.Type.Sliced, raycast: true);
            var btn = MakeButton(rt.gameObject, bg);

            var pillRt = CreateUIObject("PricePill", rt, new Vector2(126, 42));
            pillRt.anchorMin = pillRt.anchorMax = new Vector2(0f, 0.5f);
            pillRt.pivot = new Vector2(0f, 0.5f);
            pillRt.anchoredPosition = new Vector2(18, 0);
            AddImage(pillRt, LoadSprite("Buttons/Btn_Capsule_Cream.png"), Image.Type.Sliced);

            var coinRt = CreateUIObject("CoinIcon", pillRt, new Vector2(42, 42));
            coinRt.anchorMin = coinRt.anchorMax = new Vector2(0f, 0.5f);
            coinRt.anchoredPosition = new Vector2(12, 0);
            var icon = AddImage(coinRt, LoadSprite("Icons/Icon_Coin.png"));

            var priceTxt = AddLabel("PriceText", pillRt, "10,500", 20, theme.textDark, theme, TextAlignmentOptions.Center, true, false);
            priceTxt.rectTransform.offsetMin = new Vector2(32, 0);

            var actionTxt = AddLabel("ActionLabel", rt, "GET IT!", 26, theme.textLight, theme);
            actionTxt.rectTransform.anchorMin = new Vector2(0.52f, 0f);
            actionTxt.rectTransform.anchorMax = new Vector2(0.96f, 1f);

            var fb = rt.gameObject.AddComponent<FantasyButton>();
            fb.BindReferences(btn, actionTxt, bg, icon, priceTxt, pillRt.gameObject);
            return SavePrefab(rt.gameObject, "01_Primitives/Btn_PriceAction_Emerald.prefab");
        }

        private static GameObject BuildCircleRingButton(string suffix, string ringSprite, string iconSprite, UIThemeSO theme)
        {
            var rt = CreateUIObject($"Btn_CircleRing_{suffix}", null, new Vector2(82, 82));
            var bg = AddImage(rt, LoadSprite(ringSprite), Image.Type.Simple, raycast: true);
            var btn = MakeButton(rt.gameObject, bg);
            var icRt = CreateUIObject("Icon", rt, new Vector2(38, 38));
            var ic = AddImage(icRt, LoadSprite(iconSprite));
            var fb = rt.gameObject.AddComponent<FantasyButton>();
            fb.BindReferences(btn, null, bg, ic);
            return SavePrefab(rt.gameObject, $"01_Primitives/Btn_CircleRing_{suffix}.prefab");
        }

        private static GameObject BuildFacetedCloseXButton(UIThemeSO theme)
        {
            var rt = CreateUIObject("Btn_FacetedCloseX", null, new Vector2(80, 80));
            var bg = AddImage(rt, LoadSprite("Buttons/Btn_FacetedCloseX.png"), Image.Type.Simple, raycast: true);
            var btn = MakeButton(rt.gameObject, bg);
            var fb = rt.gameObject.AddComponent<FantasyButton>();
            fb.BindReferences(btn, null, bg);
            return SavePrefab(rt.gameObject, "01_Primitives/Btn_FacetedCloseX.prefab");
        }

        private static GameObject BuildQuickTileButton(string suffix, string topLabel, string iconPath, bool withBadge, UIThemeSO theme)
        {
            var rt = CreateUIObject($"Btn_QuickTile_{suffix}", null, new Vector2(104, 104));
            var bg = AddImage(rt, LoadSprite("Buttons/Btn_QuickTile_Stone.png"), Image.Type.Sliced, raycast: true);
            var btn = MakeButton(rt.gameObject, bg);

            // Thorny bramble corner ornament
            var brRt = CreateUIObject("Bramble", rt, new Vector2(56, 56));
            brRt.anchorMin = brRt.anchorMax = new Vector2(0f, 1f);
            brRt.anchoredPosition = new Vector2(18, -18);
            AddImage(brRt, LoadSprite("Ornaments/Orn_ThornyBramble.png"));

            var icRt = CreateUIObject("Icon", rt, new Vector2(62, 62));
            icRt.anchoredPosition = new Vector2(0, -2);
            var ic = AddImage(icRt, LoadSprite(iconPath));

            TMP_Text title = null;
            if (!string.IsNullOrEmpty(topLabel))
            {
                title = AddLabel("TopLabel", rt, topLabel, 20, theme.textLight, theme);
                title.rectTransform.anchorMin = new Vector2(0f, 0.88f);
                title.rectTransform.anchorMax = new Vector2(1f, 1.24f);
            }

            TMP_Text badgeTxt = null;
            GameObject badgeRoot = null;
            if (withBadge)
            {
                var bRt = CreateUIObject("CountBadge", rt, new Vector2(74, 30));
                bRt.anchorMin = bRt.anchorMax = new Vector2(1f, 0f);
                bRt.anchoredPosition = new Vector2(-14, 10);
                AddImage(bRt, LoadSprite("Buttons/Btn_Capsule_Cream.png"), Image.Type.Sliced);
                badgeTxt = AddLabel("Count", bRt, "0/20", 15, theme.textDark, theme, TextAlignmentOptions.Center, true, false);
                badgeRoot = bRt.gameObject;
            }

            var fb = rt.gameObject.AddComponent<FantasyButton>();
            fb.BindReferences(btn, title, bg, ic, badgeTxt, badgeRoot);
            return SavePrefab(rt.gameObject, $"01_Primitives/Btn_QuickTile_{suffix}.prefab");
        }

        private static GameObject BuildSocialConnectButton(UIThemeSO theme)
        {
            var rt = CreateUIObject("Btn_SocialConnect", null, new Vector2(210, 58));
            var bg = AddImage(rt, LoadSprite("Buttons/Btn_Capsule_Cobalt.png"), Image.Type.Sliced, raycast: true);
            var btn = MakeButton(rt.gameObject, bg);
            var lbl = AddLabel("Label", rt, "CONNECT", 22, theme.textLight, theme);
            var fb = rt.gameObject.AddComponent<FantasyButton>();
            fb.BindReferences(btn, lbl, bg);
            return SavePrefab(rt.gameObject, "01_Primitives/Btn_SocialConnect.prefab");
        }

        private static GameObject BuildBookmarkPlusBadge(string variant, string spritePath)
        {
            var rt = CreateUIObject($"Badge_BookmarkPlus_{variant}", null, new Vector2(54, 66));
            var bg = AddImage(rt, LoadSprite(spritePath), Image.Type.Simple, raycast: true);
            MakeButton(rt.gameObject, bg);
            return SavePrefab(rt.gameObject, $"01_Primitives/Badge_BookmarkPlus_{variant}.prefab");
        }

        private static GameObject BuildDualPillToggle(UIThemeSO theme)
        {
            var rt = CreateUIObject("Toggle_DualPill_OnOff", null, new Vector2(210, 60));
            var bg = AddImage(rt, LoadSprite("Bars/Toggle_ParchmentTrack.png"), Image.Type.Sliced, raycast: true);
            var btn = MakeButton(rt.gameObject, bg, addJuice: false);

            var onLbl = AddLabel("OnLabel", rt, "ON", 20, theme.textDark, theme, TextAlignmentOptions.Center, true, false);
            onLbl.rectTransform.anchorMin = new Vector2(0f, 0f); onLbl.rectTransform.anchorMax = new Vector2(0.26f, 1f);

            var onOrbRt = CreateUIObject("OnOrb", rt, new Vector2(46, 46));
            onOrbRt.anchoredPosition = new Vector2(-24, 0);
            var onOrb = AddImage(onOrbRt, LoadSprite("Bars/Toggle_Orb_Emerald.png"));

            var offOrbRt = CreateUIObject("OffOrb", rt, new Vector2(46, 46));
            offOrbRt.anchoredPosition = new Vector2(24, 0);
            var offOrb = AddImage(offOrbRt, LoadSprite("Bars/Toggle_Orb_Crimson.png"));

            var offLbl = AddLabel("OffLabel", rt, "OFF", 20, theme.textDark, theme, TextAlignmentOptions.Center, true, false);
            offLbl.rectTransform.anchorMin = new Vector2(0.74f, 0f); offLbl.rectTransform.anchorMax = new Vector2(1f, 1f);

            var toggle = rt.gameObject.AddComponent<DualPillToggle>();
            toggle.BindReferences(btn, onOrb, offOrb, onLbl, offLbl, true);
            return SavePrefab(rt.gameObject, "01_Primitives/Toggle_DualPill_OnOff.prefab");
        }

        private static GameObject BuildStatusRibbon(string suffix, string text, string spritePath, UIThemeSO theme)
        {
            var rt = CreateUIObject($"Ribbon_Status_{suffix}", null, new Vector2(188, 48));
            AddImage(rt, LoadSprite(spritePath), Image.Type.Sliced);
            var lbl = AddLabel("Label", rt, text, 20, theme.textLight, theme);
            lbl.rectTransform.offsetMin = new Vector2(22, 4);
            lbl.rectTransform.offsetMax = new Vector2(-14, -4);
            return SavePrefab(rt.gameObject, $"01_Primitives/Ribbon_Status_{suffix}.prefab");
        }

        // =====================================================================
        // 02_BARS & COUNTERS
        // =====================================================================
        public static void BuildAllBarsAndCounters(UIThemeSO theme, UIEventChannels channel)
        {
            BuildCurrencyBar("Gold", CurrencyKind.Gold, "Icons/Icon_Coin.png", "Bars/Bar_Fill_Emerald.png", false, "19,456,000", theme, channel);
            BuildCurrencyBar("Gem", CurrencyKind.Gems, "Icons/Icon_Gem.png", null, true, "19,456", theme, channel);
            BuildTimerPill(theme);
            BuildStandaloneProgressBar("Healthbar_Emerald", "Bars/Bar_Fill_Emerald.png", "2,000/5,000 HP", " HP", theme);
            BuildStandaloneProgressBar("Manabar_Crimson", "Bars/Bar_Fill_Crimson.png", "19,456,000 MP", " MP", theme);
            BuildPlayerProfileWidget(theme);
            BuildDualPlayerVersusDock(theme, channel);
            BuildFpsCounter(theme);
        }

        private static GameObject BuildCurrencyBar(string name, CurrencyKind kind, string iconPath, string fillPath, bool withPlusTag, string sample, UIThemeSO theme, UIEventChannels channel)
        {
            var rt = CreateUIObject($"HUD_{name}Bar", null, new Vector2(248, 56));
            AddImage(rt, LoadSprite("Bars/Bar_StoneFrame.png"), Image.Type.Sliced);

            if (!string.IsNullOrEmpty(fillPath))
            {
                var fillRt = CreateUIObject("Fill", rt, Vector2.zero);
                Stretch(fillRt, 22, 13, 20, 13);
                AddImage(fillRt, LoadSprite(fillPath), Image.Type.Sliced);
            }

            var icRt = CreateUIObject("Icon", rt, new Vector2(62, 62));
            icRt.anchorMin = icRt.anchorMax = new Vector2(0f, 0.5f);
            icRt.anchoredPosition = new Vector2(8, 0);
            AddImage(icRt, LoadSprite(iconPath));

            var txt = AddLabel("AmountText", rt, sample, 22, theme.textLight, theme, TextAlignmentOptions.Center);
            txt.rectTransform.offsetMin = new Vector2(44, 4);
            txt.rectTransform.offsetMax = new Vector2(withPlusTag ? -44 : -14, -4);
            var roller = txt.gameObject.AddComponent<UINumberRoller>();

            Button plusBtn = null;
            if (withPlusTag)
            {
                var pRt = CreateUIObject("PlusBookmark", rt, new Vector2(48, 58));
                pRt.anchorMin = pRt.anchorMax = new Vector2(1f, 0.5f);
                pRt.anchoredPosition = new Vector2(-14, -4);
                var pImg = AddImage(pRt, LoadSprite("Buttons/Btn_Bookmark_Purple.png"), Image.Type.Simple, raycast: true);
                plusBtn = MakeButton(pRt.gameObject, pImg);
            }

            var view = rt.gameObject.AddComponent<CurrencyCounterView>();
            view.BindReferences(kind, roller, plusBtn, channel);
            return SavePrefab(rt.gameObject, $"02_BarsAndCounters/HUD_{name}Bar.prefab");
        }

        private static GameObject BuildTimerPill(UIThemeSO theme)
        {
            var rt = CreateUIObject("HUD_TimerPill", null, new Vector2(158, 52));
            AddImage(rt, LoadSprite("Bars/Bar_StoneFrame.png"), Image.Type.Sliced);
            var icRt = CreateUIObject("LightningIcon", rt, new Vector2(44, 44));
            icRt.anchorMin = icRt.anchorMax = new Vector2(0f, 0.5f);
            icRt.anchoredPosition = new Vector2(14, 0);
            AddImage(icRt, LoadSprite("Icons/Icon_Lightning.png"));
            var txt = AddLabel("TimerText", rt, "10:56", 24, theme.textLight, theme);
            txt.rectTransform.offsetMin = new Vector2(36, 4);
            return SavePrefab(rt.gameObject, "02_BarsAndCounters/HUD_TimerPill.prefab");
        }

        private static GameObject BuildStandaloneProgressBar(string name, string fillSprite, string sample, string suffix, UIThemeSO theme)
        {
            var rt = CreateUIObject($"HUD_{name}", null, new Vector2(236, 36));
            AddImage(rt, LoadSprite("Bars/Bar_StoneFrame.png"), Image.Type.Sliced);

            var fillArea = CreateUIObject("FillArea", rt, Vector2.zero);
            Stretch(fillArea, 12, 8, 12, 8);

            var trailRt = CreateUIObject("Trail", fillArea, Vector2.zero);
            Stretch(trailRt);
            var trailImg = AddImage(trailRt, LoadSprite("Bars/Bar_Fill_Gold.png"), Image.Type.Sliced, new Color(1f, 0.9f, 0.5f, 0.65f));

            var fillRt = CreateUIObject("Fill", fillArea, Vector2.zero);
            Stretch(fillRt);
            var fillImg = AddImage(fillRt, LoadSprite(fillSprite), Image.Type.Sliced);

            var lbl = AddLabel("ValueLabel", rt, sample, 17, theme.textLight, theme);
            var bar = rt.gameObject.AddComponent<FantasyProgressBar>();
            bar.BindReferences(fillImg, lbl, trailImg, suffix, suffix == " HP");
            return SavePrefab(rt.gameObject, $"02_BarsAndCounters/HUD_{name}.prefab");
        }

        private static GameObject BuildPlayerProfileWidget(UIThemeSO theme)
        {
            var rt = CreateUIObject("HUD_PlayerProfile", null, new Vector2(310, 92));
            AddImage(rt, LoadSprite("Panels/Panel_StoneRecessed.png"), Image.Type.Sliced);

            var avFrame = CreateUIObject("AvatarFrame", rt, new Vector2(82, 82));
            avFrame.anchorMin = avFrame.anchorMax = new Vector2(0f, 0.5f);
            avFrame.anchoredPosition = new Vector2(46, 0);
            AddImage(avFrame, LoadSprite("Slots/Slot_RivetedIron.png"), Image.Type.Sliced);
            var avRt = CreateUIObject("Avatar", avFrame, new Vector2(58, 58));
            AddImage(avRt, LoadSprite("Icons/Avatar_Hero1.png"));

            var nameTxt = AddLabel("NameText", rt, "RED CHAP", 22, theme.textGold, theme, TextAlignmentOptions.Left);
            nameTxt.rectTransform.anchorMin = new Vector2(0f, 0.52f);
            nameTxt.rectTransform.anchorMax = new Vector2(1f, 0.92f);
            nameTxt.rectTransform.offsetMin = new Vector2(96, 0);
            nameTxt.rectTransform.offsetMax = new Vector2(-14, 0);

            var hpRt = CreateUIObject("HealthBar", rt, new Vector2(196, 30));
            hpRt.anchorMin = hpRt.anchorMax = new Vector2(0f, 0.28f);
            hpRt.pivot = new Vector2(0f, 0.5f);
            hpRt.anchoredPosition = new Vector2(96, 0);
            AddImage(hpRt, LoadSprite("Bars/Bar_StoneFrame.png"), Image.Type.Sliced);
            var fillArea = CreateUIObject("FillArea", hpRt, Vector2.zero);
            Stretch(fillArea, 10, 6, 10, 6);
            var fillRt = CreateUIObject("Fill", fillArea, Vector2.zero);
            Stretch(fillRt);
            var fillImg = AddImage(fillRt, LoadSprite("Bars/Bar_Fill_Emerald.png"), Image.Type.Sliced);
            var hpLbl = AddLabel("ValueLabel", hpRt, "2,000/5,000 HP", 14, theme.textLight, theme);
            var bar = hpRt.gameObject.AddComponent<FantasyProgressBar>();
            bar.BindReferences(fillImg, hpLbl, null, " HP", true);

            return SavePrefab(rt.gameObject, "02_BarsAndCounters/HUD_PlayerProfile.prefab");
        }

        private static GameObject BuildDualPlayerVersusDock(UIThemeSO theme, UIEventChannels channel)
        {
            var rt = CreateUIObject("HUD_DualPlayerVersusDock", null, new Vector2(1160, 128));
            var bg = AddImage(rt, LoadSprite("Panels/Panel_VersusDock.png"), Image.Type.Sliced);
            bg.gameObject.AddComponent<UIThemeBinder>().Configure(UIThemeColorToken.PanelTint);

            // Center Crowned Skull VS Emblem
            var vsRt = CreateUIObject("CrownedSkullVS", rt, new Vector2(150, 130));
            vsRt.anchorMin = vsRt.anchorMax = new Vector2(0.5f, 0.5f);
            vsRt.anchoredPosition = new Vector2(0, 18);
            AddImage(vsRt, LoadSprite("Ornaments/Orn_CrownedSkullVS.png"));
            var vsTxt = AddLabel("VSText", vsRt, "VS", 22, theme.textGold, theme);
            vsTxt.rectTransform.anchoredPosition = new Vector2(0, 14);

            // Left Player Cluster
            var (lName, lAv, lHp, lMp) = BuildVersusPlayerSide(rt, true, "PLAYER12", "Icons/Avatar_Hero1.png", theme);
            // Right Player Cluster
            var (rName, rAv, rHp, rMp) = BuildVersusPlayerSide(rt, false, "PLAYER144", "Icons/Avatar_Hero2.png", theme);

            var comp = rt.gameObject.AddComponent<DualPlayerVersusBar>();
            comp.BindReferences(lName, lAv, lHp, lMp, rName, rAv, rHp, rMp, channel);
            return SavePrefab(rt.gameObject, "02_BarsAndCounters/HUD_DualPlayerVersusDock.prefab");
        }

        private static (TMP_Text, Image, FantasyProgressBar, FantasyProgressBar) BuildVersusPlayerSide(
            RectTransform parent, bool isLeft, string playerName, string avatarPath, UIThemeSO theme)
        {
            float sign = isLeft ? -1f : 1f;
            var side = CreateUIObject(isLeft ? "LeftPlayer" : "RightPlayer", parent, new Vector2(390, 108));
            side.anchorMin = side.anchorMax = new Vector2(isLeft ? 0f : 1f, 0.5f);
            side.anchoredPosition = new Vector2(-sign * 215f, -4f);

            var avFrame = CreateUIObject("PortraitFrame", side, new Vector2(84, 84));
            avFrame.anchorMin = avFrame.anchorMax = new Vector2(isLeft ? 0f : 1f, 0.5f);
            avFrame.anchoredPosition = new Vector2(-sign * 46f, -4f);
            AddImage(avFrame, LoadSprite("Slots/Slot_RivetedIron.png"), Image.Type.Sliced);
            var avRt = CreateUIObject("Avatar", avFrame, new Vector2(60, 60));
            var avImg = AddImage(avRt, LoadSprite(avatarPath));

            var nameTxt = AddLabel("PlayerName", side, playerName, 21, theme.textGold, theme, isLeft ? TextAlignmentOptions.Left : TextAlignmentOptions.Right);
            nameTxt.rectTransform.anchorMin = new Vector2(0f, 0.86f);
            nameTxt.rectTransform.anchorMax = new Vector2(1f, 1.22f);
            nameTxt.rectTransform.offsetMin = new Vector2(isLeft ? 98 : 10, 0);
            nameTxt.rectTransform.offsetMax = new Vector2(isLeft ? -10 : -98, 0);

            var hpBar = CreateInlineBar(side, "HPBar", new Vector2(isLeft ? 48f : -48f, 12f), "Bars/Bar_Fill_Emerald.png", "2,000/5,000 HP", " HP", true, theme);
            var mpBar = CreateInlineBar(side, "MPBar", new Vector2(isLeft ? 48f : -48f, -26f), "Bars/Bar_Fill_Crimson.png", "19,456,000 MP", " MP", false, theme);
            return (nameTxt, avImg, hpBar, mpBar);
        }

        private static FantasyProgressBar CreateInlineBar(RectTransform parent, string name, Vector2 pos, string fillSprite, string sample, string suffix, bool fraction, UIThemeSO theme)
        {
            var rt = CreateUIObject(name, parent, new Vector2(276, 34));
            rt.anchoredPosition = pos;
            AddImage(rt, LoadSprite("Bars/Bar_StoneFrame.png"), Image.Type.Sliced);
            var fa = CreateUIObject("FillArea", rt, Vector2.zero);
            Stretch(fa, 10, 6, 10, 6);
            var fillRt = CreateUIObject("Fill", fa, Vector2.zero);
            Stretch(fillRt);
            var fillImg = AddImage(fillRt, LoadSprite(fillSprite), Image.Type.Sliced);
            var lbl = AddLabel("Label", rt, sample, 14, theme.textLight, theme);
            var bar = rt.gameObject.AddComponent<FantasyProgressBar>();
            bar.BindReferences(fillImg, lbl, null, suffix, fraction);
            return bar;
        }

        private static GameObject BuildFpsCounter(UIThemeSO theme)
        {
            var rt = CreateUIObject("HUD_FpsCounter", null, new Vector2(132, 38));
            AddImage(rt, LoadSprite("Panels/Panel_StoneRecessed.png"), Image.Type.Sliced);
            var txt = AddLabel("FPSText", rt, "FPS: 60", 18, theme.emeraldPrimary, theme);
            var fps = rt.gameObject.AddComponent<FpsCounterView>();
            fps.BindReferences(txt);
            return SavePrefab(rt.gameObject, "02_BarsAndCounters/HUD_FpsCounter.prefab");
        }

        // =====================================================================
        // 03_HUD MODULES
        // =====================================================================
        public static void BuildAllHudModules(UIThemeSO theme)
        {
            BuildVirtualJoystick();
            BuildWeaponSlot("Primary", 142f, "CANNON", "Icons/Icon_Cannon.png", theme);
            BuildWeaponSlot("Secondary", 104f, "SLASH", "Icons/Icon_Sword.png", theme);
            BuildCircularMinimap(theme);
            BuildQuestRail(theme);
            BuildFloatingHealthbar(theme);
            BuildEnemyEdgeMark();
        }

        private static GameObject BuildVirtualJoystick()
        {
            var rt = CreateUIObject("HUD_VirtualJoystick", null, new Vector2(220, 220));
            var bgImg = AddImage(rt, LoadSprite("Slots/Frame_JoystickBase.png"), Image.Type.Simple, raycast: true);
            var hdlRt = CreateUIObject("HelmHandle", rt, new Vector2(106, 106));
            AddImage(hdlRt, LoadSprite("Slots/Frame_JoystickHelm.png"));
            var joy = rt.gameObject.AddComponent<VirtualJoystick>();
            joy.BindReferences(rt, hdlRt, 66f);
            return SavePrefab(rt.gameObject, "03_HUDModules/HUD_VirtualJoystick.prefab");
        }

        private static GameObject BuildWeaponSlot(string suffix, float size, string label, string iconPath, UIThemeSO theme)
        {
            var rt = CreateUIObject($"HUD_WeaponSlot_{suffix}", null, new Vector2(size, size));
            var ring = AddImage(rt, LoadSprite("Slots/Frame_WeaponRing.png"), Image.Type.Simple, raycast: true);
            var btn = MakeButton(rt.gameObject, ring);

            var icRt = CreateUIObject("Icon", rt, new Vector2(size * 0.56f, size * 0.56f));
            var icImg = AddImage(icRt, LoadSprite(iconPath));

            var cdRt = CreateUIObject("CooldownOverlay", rt, new Vector2(size * 0.80f, size * 0.80f));
            var cdImg = AddImage(cdRt, LoadSprite("FX/FX_RadialCircle.png"), Image.Type.Filled, new Color(0.08f, 0.05f, 0.11f, 0.72f));
            cdImg.fillMethod = Image.FillMethod.Radial360;
            cdImg.fillOrigin = (int)Image.Origin360.Top;
            cdImg.fillClockwise = true;
            cdImg.fillAmount = 0f;

            var tagRt = CreateUIObject("NameTag", rt, new Vector2(size * 0.92f, 28f));
            tagRt.anchorMin = tagRt.anchorMax = new Vector2(0.5f, 0f);
            tagRt.anchoredPosition = new Vector2(0, 8);
            AddImage(tagRt, LoadSprite("Buttons/Btn_Capsule_Stone.png"), Image.Type.Sliced);
            var nameLbl = AddLabel("NameLabel", tagRt, label, 15, theme.textGold, theme);

            var badgeRt = CreateUIObject("CooldownBadge", rt, new Vector2(58, 30));
            AddImage(badgeRt, LoadSprite("Buttons/Btn_Capsule_Crimson.png"), Image.Type.Sliced);
            var timerLbl = AddLabel("TimerLabel", badgeRt, "2.0", 17, theme.textLight, theme);
            badgeRt.gameObject.SetActive(false);

            var slot = rt.gameObject.AddComponent<WeaponSlotView>();
            slot.BindReferences(btn, icImg, cdImg, badgeRt.gameObject, timerLbl, nameLbl);
            return SavePrefab(rt.gameObject, $"03_HUDModules/HUD_WeaponSlot_{suffix}.prefab");
        }

        private static GameObject BuildCircularMinimap(UIThemeSO theme)
        {
            var rt = CreateUIObject("HUD_CircularMinimap", null, new Vector2(226, 226));

            // Masked inner circle
            var maskRt = CreateUIObject("MapMask", rt, new Vector2(182, 182));
            var maskImg = AddImage(maskRt, LoadSprite("Slots/Frame_MinimapMask.png"));
            var mask = maskRt.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var mapRt = CreateUIObject("MapImage", maskRt, new Vector2(210, 210));
            AddImage(mapRt, LoadSprite("FX/Map_FantasyWorld.png"));

            var markersRt = CreateUIObject("MapMarkers", maskRt, Vector2.zero);
            Stretch(markersRt);

            var arrowRt = CreateUIObject("PlayerArrow", maskRt, new Vector2(28, 28));
            AddImage(arrowRt, LoadSprite("Icons/Icon_PlayerArrow.png"));

            // Carved iron outer frame
            var frameRt = CreateUIObject("OuterFrame", rt, new Vector2(226, 226));
            AddImage(frameRt, LoadSprite("Slots/Frame_MinimapOuter.png"));

            var compassRt = CreateUIObject("CompassN", frameRt, new Vector2(40, 40));
            compassRt.anchorMin = compassRt.anchorMax = new Vector2(0.5f, 1f);
            compassRt.anchoredPosition = new Vector2(0, -6);
            AddImage(compassRt, LoadSprite("Slots/Frame_CompassN.png"));

            var edgeRt = CreateUIObject("EdgeMarkers", rt, Vector2.zero);
            Stretch(edgeRt);

            var ctrl = rt.gameObject.AddComponent<MinimapController>();
            ctrl.BindReferences(markersRt, edgeRt, arrowRt, 74f,
                LoadSprite("Icons/Icon_House.png"),
                LoadSprite("Icons/Icon_Sword.png"),
                LoadSprite("Icons/Icon_Shield.png"),
                LoadSprite("Icons/Icon_Target.png"),
                LoadSprite("Icons/Icon_Gem.png"),
                LoadSprite("Ornaments/Orn_MapDot.png"));
            return SavePrefab(rt.gameObject, "03_HUDModules/HUD_CircularMinimap.prefab");
        }

        private static GameObject BuildQuestRail(UIThemeSO theme)
        {
            var rt = CreateUIObject("HUD_QuestRail", null, new Vector2(275, 340));

            // Hanging chain behind quest slots
            var chainRt = CreateUIObject("HangingChain", rt, new Vector2(28, 300));
            chainRt.anchorMin = chainRt.anchorMax = new Vector2(0f, 1f);
            chainRt.pivot = new Vector2(0.5f, 1f);
            chainRt.anchoredPosition = new Vector2(44, -36);
            AddImage(chainRt, LoadSprite("Ornaments/Orn_IronChain.png"), Image.Type.Tiled);

            // Header bar "QUESTS"
            var hdrRt = CreateUIObject("QuestsHeader", rt, new Vector2(118, 36));
            hdrRt.anchorMin = hdrRt.anchorMax = new Vector2(0f, 1f);
            hdrRt.pivot = new Vector2(0f, 1f);
            AddImage(hdrRt, LoadSprite("Bars/Bar_StoneFrame.png"), Image.Type.Sliced);
            AddLabel("Title", hdrRt, "QUESTS", 18, theme.textGold, theme);

            var listRt = CreateUIObject("QuestList", rt, new Vector2(275, 290));
            listRt.anchorMin = listRt.anchorMax = new Vector2(0f, 1f);
            listRt.pivot = new Vector2(0f, 1f);
            listRt.anchoredPosition = new Vector2(0, -46);
            var vlg = listRt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.childControlHeight = false;
            vlg.childControlWidth = false;

            // Template Row
            var rowRt = CreateUIObject("QuestRowTemplate", listRt, new Vector2(304, 78));
            var hitImg = rowRt.gameObject.AddComponent<Image>();
            hitImg.color = new Color(0, 0, 0, 0);
            MakeButton(rowRt.gameObject, hitImg);

            var slotRt = CreateUIObject("SlotFrame", rowRt, new Vector2(74, 74));
            slotRt.anchorMin = slotRt.anchorMax = new Vector2(0f, 0.5f);
            slotRt.anchoredPosition = new Vector2(44, 0);
            AddImage(slotRt, LoadSprite("Slots/Slot_RivetedIron.png"), Image.Type.Sliced);
            var icRt = CreateUIObject("Icon", slotRt, new Vector2(46, 46));
            AddImage(icRt, LoadSprite("Icons/Icon_Target.png"));

            var ribRt = CreateUIObject("StatusRibbon", rowRt, new Vector2(212, 50));
            ribRt.anchorMin = ribRt.anchorMax = new Vector2(0f, 0.5f);
            ribRt.pivot = new Vector2(0f, 0.5f);
            ribRt.anchoredPosition = new Vector2(84, 0);
            AddImage(ribRt, LoadSprite("Ribbons/Ribbon_Status_Emerald.png"), Image.Type.Sliced);
            var rLbl = AddLabel("Label", ribRt, "COLLECT NOW!", 17, theme.textLight, theme);
            rLbl.rectTransform.offsetMin = new Vector2(30, 4);
            rLbl.rectTransform.offsetMax = new Vector2(-16, -4);

            var ctrl = rt.gameObject.AddComponent<QuestRailController>();
            ctrl.BindReferences(listRt, rowRt.gameObject,
                LoadSprite("Ribbons/Ribbon_Status_Purple.png"),
                LoadSprite("Ribbons/Ribbon_Status_Crimson.png"),
                LoadSprite("Ribbons/Ribbon_Status_Emerald.png"));
            return SavePrefab(rt.gameObject, "03_HUDModules/HUD_QuestRail.prefab");
        }

        private static GameObject BuildFloatingHealthbar(UIThemeSO theme)
        {
            var rt = CreateUIObject("HUD_FloatingHealthbar", null, new Vector2(140, 22));
            AddImage(rt, LoadSprite("Bars/Bar_StoneFrame.png"), Image.Type.Sliced);
            var fa = CreateUIObject("FillArea", rt, Vector2.zero);
            Stretch(fa, 6, 4, 6, 4);
            var fillRt = CreateUIObject("Fill", fa, Vector2.zero);
            Stretch(fillRt);
            var fillImg = AddImage(fillRt, LoadSprite("Bars/Bar_Fill_Crimson.png"), Image.Type.Sliced);
            var bar = rt.gameObject.AddComponent<FantasyProgressBar>();
            bar.BindReferences(fillImg, null, null, "", false);
            rt.gameObject.AddComponent<FloatingHealthbarView>();
            return SavePrefab(rt.gameObject, "03_HUDModules/HUD_FloatingHealthbar.prefab");
        }

        private static GameObject BuildEnemyEdgeMark()
        {
            var rt = CreateUIObject("HUD_EnemyEdgeMark", null, new Vector2(52, 52));
            var arrowRt = CreateUIObject("Arrow", rt, new Vector2(46, 46));
            AddImage(arrowRt, LoadSprite("Icons/Icon_EnemyMark.png"));
            var comp = rt.gameObject.AddComponent<OffscreenEdgeMarker>();
            comp.BindReferences(arrowRt);
            return SavePrefab(rt.gameObject, "03_HUDModules/HUD_EnemyEdgeMark.prefab");
        }

        // =====================================================================
        // 04_POPUPS & MODALS (Matching All 4 Reference Images + GameplayScene)
        // =====================================================================
        public static void BuildAllModals(UIThemeSO theme)
        {
            BuildPauseScrollModal(theme);
            BuildLevelCompleteModal(theme);
            BuildLeatherInventoryModal(theme);
            BuildDiscoveryModal(theme);
            BuildShipUpgradeModal(theme);
            BuildPowerupSelectionModal(theme);
        }

        private static (RectTransform root, Button backdrop) CreateModalShell(string name, UIThemeSO theme)
        {
            var root = CreateUIObject(name, null, Vector2.zero);
            Stretch(root);
            root.gameObject.AddComponent<CanvasGroup>();

            var dimRt = CreateUIObject("DimBackdrop", root, Vector2.zero);
            Stretch(dimRt);
            var dimImg = AddImage(dimRt, null, Image.Type.Simple, theme.backdropDim, raycast: true);
            dimImg.gameObject.AddComponent<UIThemeBinder>().Configure(UIThemeColorToken.BackdropDim);
            var backdropBtn = dimRt.gameObject.AddComponent<Button>();
            backdropBtn.transition = Selectable.Transition.None;
            return (root, backdropBtn);
        }

        // Reference Image 1: Hanging Parchment Scroll Modal
        private static GameObject BuildPauseScrollModal(UIThemeSO theme)
        {
            var (root, backdropBtn) = CreateModalShell("Modal_PauseScroll", theme);

            var panel = CreateUIObject("ScrollContainer", root, new Vector2(460, 620));

            // Hanging Iron Chains (Top Left & Top Right)
            foreach (float x in new[] { -165f, 165f })
            {
                var ch = CreateUIObject("IronChain", panel, new Vector2(28, 300));
                ch.anchorMin = ch.anchorMax = new Vector2(0.5f, 1f);
                ch.pivot = new Vector2(0.5f, 0f);
                ch.anchoredPosition = new Vector2(x, -16f);
                AddImage(ch, LoadSprite("Ornaments/Orn_IronChain.png"), Image.Type.Tiled);
            }

            // Aged Parchment Body
            var body = CreateUIObject("ParchmentBody", panel, new Vector2(390, 540));
            AddImage(body, LoadSprite("Panels/Panel_ParchmentScroll.png"), Image.Type.Sliced);

            // Top & Bottom Wooden/Parchment Rollers
            var topRoll = CreateUIObject("TopRoller", panel, new Vector2(454, 68));
            topRoll.anchorMin = topRoll.anchorMax = new Vector2(0.5f, 1f);
            topRoll.anchoredPosition = new Vector2(0, -34);
            AddImage(topRoll, LoadSprite("Panels/Panel_ScrollRoller.png"), Image.Type.Sliced);

            var botRoll = CreateUIObject("BottomRoller", panel, new Vector2(454, 68));
            botRoll.anchorMin = botRoll.anchorMax = new Vector2(0.5f, 0f);
            botRoll.anchoredPosition = new Vector2(0, 34);
            AddImage(botRoll, LoadSprite("Panels/Panel_ScrollRoller.png"), Image.Type.Sliced);

            // Purple-Gold Header Ribbon
            var ribbon = CreateUIObject("TitleRibbon", panel, new Vector2(340, 82));
            ribbon.anchorMin = ribbon.anchorMax = new Vector2(0.5f, 1f);
            ribbon.anchoredPosition = new Vector2(0, 4);
            AddImage(ribbon, LoadSprite("Ribbons/Ribbon_Header_Purple.png"), Image.Type.Sliced);
            var titleTxt = AddLabel("TitleText", ribbon, "TITLE EXAMPLE", 24, theme.textLight, theme);
            titleTxt.rectTransform.anchoredPosition = new Vector2(0, 6);
            titleTxt.gameObject.AddComponent<UIArchedText>().SetArch(7f);

            // Flanking Circular Iron-Ring Corner Buttons (< Back & X Close)
            var backRt = CreateUIObject("BackButton", panel, new Vector2(78, 78));
            backRt.anchorMin = backRt.anchorMax = new Vector2(0f, 1f);
            backRt.anchoredPosition = new Vector2(22, -26);
            var backImg = AddImage(backRt, LoadSprite("Buttons/Btn_CircleRing_Emerald.png"), Image.Type.Simple, raycast: true);
            var backBtn = MakeButton(backRt.gameObject, backImg);
            AddImage(CreateUIObject("Icon", backRt, new Vector2(34, 34)), LoadSprite("Icons/Icon_ArrowLeft.png"));

            var closeRt = CreateUIObject("CloseButton", panel, new Vector2(78, 78));
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(-22, -26);
            var closeImg = AddImage(closeRt, LoadSprite("Buttons/Btn_CircleRing_Crimson.png"), Image.Type.Simple, raycast: true);
            var closeBtn = MakeButton(closeRt.gameObject, closeImg);
            AddImage(CreateUIObject("Icon", closeRt, new Vector2(32, 32)), LoadSprite("Icons/Icon_CloseX.png"));

            // Content Stack inside Parchment
            var content = CreateUIObject("Content", body, Vector2.zero);
            Stretch(content, 36, 56, 36, 68);
            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false;
            vlg.childControlWidth = false;

            // Dual-Orb ON/OFF Toggle
            var togRt = CreateUIObject("SoundToggle", content, new Vector2(218, 58));
            var togBg = AddImage(togRt, LoadSprite("Bars/Toggle_ParchmentTrack.png"), Image.Type.Sliced, raycast: true);
            var togBtn = MakeButton(togRt.gameObject, togBg, false);
            var onL = AddLabel("OnLbl", togRt, "ON", 18, theme.textDark, theme, TextAlignmentOptions.Center, true, false);
            onL.rectTransform.anchorMin = new Vector2(0, 0); onL.rectTransform.anchorMax = new Vector2(0.25f, 1);
            var onO = AddImage(CreateUIObject("OnOrb", togRt, new Vector2(46, 46)), LoadSprite("Bars/Toggle_Orb_Emerald.png"));
            onO.rectTransform.anchoredPosition = new Vector2(-24, 0);
            var offO = AddImage(CreateUIObject("OffOrb", togRt, new Vector2(46, 46)), LoadSprite("Bars/Toggle_Orb_Crimson.png"));
            offO.rectTransform.anchoredPosition = new Vector2(24, 0);
            var offL = AddLabel("OffLbl", togRt, "OFF", 18, theme.textDark, theme, TextAlignmentOptions.Center, true, false);
            offL.rectTransform.anchorMin = new Vector2(0.75f, 0); offL.rectTransform.anchorMax = new Vector2(1, 1);
            var toggle = togRt.gameObject.AddComponent<DualPillToggle>();
            toggle.BindReferences(togBtn, onO, offO, onL, offL, true);

            // EXTRA LIFE Header + Coin CTA Pill
            var extraHdr = AddLabel("ExtraLifeHeader", content, "EXTRA LIFE", 22, theme.woodMid, theme, TextAlignmentOptions.Center, true, false);
            extraHdr.rectTransform.sizeDelta = new Vector2(260, 28);

            var extraRt = CreateUIObject("ExtraLifeButton", content, new Vector2(286, 74));
            var extraBg = AddImage(extraRt, LoadSprite("Buttons/Btn_Capsule_Emerald.png"), Image.Type.Sliced, raycast: true);
            var extraBtn = MakeButton(extraRt.gameObject, extraBg);
            var cPill = CreateUIObject("CostCapsule", extraRt, new Vector2(130, 40));
            cPill.anchorMin = cPill.anchorMax = new Vector2(0f, 0.5f); cPill.pivot = new Vector2(0f, 0.5f);
            cPill.anchoredPosition = new Vector2(18, 0);
            AddImage(cPill, LoadSprite("Buttons/Btn_Capsule_Cream.png"), Image.Type.Sliced);
            var cIcon = CreateUIObject("Coin", cPill, new Vector2(40, 40));
            cIcon.anchorMin = cIcon.anchorMax = new Vector2(0f, 0.5f); cIcon.anchoredPosition = new Vector2(10, 0);
            AddImage(cIcon, LoadSprite("Icons/Icon_Coin.png"));
            var costTxt = AddLabel("CostText", cPill, "10,500", 19, theme.textDark, theme, TextAlignmentOptions.Center, true, false);
            costTxt.rectTransform.offsetMin = new Vector2(30, 0);
            var getTxt = AddLabel("GetIt", extraRt, "GET IT!", 25, theme.textLight, theme);
            getTxt.rectTransform.anchorMin = new Vector2(0.52f, 0); getTxt.rectTransform.anchorMax = new Vector2(0.96f, 1);

            // PAUSED Header + QUIT GAME + RESTART Iron-Capped Buttons
            var pausedHdr = AddLabel("PausedHeader", content, "PAUSED", 22, theme.woodMid, theme, TextAlignmentOptions.Center, true, false);
            pausedHdr.rectTransform.sizeDelta = new Vector2(260, 28);

            var quitRt = CreateUIObject("QuitGameButton", content, new Vector2(286, 72));
            var quitBg = AddImage(quitRt, LoadSprite("Buttons/Btn_IronSlab_Crimson.png"), Image.Type.Sliced, raycast: true);
            var quitBtn = MakeButton(quitRt.gameObject, quitBg);
            AddLabel("Label", quitRt, "QUIT GAME", 26, theme.textLight, theme);

            var restartRt = CreateUIObject("RestartButton", content, new Vector2(286, 72));
            var restartBg = AddImage(restartRt, LoadSprite("Buttons/Btn_IronSlab_Emerald.png"), Image.Type.Sliced, raycast: true);
            var restartBtn = MakeButton(restartRt.gameObject, restartBg);
            AddLabel("Label", restartRt, "RESTART", 26, theme.textLight, theme);

            // Transition & Modal Controller
            var trans = root.gameObject.AddComponent<UIViewTransition>();
            trans.Configure(UITransitionStyle.ScrollUnroll, panel);
            var modal = root.gameObject.AddComponent<PauseScrollModal>();
            modal.ConfigureView("PauseScroll", UILayer.Popups, false);
            modal.BindModalButtons(backdropBtn, closeBtn);
            modal.BindReferences(titleTxt, backBtn, toggle, extraBtn, costTxt, quitBtn, restartBtn);

            return SavePrefab(root.gameObject, "04_PopupsAndModals/Modal_PauseScroll.prefab");
        }

        // Reference Images 2 & 3: Cracked Dark-Stone Level Complete + Wooden Leaderboard Drawer
        private static GameObject BuildLevelCompleteModal(UIThemeSO theme)
        {
            var (root, backdropBtn) = CreateModalShell("Modal_LevelComplete", theme);
            var holder = CreateUIObject("ModalHolder", root, new Vector2(820, 620));

            // Attached Planked Wood Leaderboard Drawer (Behind right edge of stone plaque!)
            var drawer = CreateUIObject("WoodLeaderboardDrawer", holder, new Vector2(276, 480));
            drawer.anchoredPosition = new Vector2(246, -12);
            AddImage(drawer, LoadSprite("Panels/Panel_WoodDrawer.png"), Image.Type.Sliced);

            var latchRt = CreateUIObject("DrawerLatch", drawer, new Vector2(28, 92));
            latchRt.anchorMin = latchRt.anchorMax = new Vector2(1f, 0.5f);
            latchRt.anchoredPosition = new Vector2(-4, 0);
            var latchImg = AddImage(latchRt, null, Image.Type.Simple, new Color(0, 0, 0, 0), raycast: true);
            var latchBtn = MakeButton(latchRt.gameObject, latchImg, false);

            var topHdr = AddLabel("TopPlayersHeader", drawer, "TOP PLAYERS", 22, theme.textLight, theme);
            topHdr.rectTransform.anchorMin = new Vector2(0.08f, 0.87f);
            topHdr.rectTransform.anchorMax = new Vector2(0.92f, 0.97f);

            var listRt = CreateUIObject("PlayersList", drawer, Vector2.zero);
            Stretch(listRt, 18, 86, 26, 68);
            var listVlg = listRt.gameObject.AddComponent<VerticalLayoutGroup>();
            listVlg.spacing = 10;
            listVlg.childControlHeight = false;
            listVlg.childControlWidth = true;

            // Leaderboard Row Template
            var rowTpl = CreateUIObject("LeaderboardRowTemplate", listRt, new Vector2(226, 86));
            var rankBadge = CreateUIObject("RankBadge", rowTpl, new Vector2(30, 38));
            rankBadge.anchorMin = rankBadge.anchorMax = new Vector2(0f, 1f);
            rankBadge.anchoredPosition = new Vector2(14, -22);
            AddImage(rankBadge, LoadSprite("Ribbons/Ribbon_RankBadge_Crimson.png"));
            AddLabel("RankText", rankBadge, "1", 18, theme.textLight, theme);

            var pFrame = CreateUIObject("PortraitFrame", rowTpl, new Vector2(68, 68));
            pFrame.anchorMin = pFrame.anchorMax = new Vector2(0f, 0.5f);
            pFrame.anchoredPosition = new Vector2(56, 0);
            AddImage(pFrame, LoadSprite("Slots/Slot_RivetedIron.png"), Image.Type.Sliced);
            AddImage(CreateUIObject("Avatar", pFrame, new Vector2(46, 46)), LoadSprite("Icons/Avatar_Hero1.png"));

            var info = CreateUIObject("Info", rowTpl, Vector2.zero);
            Stretch(info, 98, 8, 6, 8);
            var pName = AddLabel("PlayerName", info, "MELISSA", 20, theme.textLight, theme, TextAlignmentOptions.Left);
            pName.rectTransform.anchorMin = new Vector2(0, 0.45f); pName.rectTransform.anchorMax = new Vector2(1, 1);
            var pScore = AddLabel("ScoreText", info, "20,500", 18, theme.textGold, theme, TextAlignmentOptions.Left);
            pScore.rectTransform.anchorMin = new Vector2(0, 0); pScore.rectTransform.anchorMax = new Vector2(1, 0.48f);

            // Social CONNECT button at bottom of wooden drawer
            var connRt = CreateUIObject("SocialConnectBtn", drawer, new Vector2(216, 54));
            connRt.anchorMin = connRt.anchorMax = new Vector2(0.5f, 0f);
            connRt.anchoredPosition = new Vector2(-4, 42);
            var connImg = AddImage(connRt, LoadSprite("Buttons/Btn_Capsule_Cobalt.png"), Image.Type.Sliced, raycast: true);
            var connBtn = MakeButton(connRt.gameObject, connImg);
            AddLabel("Label", connRt, "CONNECT", 22, theme.textLight, theme);

            // Main Cracked Dark-Stone Plaque
            var stone = CreateUIObject("StonePlaque", holder, new Vector2(486, 580));
            stone.anchoredPosition = new Vector2(-88, 0);
            AddImage(stone, LoadSprite("Panels/Panel_StonePlaque.png"), Image.Type.Sliced);

            // Crimson-Gold Header Ribbon
            var rib = CreateUIObject("HeaderRibbon", stone, new Vector2(340, 80));
            rib.anchorMin = rib.anchorMax = new Vector2(0.5f, 1f);
            rib.anchoredPosition = new Vector2(0, 6);
            AddImage(rib, LoadSprite("Ribbons/Ribbon_Header_Crimson.png"), Image.Type.Sliced);
            var ribTxt = AddLabel("RibbonTitle", rib, "MENU EXAMPLE", 24, theme.textLight, theme);
            ribTxt.rectTransform.anchoredPosition = new Vector2(0, 6);
            ribTxt.gameObject.AddComponent<UIArchedText>().SetArch(7f);

            // Back & Close Corner Rings
            var backRt = CreateUIObject("BackButton", stone, new Vector2(74, 74));
            backRt.anchorMin = backRt.anchorMax = new Vector2(0f, 1f); backRt.anchoredPosition = new Vector2(12, -12);
            var backImg = AddImage(backRt, LoadSprite("Buttons/Btn_CircleRing_Emerald.png"), Image.Type.Simple, raycast: true);
            var backBtn = MakeButton(backRt.gameObject, backImg);
            AddImage(CreateUIObject("Icon", backRt, new Vector2(32, 32)), LoadSprite("Icons/Icon_ArrowLeft.png"));

            var closeRt = CreateUIObject("CloseButton", stone, new Vector2(74, 74));
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(1f, 1f); closeRt.anchoredPosition = new Vector2(-12, -12);
            var closeImg = AddImage(closeRt, LoadSprite("Buttons/Btn_CircleRing_Crimson.png"), Image.Type.Simple, raycast: true);
            var closeBtn = MakeButton(closeRt.gameObject, closeImg);
            AddImage(CreateUIObject("Icon", closeRt, new Vector2(30, 30)), LoadSprite("Icons/Icon_CloseX.png"));

            // LEVEL COMPLETE Headline
            var headTxt = AddLabel("Headline", stone, "LEVEL COMPLETE!", 36, theme.textLight, theme);
            headTxt.rectTransform.anchorMin = new Vector2(0.08f, 0.78f);
            headTxt.rectTransform.anchorMax = new Vector2(0.92f, 0.90f);

            // Sweeping Winged Stone Ribbon + 3 Stars
            var wingRib = CreateUIObject("WingedStarBanner", stone, new Vector2(400, 128));
            wingRib.anchoredPosition = new Vector2(0, 96);
            AddImage(wingRib, LoadSprite("Ribbons/Ribbon_StoneWinged.png"));

            var stars = new Image[3];
            Vector2[] starPos = { new Vector2(-84, 16), new Vector2(0, 28), new Vector2(84, 16) };
            float[] starSizes = { 82f, 98f, 82f };
            for (int i = 0; i < 3; i++)
            {
                var sRt = CreateUIObject($"Star_{i + 1}", wingRib, new Vector2(starSizes[i], starSizes[i]));
                sRt.anchoredPosition = starPos[i];
                stars[i] = AddImage(sRt, LoadSprite(i < 2 ? "Ornaments/Orn_Star_Gold.png" : "Ornaments/Orn_Star_Empty.png"));
            }

            // Recessed GOAL Tablet
            var goalBox = CreateUIObject("GoalTablet", stone, new Vector2(356, 100));
            goalBox.anchoredPosition = new Vector2(0, -14);
            AddImage(goalBox, LoadSprite("Panels/Panel_StoneRecessed.png"), Image.Type.Sliced);
            var goalHdr = AddLabel("GoalHeader", goalBox, "GOAL:", 20, theme.textLight, theme);
            goalHdr.rectTransform.anchorMin = new Vector2(0, 0.62f); goalHdr.rectTransform.anchorMax = new Vector2(1, 0.96f);
            var goalTxt = AddLabel("GoalDesc", goalBox, "EARN 1000 POINTS AND ALL 3 STARS.", 18, theme.textGold, theme);
            goalTxt.rectTransform.anchorMin = new Vector2(0.08f, 0.08f); goalTxt.rectTransform.anchorMax = new Vector2(0.92f, 0.62f);

            // 3 Riveted Booster Slots
            var boostRow = CreateUIObject("BoosterRow", stone, new Vector2(330, 94));
            boostRow.anchoredPosition = new Vector2(0, -124);
            var hlg = boostRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 18;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            string[] boostIcons = { "Icons/Icon_Lightning.png", "Icons/Icon_Gem.png", "Icons/Icon_Sword.png" };
            string[] boostCounts = { "1", "3", "+" };
            for (int i = 0; i < 3; i++)
            {
                var bs = CreateUIObject($"Booster_{i + 1}", boostRow, new Vector2(88, 88));
                AddImage(bs, LoadSprite(i < 2 ? "Slots/Slot_RivetedPurpleGlow.png" : "Slots/Slot_RivetedIron.png"), Image.Type.Sliced);
                AddImage(CreateUIObject("Icon", bs, new Vector2(48, 48)), LoadSprite(boostIcons[i]));
                var bDot = CreateUIObject("Badge", bs, new Vector2(32, 32));
                bDot.anchorMin = bDot.anchorMax = new Vector2(1, 0); bDot.anchoredPosition = new Vector2(-6, 6);
                AddImage(bDot, LoadSprite("Bars/Toggle_Orb_Emerald.png"));
                AddLabel("Count", bDot, boostCounts[i], 16, theme.textLight, theme);
            }

            // Iron-Capped PLAY Button
            var playRt = CreateUIObject("PlayButton", stone, new Vector2(280, 74));
            playRt.anchorMin = playRt.anchorMax = new Vector2(0.5f, 0f);
            playRt.anchoredPosition = new Vector2(0, 62);
            var playBg = AddImage(playRt, LoadSprite("Buttons/Btn_IronSlab_Emerald.png"), Image.Type.Sliced, raycast: true);
            var playBtn = MakeButton(playRt.gameObject, playBg);
            AddLabel("Label", playRt, "PLAY", 30, theme.textLight, theme);

            var trans = root.gameObject.AddComponent<UIViewTransition>();
            trans.Configure(UITransitionStyle.ChainDrop, holder);
            var modal = root.gameObject.AddComponent<LevelCompleteModal>();
            modal.ConfigureView("LevelComplete", UILayer.Popups, false);
            modal.BindModalButtons(backdropBtn, closeBtn);
            modal.BindReferences(ribTxt, headTxt, goalTxt, stars,
                LoadSprite("Ornaments/Orn_Star_Gold.png"), LoadSprite("Ornaments/Orn_Star_Empty.png"),
                playBtn, backBtn, drawer, latchBtn, listRt, rowTpl.gameObject, connBtn,
                new[]
                {
                    LoadSprite("Ribbons/Ribbon_RankBadge_Crimson.png"),
                    LoadSprite("Ribbons/Ribbon_RankBadge_Purple.png"),
                    LoadSprite("Ribbons/Ribbon_RankBadge_Emerald.png"),
                });

            return SavePrefab(root.gameObject, "04_PopupsAndModals/Modal_LevelComplete.prefab");
        }

        // Reference Image 4 + GameplayScene Inventory: Stitched Plum-Leather Bag Modal
        private static GameObject BuildLeatherInventoryModal(UIThemeSO theme)
        {
            var (root, backdropBtn) = CreateModalShell("Modal_LeatherInventory", theme);
            var bagRoot = CreateUIObject("LeatherBagRoot", root, new Vector2(1060, 640));

            // Left-Side Category Tabs Rail (Ref 4 style: All Bag, Fish, Weapons, Potions)
            var tabAll = CreateBagTab(bagRoot, "Tab_AllBag", new Vector2(-490, 155), "Icons/Icon_Backpack.png", true);
            var tabFish = CreateBagTab(bagRoot, "Tab_FishBag", new Vector2(-490, 50), "Icons/Icon_Fish.png", false);
            var tabWeapons = CreateBagTab(bagRoot, "Tab_Weapons", new Vector2(-490, -55), "Icons/Icon_Sword.png", false);
            var tabPotions = CreateBagTab(bagRoot, "Tab_Potions", new Vector2(-490, -160), "Icons/Icon_Potion.png", false);

            // Main Stitched Plum-Burgundy Leather Bag Panel
            var bagPanel = CreateUIObject("StitchedLeatherPanel", bagRoot, new Vector2(920, 540));
            bagPanel.anchoredPosition = new Vector2(36, 12);
            AddImage(bagPanel, LoadSprite("Panels/Panel_LeatherBag.png"), Image.Type.Sliced);

            // Top Brown Leather Belt Strap & Silver Buckle Header ("BAG 1")
            var belt = CreateUIObject("LeatherBeltHeader", bagPanel, new Vector2(490, 84));
            belt.anchorMin = belt.anchorMax = new Vector2(0.5f, 1f);
            belt.anchoredPosition = new Vector2(0, 6);
            AddImage(belt, LoadSprite("Panels/Panel_LeatherBeltHeader.png"), Image.Type.Sliced);
            var beltTitle = AddLabel("BagTitle", belt, "BAG 1", 30, theme.textLight, theme);

            // Top-Right Chunky Faceted Red Close X (Ref 4!)
            var closeRt = CreateUIObject("FacetedCloseButton", bagPanel, new Vector2(84, 84));
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.anchoredPosition = new Vector2(8, 8);
            var closeImg = AddImage(closeRt, LoadSprite("Buttons/Btn_FacetedCloseX.png"), Image.Type.Simple, raycast: true);
            var closeBtn = MakeButton(closeRt.gameObject, closeImg);

            // Left Section: 2x5 Octagonal Recessed Dark Slot Grid (Ref 4)
            var gridContainer = CreateUIObject("OctagonalGridPanel", bagPanel, new Vector2(560, 350));
            gridContainer.anchorMin = gridContainer.anchorMax = new Vector2(0f, 0.5f);
            gridContainer.pivot = new Vector2(0f, 0.5f);
            gridContainer.anchoredPosition = new Vector2(36, 18);

            var glg = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(98, 98);
            glg.spacing = new Vector2(14, 18);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;
            glg.childAlignment = TextAnchor.UpperLeft;

            // Octagonal Slot Template
            var slotTpl = CreateUIObject("OctagonalSlotTemplate", gridContainer, new Vector2(98, 98));
            var slotBg = AddImage(slotTpl, LoadSprite("Slots/Slot_OctagonalLeather.png"), Image.Type.Sliced, raycast: true);
            MakeButton(slotTpl.gameObject, slotBg);
            var sIcon = CreateUIObject("Icon", slotTpl, new Vector2(64, 64));
            AddImage(sIcon, LoadSprite("Icons/Icon_Sword.png"));
            var rStripe = CreateUIObject("RarityStripe", slotTpl, new Vector2(54, 6));
            rStripe.anchorMin = rStripe.anchorMax = new Vector2(0.5f, 0f); rStripe.anchoredPosition = new Vector2(0, 8);
            AddImage(rStripe, null, Image.Type.Simple, theme.goldBright);
            var sCount = AddLabel("Count", slotTpl, "x2", 18, theme.textLight, theme, TextAlignmentOptions.BottomRight);
            sCount.rectTransform.offsetMin = new Vector2(4, 12);
            sCount.rectTransform.offsetMax = new Vector2(-10, -4);

            var emptyLbl = AddLabel("EmptyGridText", bagPanel, "NO ITEMS IN THIS BAG.", 22, theme.stitchCream, theme);
            emptyLbl.rectTransform.anchorMin = new Vector2(0.04f, 0.35f); emptyLbl.rectTransform.anchorMax = new Vector2(0.62f, 0.65f);
            emptyLbl.gameObject.SetActive(false);

            // Bag Capacity Slider Bar + Sell All Button below grid
            var capRt = CreateUIObject("CapacityBar", bagPanel, new Vector2(340, 38));
            capRt.anchorMin = capRt.anchorMax = new Vector2(0f, 0f);
            capRt.pivot = new Vector2(0f, 0f);
            capRt.anchoredPosition = new Vector2(36, 36);
            AddImage(capRt, LoadSprite("Bars/Bar_StoneFrame.png"), Image.Type.Sliced);
            var cfa = CreateUIObject("FillArea", capRt, Vector2.zero); Stretch(cfa, 12, 8, 12, 8);
            var cFillRt = CreateUIObject("Fill", cfa, Vector2.zero); Stretch(cFillRt);
            var cFillImg = AddImage(cFillRt, LoadSprite("Bars/Bar_Fill_Emerald.png"), Image.Type.Sliced);
            var cLbl = AddLabel("CapLabel", capRt, "5/20 BAG", 16, theme.textLight, theme);
            var capBar = capRt.gameObject.AddComponent<FantasyProgressBar>();
            capBar.BindReferences(cFillImg, cLbl, null, " BAG", true);

            var sellRt = CreateUIObject("SellAllButton", bagPanel, new Vector2(194, 48));
            sellRt.anchorMin = sellRt.anchorMax = new Vector2(0f, 0f);
            sellRt.pivot = new Vector2(0f, 0f);
            sellRt.anchoredPosition = new Vector2(394, 31);
            var sellBg = AddImage(sellRt, LoadSprite("Buttons/Btn_Capsule_Emerald.png"), Image.Type.Sliced, raycast: true);
            var sellBtn = MakeButton(sellRt.gameObject, sellBg);
            AddLabel("Label", sellRt, "SELL ALL", 20, theme.textLight, theme);

            // Right Section: Parchment Item Detail Card
            var detCard = CreateUIObject("DetailCard", bagPanel, new Vector2(268, 412));
            detCard.anchorMin = detCard.anchorMax = new Vector2(1f, 0.5f);
            detCard.pivot = new Vector2(1f, 0.5f);
            detCard.anchoredPosition = new Vector2(-34, -8);
            AddImage(detCard, LoadSprite("Panels/Panel_TooltipParchment.png"), Image.Type.Sliced);

            var detContent = CreateUIObject("DetailContent", detCard, Vector2.zero); Stretch(detContent, 18, 28, 18, 22);
            var detIconFrame = CreateUIObject("IconFrame", detContent, new Vector2(104, 104));
            detIconFrame.anchorMin = detIconFrame.anchorMax = new Vector2(0.5f, 1f);
            detIconFrame.anchoredPosition = new Vector2(0, -56);
            AddImage(detIconFrame, LoadSprite("Slots/Slot_RivetedIron.png"), Image.Type.Sliced);
            var detIcon = AddImage(CreateUIObject("Icon", detIconFrame, new Vector2(70, 70)), LoadSprite("Icons/Icon_Sword.png"));

            var detName = AddLabel("ItemName", detContent, "IRON CUTLASS", 22, theme.textDark, theme, TextAlignmentOptions.Center, true, false);
            detName.rectTransform.anchorMin = new Vector2(0, 0.58f); detName.rectTransform.anchorMax = new Vector2(1, 0.70f);

            var detRarity = AddLabel("ItemRarity", detContent, "RARE", 17, theme.royalPurple, theme, TextAlignmentOptions.Center, true, false);
            detRarity.rectTransform.anchorMin = new Vector2(0, 0.50f); detRarity.rectTransform.anchorMax = new Vector2(1, 0.59f);

            var detDesc = AddLabel("ItemDesc", detContent, "Forged in dungeon steel.", 16, theme.woodDark, theme, TextAlignmentOptions.Top, false, false);
            detDesc.rectTransform.anchorMin = new Vector2(0.04f, 0.25f); detDesc.rectTransform.anchorMax = new Vector2(0.96f, 0.50f);

            var detInfo = AddLabel("ItemInfo", detContent, "Owned: 1", 15, theme.woodMid, theme, TextAlignmentOptions.Center, true, false);
            detInfo.rectTransform.anchorMin = new Vector2(0, 0.16f); detInfo.rectTransform.anchorMax = new Vector2(1, 0.25f);

            var actRt = CreateUIObject("ActionButton", detContent, new Vector2(186, 52));
            actRt.anchorMin = actRt.anchorMax = new Vector2(0.5f, 0f); actRt.anchoredPosition = new Vector2(0, 26);
            var actBg = AddImage(actRt, LoadSprite("Buttons/Btn_IronSlab_Emerald.png"), Image.Type.Sliced, raycast: true);
            var actBtn = MakeButton(actRt.gameObject, actBg);
            var actLbl = AddLabel("Label", actRt, "EQUIP", 22, theme.textLight, theme);

            var detEmpty = AddLabel("SelectHint", detCard, "SELECT AN ITEM", 20, theme.woodMid, theme, TextAlignmentOptions.Center, true, false);
            detEmpty.gameObject.SetActive(false);

            // Bottom Stacked Gold Readout ("YOUR GOLD 32167363" - Ref 4!)
            var goldStrip = CreateUIObject("BottomGoldStrip", bagRoot, new Vector2(420, 64));
            goldStrip.anchorMin = goldStrip.anchorMax = new Vector2(0.5f, 0f);
            goldStrip.anchoredPosition = new Vector2(-180, 12);
            var gCoin = CreateUIObject("CoinStack", goldStrip, new Vector2(64, 64));
            gCoin.anchorMin = gCoin.anchorMax = new Vector2(0f, 0.5f); gCoin.anchoredPosition = new Vector2(36, 0);
            AddImage(gCoin, LoadSprite("Icons/Icon_CoinStack.png"));

            var ygLbl = AddLabel("YourGoldLabel", goldStrip, "YOUR GOLD", 24, theme.textLight, theme, TextAlignmentOptions.Left);
            ygLbl.rectTransform.anchorMin = new Vector2(0, 0); ygLbl.rectTransform.anchorMax = new Vector2(0.52f, 1);
            ygLbl.rectTransform.offsetMin = new Vector2(76, 0);

            var gVal = AddLabel("GoldValue", goldStrip, "19,456,000", 26, theme.goldAmber, theme, TextAlignmentOptions.Left);
            gVal.rectTransform.anchorMin = new Vector2(0.50f, 0); gVal.rectTransform.anchorMax = new Vector2(1f, 1);
            var goldRoller = gVal.gameObject.AddComponent<UINumberRoller>();

            var trans = root.gameObject.AddComponent<UIViewTransition>();
            trans.Configure(UITransitionStyle.PopBounce, bagRoot);
            var modal = root.gameObject.AddComponent<LeatherInventoryModal>();
            modal.ConfigureView("LeatherInventory", UILayer.Popups, false);
            modal.BindModalButtons(backdropBtn, closeBtn);
            modal.BindReferences(beltTitle, goldRoller, capBar, tabAll, tabFish, tabWeapons, tabPotions,
                gridContainer, slotTpl.gameObject, emptyLbl.gameObject,
                LoadSprite("Slots/Slot_OctagonalLeather.png"), LoadSprite("Slots/Slot_OctagonalSelected.png"),
                detContent.gameObject, detEmpty.gameObject, detIcon, detName, detRarity, detDesc, detInfo, actBtn, actLbl, sellBtn);

            return SavePrefab(root.gameObject, "04_PopupsAndModals/Modal_LeatherInventory.prefab");
        }

        private static Button CreateBagTab(RectTransform parent, string name, Vector2 pos, string iconPath, bool active)
        {
            var rt = CreateUIObject(name, parent, new Vector2(102, 86));
            rt.anchoredPosition = pos;
            var bg = AddImage(rt, LoadSprite(active ? "Slots/Slot_LeatherTab_Active.png" : "Slots/Slot_LeatherTab_Inactive.png"), Image.Type.Sliced, raycast: true);
            var btn = MakeButton(rt.gameObject, bg);
            AddImage(CreateUIObject("Icon", rt, new Vector2(56, 56)), LoadSprite(iconPath));
            return btn;
        }

        // GameplayScene Upgraded Popup: New Fish / Loot Discovery
        private static GameObject BuildDiscoveryModal(UIThemeSO theme)
        {
            var (root, backdropBtn) = CreateModalShell("Modal_NewDiscovery", theme);
            var panel = CreateUIObject("DiscoveryPanel", root, new Vector2(680, 460));
            AddImage(panel, LoadSprite("Panels/Panel_StonePlaque.png"), Image.Type.Sliced);

            var rib = CreateUIObject("HeaderRibbon", panel, new Vector2(340, 76));
            rib.anchorMin = rib.anchorMax = new Vector2(0.5f, 1f); rib.anchoredPosition = new Vector2(0, 8);
            AddImage(rib, LoadSprite("Ribbons/Ribbon_Header_Emerald.png"), Image.Type.Sliced);
            var hdrTxt = AddLabel("HeaderText", rib, "NEW DISCOVERY!", 24, theme.textLight, theme);
            hdrTxt.rectTransform.anchoredPosition = new Vector2(0, 6);

            var closeRt = CreateUIObject("CloseBtn", panel, new Vector2(72, 72));
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(1f, 1f); closeRt.anchoredPosition = new Vector2(-12, -12);
            var cImg = AddImage(closeRt, LoadSprite("Buttons/Btn_CircleRing_Crimson.png"), Image.Type.Simple, raycast: true);
            var closeBtn = MakeButton(closeRt.gameObject, cImg);
            AddImage(CreateUIObject("Icon", closeRt, new Vector2(30, 30)), LoadSprite("Icons/Icon_CloseX.png"));

            // Left Glowing Visual Pedestal
            var vis = CreateUIObject("VisualPedestal", panel, new Vector2(236, 286));
            vis.anchorMin = vis.anchorMax = new Vector2(0f, 0.5f); vis.anchoredPosition = new Vector2(154, -14);
            AddImage(vis, LoadSprite("Slots/Slot_RivetedPurpleGlow.png"), Image.Type.Sliced);
            AddImage(CreateUIObject("Glow", vis, new Vector2(200, 200)), LoadSprite("FX/FX_SoftGlow.png"), Image.Type.Simple, new Color(0.3f, 0.85f, 1f, 0.55f));
            var itemIcon = AddImage(CreateUIObject("ItemIcon", vis, new Vector2(138, 138)), LoadSprite("Icons/Icon_Fish.png"));

            // Right Parchment Info & Stats
            var info = CreateUIObject("InfoCard", panel, new Vector2(346, 286));
            info.anchorMin = info.anchorMax = new Vector2(1f, 0.5f); info.anchoredPosition = new Vector2(-204, -14);
            AddImage(info, LoadSprite("Panels/Panel_TooltipParchment.png"), Image.Type.Sliced);

            var itemName = AddLabel("ItemName", info, "AZURE GOLDEN KOI", 24, theme.textDark, theme, TextAlignmentOptions.Center, true, false);
            itemName.rectTransform.anchorMin = new Vector2(0.06f, 0.78f); itemName.rectTransform.anchorMax = new Vector2(0.94f, 0.94f);

            var statsRoot = CreateUIObject("StatsHolder", info, Vector2.zero);
            Stretch(statsRoot, 18, 96, 18, 66);
            var vlg = statsRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6; vlg.childControlHeight = false; vlg.childControlWidth = true;

            var statTpl = CreateUIObject("StatRowTemplate", statsRoot, new Vector2(280, 34));
            AddImage(statTpl, LoadSprite("Panels/Panel_StoneRecessed.png"), Image.Type.Sliced);
            var stIcon = CreateUIObject("Icon", statTpl, new Vector2(24, 24));
            stIcon.anchorMin = stIcon.anchorMax = new Vector2(0, 0.5f); stIcon.anchoredPosition = new Vector2(18, 0);
            AddImage(stIcon, LoadSprite("Icons/Icon_Gem.png"));
            var stName = AddLabel("NameText", statTpl, "RARITY", 15, theme.textLight, theme, TextAlignmentOptions.Left);
            stName.rectTransform.offsetMin = new Vector2(36, 0);
            var stVal = AddLabel("ValueText", statTpl, "LEGENDARY", 15, theme.textGold, theme, TextAlignmentOptions.Right);
            stVal.rectTransform.offsetMax = new Vector2(-12, 0);

            var desc = AddLabel("Description", info, "A legendary glowing reef fish.", 15, theme.woodDark, theme, TextAlignmentOptions.Center, false, false);
            desc.rectTransform.anchorMin = new Vector2(0.08f, 0.08f); desc.rectTransform.anchorMax = new Vector2(0.92f, 0.34f);

            var trans = root.gameObject.AddComponent<UIViewTransition>();
            trans.Configure(UITransitionStyle.PopBounce, panel);
            var modal = root.gameObject.AddComponent<DiscoveryPopupModal>();
            modal.ConfigureView("NewDiscovery", UILayer.Popups, false);
            modal.BindModalButtons(backdropBtn, closeBtn);
            modal.BindReferences(hdrTxt, itemName, desc, itemIcon, statsRoot, statTpl.gameObject);

            return SavePrefab(root.gameObject, "04_PopupsAndModals/Modal_NewDiscovery.prefab");
        }

        // GameplayScene Upgraded Popup: Ship Building & Upgrade Workshop
        private static GameObject BuildShipUpgradeModal(UIThemeSO theme)
        {
            var (root, backdropBtn) = CreateModalShell("Modal_ShipUpgrade", theme);
            var panel = CreateUIObject("ShipPanel", root, new Vector2(820, 520));
            AddImage(panel, LoadSprite("Panels/Panel_StonePlaque.png"), Image.Type.Sliced);

            var rib = CreateUIObject("HeaderRibbon", panel, new Vector2(360, 78));
            rib.anchorMin = rib.anchorMax = new Vector2(0.5f, 1f); rib.anchoredPosition = new Vector2(0, 8);
            AddImage(rib, LoadSprite("Ribbons/Ribbon_Header_Purple.png"), Image.Type.Sliced);
            var titleTxt = AddLabel("TitleText", rib, "SHIP BUILDING", 24, theme.textLight, theme);
            titleTxt.rectTransform.anchoredPosition = new Vector2(0, 6);

            var closeRt = CreateUIObject("CloseBtn", panel, new Vector2(74, 74));
            closeRt.anchorMin = closeRt.anchorMax = new Vector2(1f, 1f); closeRt.anchoredPosition = new Vector2(-12, -12);
            var cImg = AddImage(closeRt, LoadSprite("Buttons/Btn_CircleRing_Crimson.png"), Image.Type.Simple, raycast: true);
            var closeBtn = MakeButton(closeRt.gameObject, cImg);
            AddImage(CreateUIObject("Icon", closeRt, new Vector2(30, 30)), LoadSprite("Icons/Icon_CloseX.png"));

            // Left Ship Preview Frame
            var left = CreateUIObject("ShipVisuals", panel, new Vector2(280, 360));
            left.anchorMin = left.anchorMax = new Vector2(0f, 0.5f); left.anchoredPosition = new Vector2(174, -14);
            AddImage(left, LoadSprite("Panels/Panel_WoodDrawer.png"), Image.Type.Sliced);
            AddImage(CreateUIObject("Glow", left, new Vector2(220, 220)), LoadSprite("FX/FX_SoftGlow.png"), Image.Type.Simple, new Color(1f, 0.84f, 0.35f, 0.55f));
            var shipImg = AddImage(CreateUIObject("ShipIcon", left, new Vector2(168, 168)), LoadSprite("Icons/Icon_Boat.png"));
            var lvlBadge = CreateUIObject("LevelBadge", left, new Vector2(140, 40));
            lvlBadge.anchorMin = lvlBadge.anchorMax = new Vector2(0.5f, 0f); lvlBadge.anchoredPosition = new Vector2(0, 34);
            AddImage(lvlBadge, LoadSprite("Buttons/Btn_Capsule_Stone.png"), Image.Type.Sliced);
            var lvlTxt = AddLabel("LevelText", lvlBadge, "LVL 4", 20, theme.textGold, theme);

            // Right Dynamic Stat Upgrade Rows
            var right = CreateUIObject("StatUpgradesRoot", panel, new Vector2(436, 360));
            right.anchorMin = right.anchorMax = new Vector2(1f, 0.5f); right.anchoredPosition = new Vector2(-248, -14);
            var vlg = right.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14; vlg.childControlHeight = false; vlg.childControlWidth = true;

            var rowTpl = CreateUIObject("StatUpgradeRowTemplate", right, new Vector2(430, 98));
            AddImage(rowTpl, LoadSprite("Panels/Panel_StoneRecessed.png"), Image.Type.Sliced);
            var sName = AddLabel("StatName", rowTpl, "HULL ARMOR", 19, theme.textLight, theme, TextAlignmentOptions.Left);
            sName.rectTransform.anchorMin = new Vector2(0.04f, 0.58f); sName.rectTransform.anchorMax = new Vector2(0.56f, 0.92f);
            var sVal = AddLabel("StatValue", rowTpl, "4,200", 18, theme.textGold, theme, TextAlignmentOptions.Right);
            sVal.rectTransform.anchorMin = new Vector2(0.35f, 0.58f); sVal.rectTransform.anchorMax = new Vector2(0.64f, 0.92f);

            var barFrame = CreateUIObject("BarFrame", rowTpl, new Vector2(246, 26));
            barFrame.anchorMin = barFrame.anchorMax = new Vector2(0f, 0f); barFrame.pivot = new Vector2(0, 0);
            barFrame.anchoredPosition = new Vector2(16, 16);
            AddImage(barFrame, LoadSprite("Bars/Bar_StoneFrame.png"), Image.Type.Sliced);
            var fillRt = CreateUIObject("Fill", barFrame, Vector2.zero); Stretch(fillRt, 8, 5, 8, 5);
            AddImage(fillRt, LoadSprite("Bars/Bar_Fill_Emerald.png"), Image.Type.Sliced);

            var upRt = CreateUIObject("UpgradeBtn", rowTpl, new Vector2(138, 60));
            upRt.anchorMin = upRt.anchorMax = new Vector2(1f, 0.5f); upRt.anchoredPosition = new Vector2(-82, 0);
            var upImg = AddImage(upRt, LoadSprite("Buttons/Btn_Capsule_Emerald.png"), Image.Type.Sliced, raycast: true);
            MakeButton(upRt.gameObject, upImg);
            var upCoin = CreateUIObject("Coin", upRt, new Vector2(28, 28));
            upCoin.anchorMin = upCoin.anchorMax = new Vector2(0, 0.5f); upCoin.anchoredPosition = new Vector2(20, 0);
            AddImage(upCoin, LoadSprite("Icons/Icon_Coin.png"));
            var upCost = AddLabel("CostText", upRt, "2,500", 19, theme.textLight, theme);
            upCost.rectTransform.offsetMin = new Vector2(30, 0);

            var trans = root.gameObject.AddComponent<UIViewTransition>();
            trans.Configure(UITransitionStyle.PopBounce, panel);
            var modal = root.gameObject.AddComponent<ShipUpgradeModal>();
            modal.ConfigureView("ShipUpgrade", UILayer.Popups, false);
            modal.BindModalButtons(backdropBtn, closeBtn);
            modal.BindReferences(titleTxt, lvlTxt, shipImg, right, rowTpl.gameObject);

            return SavePrefab(root.gameObject, "04_PopupsAndModals/Modal_ShipUpgrade.prefab");
        }

        // GameplayScene Upgraded Overlay: Roguelike Powerup Card Selection
        private static GameObject BuildPowerupSelectionModal(UIThemeSO theme)
        {
            var (root, backdropBtn) = CreateModalShell("Modal_PowerupSelection", theme);
            var title = AddLabel("SelectTitle", root, "CHOOSE A POWERUP!", 42, theme.textGold, theme);
            title.rectTransform.anchorMin = new Vector2(0.15f, 0.80f);
            title.rectTransform.anchorMax = new Vector2(0.85f, 0.92f);

            var cardsRow = CreateUIObject("CardsRow", root, new Vector2(1060, 440));
            cardsRow.anchoredPosition = new Vector2(0, -20);
            var hlg = cardsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 28; hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false; hlg.childControlHeight = false;

            var cardTpl = CreateUIObject("PowerupCardTemplate", cardsRow, new Vector2(290, 400));
            AddImage(cardTpl, LoadSprite("Panels/Panel_StonePlaque.png"), Image.Type.Sliced);

            var tRib = CreateUIObject("TitleRibbon", cardTpl, new Vector2(256, 62));
            tRib.anchorMin = tRib.anchorMax = new Vector2(0.5f, 1f); tRib.anchoredPosition = new Vector2(0, -8);
            AddImage(tRib, LoadSprite("Ribbons/Ribbon_Header_Emerald.png"), Image.Type.Sliced);
            AddLabel("TitleText", tRib, "STORM VOLLEY", 19, theme.textLight, theme);

            var ped = CreateUIObject("IconPedestal", cardTpl, new Vector2(146, 146));
            ped.anchoredPosition = new Vector2(0, 48);
            AddImage(ped, LoadSprite("Slots/Slot_RivetedPurpleGlow.png"), Image.Type.Sliced);
            AddImage(CreateUIObject("Icon", ped, new Vector2(90, 90)), LoadSprite("Icons/Icon_Cannon.png"));

            var desc = AddLabel("DescText", cardTpl, "+40% Cannon Fire Rate", 18, theme.parchmentCore, theme);
            desc.rectTransform.anchorMin = new Vector2(0.08f, 0.28f); desc.rectTransform.anchorMax = new Vector2(0.92f, 0.44f);

            var costPill = CreateUIObject("CostPill", cardTpl, new Vector2(138, 36));
            costPill.anchoredPosition = new Vector2(0, -96);
            AddImage(costPill, LoadSprite("Buttons/Btn_Capsule_Cream.png"), Image.Type.Sliced);
            AddLabel("CostText", costPill, "1,000", 18, theme.textDark, theme, TextAlignmentOptions.Center, true, false);

            var actRt = CreateUIObject("ActionButton", cardTpl, new Vector2(210, 62));
            actRt.anchorMin = actRt.anchorMax = new Vector2(0.5f, 0f); actRt.anchoredPosition = new Vector2(0, 48);
            var actImg = AddImage(actRt, LoadSprite("Buttons/Btn_IronSlab_Emerald.png"), Image.Type.Sliced, raycast: true);
            MakeButton(actRt.gameObject, actImg);
            AddLabel("Label", actRt, "PICK", 24, theme.textLight, theme);

            var trans = root.gameObject.AddComponent<UIViewTransition>();
            trans.Configure(UITransitionStyle.PopBounce, cardsRow);
            var modal = root.gameObject.AddComponent<PowerupSelectionModal>();
            modal.ConfigureView("PowerupSelection", UILayer.PowerupOverlay, false);
            modal.BindModalButtons(backdropBtn, null);
            modal.BindReferences(cardsRow, cardTpl.gameObject);

            return SavePrefab(root.gameObject, "04_PopupsAndModals/Modal_PowerupSelection.prefab");
        }

        // =====================================================================
        // 05_NAVIGATION & 06_FEEDBACK
        // =====================================================================
        public static void BuildNavigationAndFeedback(UIThemeSO theme, UIEventChannels channel)
        {
            BuildTitleCrest(theme);
            BuildSagaLevelMap(theme);
            BuildCelebrationBanner(theme, channel);
            BuildToastStack(theme, channel);
        }

        private static GameObject BuildTitleCrest(UIThemeSO theme)
        {
            var rt = CreateUIObject("Ornament_TitleCrest", null, new Vector2(440, 210));
            AddImage(rt, LoadSprite("Panels/Panel_StonePlaque.png"), Image.Type.Sliced);

            var rib = CreateUIObject("TopRibbon", rt, new Vector2(310, 74));
            rib.anchorMin = rib.anchorMax = new Vector2(0.5f, 1f); rib.anchoredPosition = new Vector2(0, 10);
            AddImage(rib, LoadSprite("Ribbons/Ribbon_Header_Purple.png"), Image.Type.Sliced);
            AddLabel("SubTop", rib, "COMPLETE", 22, theme.textLight, theme);

            var mainTxt = AddLabel("FantasyTitle", rt, "FANTASY", 62, theme.goldBright, theme);
            mainTxt.rectTransform.anchoredPosition = new Vector2(0, 4);

            var wing = CreateUIObject("WingBanner", rt, new Vector2(360, 62));
            wing.anchorMin = wing.anchorMax = new Vector2(0.5f, 0f); wing.anchoredPosition = new Vector2(0, -10);
            AddImage(wing, LoadSprite("Ribbons/Ribbon_StoneWinged.png"));
            AddLabel("SubBottom", wing, "GAME UI KIT", 24, theme.textLight, theme);

            return SavePrefab(rt.gameObject, "05_NavigationAndMap/Ornament_TitleCrest.prefab");
        }

        private static GameObject BuildSagaLevelMap(UIThemeSO theme)
        {
            var rt = CreateUIObject("Screen_SagaLevelMap", null, new Vector2(620, 420));
            var dots = CreateUIObject("PathDots", rt, Vector2.zero); Stretch(dots);
            var nodes = CreateUIObject("LevelNodes", rt, Vector2.zero); Stretch(nodes);

            // Node Template
            var nodeTpl = CreateUIObject("LevelNodeTemplate", nodes, new Vector2(74, 74));
            var nBg = AddImage(nodeTpl, LoadSprite("Ornaments/Orn_MapNode_Complete.png"), Image.Type.Simple, raycast: true);
            MakeButton(nodeTpl.gameObject, nBg);
            var num = AddLabel("NumberLabel", nodeTpl, "12", 22, theme.textLight, theme);
            num.rectTransform.anchoredPosition = new Vector2(0, -4);

            var lockIc = CreateUIObject("LockIcon", nodeTpl, new Vector2(36, 36));
            lockIc.anchoredPosition = new Vector2(0, 14);
            AddImage(lockIc, LoadSprite("Icons/Icon_Padlock.png"));

            var starsRow = CreateUIObject("StarsRow", nodeTpl, new Vector2(84, 28));
            starsRow.anchorMin = starsRow.anchorMax = new Vector2(0.5f, 1f); starsRow.anchoredPosition = new Vector2(0, 10);
            var hlg = starsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 2; hlg.childAlignment = TextAnchor.MiddleCenter; hlg.childControlWidth = false; hlg.childControlHeight = false;
            for (int i = 0; i < 3; i++)
                AddImage(CreateUIObject($"S{i}", starsRow, new Vector2(24, 24)), LoadSprite("Ornaments/Orn_Star_Gold.png"));

            // Parchment Tooltip Popover
            var tip = CreateUIObject("TooltipPopover", rt, new Vector2(236, 114));
            AddImage(tip, LoadSprite("Panels/Panel_TooltipParchment.png"), Image.Type.Sliced);
            var tipTitle = AddLabel("TipTitle", tip, "INFORMATION", 18, theme.goldAmber, theme);
            tipTitle.rectTransform.anchorMin = new Vector2(0.06f, 0.78f); tipTitle.rectTransform.anchorMax = new Vector2(0.94f, 1.06f);
            var tipBody = AddLabel("TipBody", tip, "USE THIS BOOST TO REMOVE ANY TILE.", 15, theme.textDark, theme, TextAlignmentOptions.Center, true, false);
            tipBody.rectTransform.anchorMin = new Vector2(0.08f, 0.14f); tipBody.rectTransform.anchorMax = new Vector2(0.92f, 0.76f);

            var ctrl = rt.gameObject.AddComponent<SagaLevelMapController>();
            ctrl.BindReferences(dots, nodes, nodeTpl.gameObject,
                LoadSprite("Ornaments/Orn_MapDot.png"),
                LoadSprite("Ornaments/Orn_MapNode_Complete.png"),
                LoadSprite("Ornaments/Orn_MapNode_Active.png"),
                LoadSprite("Ornaments/Orn_MapNode_Locked.png"),
                LoadSprite("Icons/Icon_Padlock.png"),
                LoadSprite("Ornaments/Orn_Star_Gold.png"),
                tip, tipTitle, tipBody);

            return SavePrefab(rt.gameObject, "05_NavigationAndMap/Screen_SagaLevelMap.prefab");
        }

        private static GameObject BuildCelebrationBanner(UIThemeSO theme, UIEventChannels channel)
        {
            var rt = CreateUIObject("FX_CelebrationBanner", null, new Vector2(680, 190));
            var cg = rt.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            var glowRt = CreateUIObject("GlowBackdrop", rt, new Vector2(640, 180));
            var glowImg = AddImage(glowRt, LoadSprite("FX/FX_SoftGlow.png"), Image.Type.Simple, new Color32(52, 211, 139, 210));

            var sparkles = new RectTransform[4];
            Vector2[] spPos = { new Vector2(-240, 44), new Vector2(246, 38), new Vector2(-190, -48), new Vector2(204, -44) };
            for (int i = 0; i < 4; i++)
            {
                var sp = CreateUIObject($"Sparkle_{i}", rt, new Vector2(36, 36));
                sp.anchoredPosition = spPos[i];
                AddImage(sp, LoadSprite("FX/FX_SparkleStar.png"), Image.Type.Simple, theme.goldBright);
                sparkles[i] = sp;
            }

            var txt = AddLabel("BannerText", rt, "VICTORY!", 76, theme.textLight, theme);
            var arch = txt.gameObject.AddComponent<UIArchedText>();
            arch.SetArch(18f);

            var comp = rt.gameObject.AddComponent<CelebrationBannerView>();
            comp.BindReferences(cg, txt, arch, glowImg, sparkles, channel);
            return SavePrefab(rt.gameObject, "06_FeedbackAndFX/FX_CelebrationBanner.prefab");
        }

        private static GameObject BuildToastStack(UIThemeSO theme, UIEventChannels channel)
        {
            var rt = CreateUIObject("FX_ToastStack", null, new Vector2(340, 220));
            var vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8; vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = false; vlg.childControlWidth = false;

            var tpl = CreateUIObject("ToastTemplate", rt, new Vector2(324, 54));
            AddImage(tpl, LoadSprite("Ribbons/Ribbon_Status_Emerald.png"), Image.Type.Sliced);
            var txt = AddLabel("ToastLabel", tpl, "COLLECTED!", 20, theme.textLight, theme);
            txt.rectTransform.offsetMin = new Vector2(32, 4);
            txt.rectTransform.offsetMax = new Vector2(-18, -4);
            tpl.gameObject.SetActive(false);

            var comp = rt.gameObject.AddComponent<ToastNotificationStack>();
            comp.BindReferences(rt, tpl.gameObject,
                LoadSprite("Ribbons/Ribbon_Status_Purple.png"),
                LoadSprite("Ribbons/Ribbon_Status_Crimson.png"),
                LoadSprite("Ribbons/Ribbon_Status_Emerald.png"),
                channel);
            return SavePrefab(rt.gameObject, "06_FeedbackAndFX/FX_ToastStack.prefab");
        }
    }
}
