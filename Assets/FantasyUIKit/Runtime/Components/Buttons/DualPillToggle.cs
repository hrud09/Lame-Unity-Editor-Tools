using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Animation;
using FantasyUI.Theme;

namespace FantasyUI.Components
{
    /// <summary>
    /// Exact Reference Image 1 dual-orb parchment toggle (ON emerald orb / OFF crimson orb).
    /// </summary>
    public class DualPillToggle : MonoBehaviour
    {
        [SerializeField] private Button hitButton;
        [SerializeField] private Image onOrb;
        [SerializeField] private Image offOrb;
        [SerializeField] private TMP_Text onLabel;
        [SerializeField] private TMP_Text offLabel;
        [SerializeField] private bool isOn = true;

        public bool IsOn => isOn;
        public event Action<bool> OnValueChanged;

        private void Awake()
        {
            if (hitButton == null) hitButton = GetComponent<Button>();
            if (hitButton != null) hitButton.onClick.AddListener(Toggle);
            ApplyVisuals(instant: true);
        }

        public void BindReferences(Button btn, Image onImg, Image offImg, TMP_Text onTxt, TMP_Text offTxt, bool defaultOn = true)
        {
            hitButton = btn;
            onOrb = onImg;
            offOrb = offImg;
            onLabel = onTxt;
            offLabel = offTxt;
            isOn = defaultOn;
            ApplyVisuals(instant: true);
        }

        public void Toggle() => SetState(!isOn);

        public void SetState(bool state, bool silent = false)
        {
            if (isOn == state) return;
            isOn = state;
            ApplyVisuals(instant: false);
            UIThemeManager.Instance?.PlaySfx(UISfxCue.Toggle);
            if (!silent) OnValueChanged?.Invoke(isOn);
        }

        private void ApplyVisuals(bool instant)
        {
            float onScale = isOn ? 1.12f : 0.78f;
            float offScale = isOn ? 0.78f : 1.12f;
            Color onCol = isOn ? Color.white : new Color(0.75f, 0.75f, 0.75f, 0.55f);
            Color offCol = isOn ? new Color(0.75f, 0.75f, 0.75f, 0.55f) : Color.white;

            if (onOrb != null) onOrb.color = onCol;
            if (offOrb != null) offOrb.color = offCol;
            if (onLabel != null) onLabel.alpha = isOn ? 1f : 0.45f;
            if (offLabel != null) offLabel.alpha = isOn ? 0.45f : 1f;

            if (instant || !Application.isPlaying)
            {
                if (onOrb != null) onOrb.transform.localScale = Vector3.one * onScale;
                if (offOrb != null) offOrb.transform.localScale = Vector3.one * offScale;
                return;
            }

            Vector3 startOn = onOrb != null ? onOrb.transform.localScale : Vector3.one;
            Vector3 startOff = offOrb != null ? offOrb.transform.localScale : Vector3.one;
            UITweener.Run(this, 0.18f, EaseType.OutBack, t =>
            {
                if (onOrb != null) onOrb.transform.localScale = Vector3.LerpUnclamped(startOn, Vector3.one * onScale, t);
                if (offOrb != null) offOrb.transform.localScale = Vector3.LerpUnclamped(startOff, Vector3.one * offScale, t);
            });
        }
    }
}
