using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Animation;
using FantasyUI.Core;
using FantasyUI.Data;
using FantasyUI.Theme;

namespace FantasyUI.Components
{
    /// <summary>
    /// Exact Reference Images 2 & 3 Cracked Dark-Stone Level Complete Modal
    /// with 3 staggered winged gold stars, recessed Goal tablet, 3 riveted booster slots,
    /// iron-capped PLAY button, and an expandable Planked Wood TOP PLAYERS Leaderboard drawer.
    /// </summary>
    public class LevelCompleteModal : UIModal
    {
        [Header("Stone Plaque Elements")]
        [SerializeField] private TMP_Text ribbonHeaderText;
        [SerializeField] private TMP_Text headlineText;
        [SerializeField] private TMP_Text goalText;
        [SerializeField] private Image[] starImages;
        [SerializeField] private Sprite goldStarSprite;
        [SerializeField] private Sprite emptyStarSprite;
        [SerializeField] private Button playButton;
        [SerializeField] private Button backButton;

        [Header("Sliding Wooden Leaderboard Drawer")]
        [SerializeField] private RectTransform woodDrawerRect;
        [SerializeField] private Button drawerToggleLatch;
        [SerializeField] private Transform leaderboardListRoot;
        [SerializeField] private GameObject leaderboardRowTemplate;
        [SerializeField] private Button socialConnectButton;
        [SerializeField] private Sprite[] rankBadgeSprites; // 0:Crimson, 1:Purple, 2:Emerald

        private bool _drawerOpen = true;
        private Vector2 _drawerOpenPos;
        private bool _drawerCaptured;
        private readonly List<GameObject> _rows = new List<GameObject>();

        public event Action OnPlayClicked;
        public event Action OnSocialConnectClicked;

        protected override void Awake()
        {
            base.Awake();
            if (playButton != null) playButton.onClick.AddListener(() => { OnPlayClicked?.Invoke(); CloseSelf(); });
            if (backButton != null) backButton.onClick.AddListener(CloseSelf);
            if (drawerToggleLatch != null) drawerToggleLatch.onClick.AddListener(ToggleLeaderboardDrawer);
            if (socialConnectButton != null) socialConnectButton.onClick.AddListener(() => OnSocialConnectClicked?.Invoke());
        }

        public void BindReferences(
            TMP_Text ribbon, TMP_Text headline, TMP_Text goal, Image[] stars, Sprite goldStar, Sprite emptyStar,
            Button playBtn, Button backBtn, RectTransform drawerRect, Button latchBtn,
            Transform listRoot, GameObject rowTemplate, Button connectBtn, Sprite[] rankBadges)
        {
            ribbonHeaderText = ribbon; headlineText = headline; goalText = goal;
            starImages = stars; goldStarSprite = goldStar; emptyStarSprite = emptyStar;
            playButton = playBtn; backButton = backBtn;
            woodDrawerRect = drawerRect; drawerToggleLatch = latchBtn;
            leaderboardListRoot = listRoot; leaderboardRowTemplate = rowTemplate;
            socialConnectButton = connectBtn; rankBadgeSprites = rankBadges;
        }

        public void Present(string menuHeader, string headline, string goalDesc, int starsEarned, IList<LeaderboardEntryData> topPlayers)
        {
            if (ribbonHeaderText != null) ribbonHeaderText.text = menuHeader;
            if (headlineText != null) headlineText.text = headline;
            if (goalText != null) goalText.text = goalDesc;

            if (starImages != null)
            {
                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] == null) continue;
                    bool earned = i < starsEarned;
                    starImages[i].sprite = earned ? goldStarSprite : emptyStarSprite;
                    if (earned && Application.isPlaying)
                    {
                        var st = starImages[i].transform;
                        st.localScale = Vector3.zero;
                        UITweener.Run(st, 0.35f + i * 0.12f, EaseType.OutBack, t =>
                        {
                            st.localScale = Vector3.one * Mathf.Max(0f, t);
                        });
                    }
                }
            }

            PopulateLeaderboard(topPlayers);
        }

        public void PopulateLeaderboard(IList<LeaderboardEntryData> entries)
        {
            for (int i = 0; i < _rows.Count; i++)
                if (_rows[i] != null) Destroy(_rows[i]);
            _rows.Clear();

            if (entries == null || leaderboardListRoot == null || leaderboardRowTemplate == null) return;
            leaderboardRowTemplate.SetActive(false);

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var row = Instantiate(leaderboardRowTemplate, leaderboardListRoot);
                row.name = $"RankRow_{entry.rank}";
                row.SetActive(true);
                _rows.Add(row);

                var badgeImg = row.transform.Find("RankBadge")?.GetComponent<Image>();
                var rankTxt = row.transform.Find("RankBadge/RankText")?.GetComponent<TMP_Text>();
                var avatarImg = row.transform.Find("PortraitFrame/Avatar")?.GetComponent<Image>();
                var nameTxt = row.transform.Find("Info/PlayerName")?.GetComponent<TMP_Text>();
                var scoreTxt = row.transform.Find("Info/ScoreText")?.GetComponent<TMP_Text>();

                if (badgeImg != null && rankBadgeSprites != null && rankBadgeSprites.Length > 0)
                    badgeImg.sprite = rankBadgeSprites[Mathf.Clamp(i, 0, rankBadgeSprites.Length - 1)];
                if (rankTxt != null) rankTxt.text = entry.rank.ToString();
                if (avatarImg != null && entry.avatar != null) avatarImg.sprite = entry.avatar;
                if (nameTxt != null) nameTxt.text = entry.playerName;
                if (scoreTxt != null) scoreTxt.text = entry.score.ToString("N0");
            }
        }

        public void ToggleLeaderboardDrawer()
        {
            if (woodDrawerRect == null) return;
            if (!_drawerCaptured)
            {
                _drawerOpenPos = woodDrawerRect.anchoredPosition;
                _drawerCaptured = true;
            }
            _drawerOpen = !_drawerOpen;
            Vector2 start = woodDrawerRect.anchoredPosition;
            Vector2 target = _drawerOpen ? _drawerOpenPos : _drawerOpenPos + new Vector2(-235f, 0f);
            UIThemeManager.Instance?.PlaySfx(UISfxCue.StoneOpen);
            UITweener.Run(woodDrawerRect, 0.25f, EaseType.OutCubic, t =>
            {
                woodDrawerRect.anchoredPosition = Vector2.Lerp(start, target, t);
            });
        }
    }
}
