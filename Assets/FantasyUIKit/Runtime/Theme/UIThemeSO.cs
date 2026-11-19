using System;
using UnityEngine;
using TMPro;

namespace FantasyUI.Theme
{
    public enum UIThemeColorToken
    {
        None = 0,
        InkOutline,
        BackdropDim,
        StoneBase,
        StoneLight,
        StoneDark,
        ParchmentCore,
        ParchmentEdge,
        WoodLight,
        WoodMid,
        WoodDark,
        LeatherPlum,
        LeatherPlumDark,
        StitchCream,
        SlotRecess,
        EmeraldPrimary,
        EmeraldDark,
        CrimsonPrimary,
        CrimsonDark,
        RoyalPurple,
        RoyalPurpleDark,
        CobaltBlue,
        GoldBright,
        GoldAmber,
        GemCyan,
        TextLight,
        TextDark,
        TextGold,
        PanelTint
    }

    /// <summary>
    /// ScriptableObject defining the complete Dark-Fantasy UI color palette, typography, and optional SFX profile.
    /// Defaults match the exact colors from Reference Images 1-4.
    /// </summary>
    [CreateAssetMenu(menuName = "Fantasy UI/Theme Preset", fileName = "Theme_DarkFantasy")]
    public class UIThemeSO : ScriptableObject
    {
        public string themeName = "Dark Fantasy Default";
        [HideInInspector] public int packageRevision = 1;

        [Header("Ink & Shadows")]
        public Color inkOutline       = new Color32(30, 21, 32, 255);    // #1E1520
        public Color backdropDim      = new Color32(20, 14, 24, 195);    // #140E18

        [Header("Carved Stone & Iron")]
        public Color stoneBase        = new Color32(82, 74, 91, 255);    // #524A5B
        public Color stoneLight       = new Color32(122, 112, 134, 255); // #7A7086
        public Color stoneDark        = new Color32(53, 46, 61, 255);    // #352E3D

        [Header("Parchment Scroll")]
        public Color parchmentCore    = new Color32(255, 231, 181, 255); // #FFE7B5
        public Color parchmentEdge    = new Color32(240, 189, 120, 255); // #F0BD78

        [Header("Tavern Wood")]
        public Color woodLight        = new Color32(168, 94, 56, 255);   // #A85E38
        public Color woodMid          = new Color32(138, 69, 39, 255);   // #8A4527
        public Color woodDark         = new Color32(96, 44, 25, 255);    // #602C19

        [Header("Stitched Leather Bag")]
        public Color leatherPlum      = new Color32(102, 44, 74, 255);   // #662C4A
        public Color leatherPlumDark  = new Color32(71, 29, 53, 255);    // #471D35
        public Color stitchCream      = new Color32(218, 196, 154, 255); // #DAC49A
        public Color slotRecess       = new Color32(29, 20, 24, 255);    // #1D1418

        [Header("Semantic Accents")]
        public Color emeraldPrimary   = new Color32(36, 156, 127, 255);  // #249C7F
        public Color emeraldDark      = new Color32(22, 107, 91, 255);   // #166B5B
        public Color crimsonPrimary   = new Color32(214, 69, 80, 255);   // #D64550
        public Color crimsonDark      = new Color32(150, 39, 52, 255);   // #962734
        public Color royalPurple      = new Color32(109, 64, 199, 255);  // #6D40C7
        public Color royalPurpleDark  = new Color32(68, 35, 140, 255);   // #44238C
        public Color cobaltBlue       = new Color32(63, 111, 199, 255);  // #3F6FC7
        public Color goldBright       = new Color32(255, 212, 56, 255);  // #FFD438
        public Color goldAmber        = new Color32(240, 147, 24, 255);  // #F09318
        public Color gemCyan          = new Color32(78, 203, 245, 255);  // #4ECBF5

        [Header("Typography & Surface Tints")]
        public Color textLight        = new Color32(252, 246, 236, 255);
        public Color textDark         = new Color32(58, 36, 28, 255);
        public Color textGold         = new Color32(255, 214, 72, 255);
        public Color panelTint        = Color.white;

        [Header("Typography Assets")]
        public TMP_FontAsset displayFont;
        public Material displayOutlineMaterial;

        [Header("Audio")]
        public UISfxProfileSO sfxProfile;

        public event Action OnThemeChanged;

        public Color ResolveColor(UIThemeColorToken token)
        {
            return token switch
            {
                UIThemeColorToken.InkOutline      => inkOutline,
                UIThemeColorToken.BackdropDim     => backdropDim,
                UIThemeColorToken.StoneBase       => stoneBase,
                UIThemeColorToken.StoneLight      => stoneLight,
                UIThemeColorToken.StoneDark       => stoneDark,
                UIThemeColorToken.ParchmentCore   => parchmentCore,
                UIThemeColorToken.ParchmentEdge   => parchmentEdge,
                UIThemeColorToken.WoodLight       => woodLight,
                UIThemeColorToken.WoodMid         => woodMid,
                UIThemeColorToken.WoodDark        => woodDark,
                UIThemeColorToken.LeatherPlum     => leatherPlum,
                UIThemeColorToken.LeatherPlumDark => leatherPlumDark,
                UIThemeColorToken.StitchCream     => stitchCream,
                UIThemeColorToken.SlotRecess      => slotRecess,
                UIThemeColorToken.EmeraldPrimary  => emeraldPrimary,
                UIThemeColorToken.EmeraldDark     => emeraldDark,
                UIThemeColorToken.CrimsonPrimary  => crimsonPrimary,
                UIThemeColorToken.CrimsonDark     => crimsonDark,
                UIThemeColorToken.RoyalPurple     => royalPurple,
                UIThemeColorToken.RoyalPurpleDark => royalPurpleDark,
                UIThemeColorToken.CobaltBlue      => cobaltBlue,
                UIThemeColorToken.GoldBright      => goldBright,
                UIThemeColorToken.GoldAmber       => goldAmber,
                UIThemeColorToken.GemCyan         => gemCyan,
                UIThemeColorToken.TextLight       => textLight,
                UIThemeColorToken.TextDark        => textDark,
                UIThemeColorToken.TextGold        => textGold,
                UIThemeColorToken.PanelTint       => panelTint,
                _                                 => Color.white
            };
        }

        private void OnValidate()
        {
            OnThemeChanged?.Invoke();
        }
    }
}
