using System.Collections.Generic;
using UnityEngine;
using FantasyUI.Components;
using FantasyUI.Data;
using FantasyUI.Theme;

namespace FantasyUI.Demo
{
    /// <summary>
    /// Interactive showcase controller for UILibraryShowcase.unity, populating the Saga Level Map
    /// and allowing one-click live Theme cycling across the entire Fantasy UI Kit gallery.
    /// </summary>
    public class UIShowcaseController : MonoBehaviour
    {
        [SerializeField] private SagaLevelMapController sagaMap;
        [SerializeField] private CelebrationBannerView celebrationBanner;
        [SerializeField] private UIThemeManager themeManager;

        private int _bannerIndex;

        public void BindReferences(SagaLevelMapController map, CelebrationBannerView banner, UIThemeManager tm)
        {
            sagaMap = map;
            celebrationBanner = banner;
            themeManager = tm;
        }

        private void Start()
        {
            if (sagaMap != null)
            {
                var nodes = new List<LevelNodeData>
                {
                    new LevelNodeData { levelNumber = 10, levelTitle = "SUNKEN COVE",    goalDescription = "EARN 1000 POINTS AND ALL 3 STARS.", state = LevelNodeState.Completed, starsEarned = 3 },
                    new LevelNodeData { levelNumber = 11, levelTitle = "BRAMBLE KEEP",   goalDescription = "DEFEAT THE RAIDER CAPTAIN.",        state = LevelNodeState.Completed, starsEarned = 3 },
                    new LevelNodeData { levelNumber = 12, levelTitle = "CRIMSON ARCH",   goalDescription = "COLLECT 3 ANCIENT RELICS.",         state = LevelNodeState.Completed, starsEarned = 3 },
                    new LevelNodeData { levelNumber = 13, levelTitle = "ARCANE SPIRE",   goalDescription = "USE THIS BOOST TO REMOVE ANY TILE.",state = LevelNodeState.Active,    starsEarned = 0 },
                    new LevelNodeData { levelNumber = 14, levelTitle = "IRON CITADEL",   goalDescription = "UNLOCK BY COMPLETING LEVEL 13.",    state = LevelNodeState.Locked,    starsEarned = 0 },
                    new LevelNodeData { levelNumber = 15, levelTitle = "DREAD THRONE",   goalDescription = "FINAL BOSS ENCOUNTER.",             state = LevelNodeState.Locked,    starsEarned = 0 },
                };
                var positions = new List<Vector2>
                {
                    new Vector2(210f, -155f),
                    new Vector2(60f,  -125f),
                    new Vector2(-115f, -90f),
                    new Vector2(20f,   -10f),
                    new Vector2(145f,   45f),
                    new Vector2(-35f,  120f),
                };
                sagaMap.BuildPath(nodes, positions);
                sagaMap.ShowTooltip("INFORMATION", "USE THIS BOOST TO REMOVE ANY TILE.", new Vector2(-135f, 35f));
            }
        }

        public void CycleTheme() => themeManager?.CycleNextTheme();

        public void TriggerNextBanner()
        {
            if (celebrationBanner == null) return;
            var values = (CelebrationType[])System.Enum.GetValues(typeof(CelebrationType));
            celebrationBanner.Play(values[_bannerIndex % values.Length]);
            _bannerIndex++;
        }
    }
}
