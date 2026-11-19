using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Core;

namespace FantasyUI.Components
{
    /// <summary>
    /// Exact Reference Image 1 Hanging Parchment Scroll Modal (Pause / Settings / Extra Life).
    /// Features purple-gold header ribbon, iron corner rings, dual-orb ON/OFF toggles,
    /// coin-capsule CTA ("EXTRA LIFE / GET IT!"), and iron-capped QUIT GAME / RESTART buttons.
    /// </summary>
    public class PauseScrollModal : UIModal
    {
        [Header("Scroll Controls")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button backRingButton;
        [SerializeField] private DualPillToggle soundToggle;
        [SerializeField] private Button extraLifeButton;
        [SerializeField] private TMP_Text extraLifeCostText;
        [SerializeField] private Button quitGameButton;
        [SerializeField] private Button restartButton;

        public event Action<bool> OnSoundToggled;
        public event Action OnExtraLifeClicked;
        public event Action OnQuitClicked;
        public event Action OnRestartClicked;

        protected override void Awake()
        {
            base.Awake();
            if (backRingButton != null) backRingButton.onClick.AddListener(CloseSelf);
            if (soundToggle != null) soundToggle.OnValueChanged += v => OnSoundToggled?.Invoke(v);
            if (extraLifeButton != null) extraLifeButton.onClick.AddListener(() => OnExtraLifeClicked?.Invoke());
            if (quitGameButton != null) quitGameButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
            if (restartButton != null) restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
        }

        public void BindReferences(TMP_Text title, Button backBtn, DualPillToggle toggle, Button extraBtn, TMP_Text costTxt, Button quitBtn, Button restartBtn)
        {
            titleText = title;
            backRingButton = backBtn;
            soundToggle = toggle;
            extraLifeButton = extraBtn;
            extraLifeCostText = costTxt;
            quitGameButton = quitBtn;
            restartButton = restartBtn;
        }

        public void Configure(string title, int extraLifeCost)
        {
            if (titleText != null) titleText.text = title;
            if (extraLifeCostText != null) extraLifeCostText.text = extraLifeCost.ToString("N0");
        }
    }
}
