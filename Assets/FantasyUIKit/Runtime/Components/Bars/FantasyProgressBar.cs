using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Animation;

namespace FantasyUI.Components
{
    /// <summary>
    /// Smooth fantasy resource/HP/MP/capacity bar with optional delayed damage-drain trail and formatted text.
    /// </summary>
    public class FantasyProgressBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private Image trailImage;
        [SerializeField] private TMP_Text valueLabel;
        [SerializeField] private string suffix = " HP";
        [SerializeField] private bool showFraction = true;
        [SerializeField] private float smoothDuration = 0.25f;

        private float _currentNorm = 1f;
        private float _trailNorm = 1f;

        public void BindReferences(Image fill, TMP_Text label = null, Image trail = null, string textSuffix = " HP", bool fraction = true)
        {
            fillImage = fill;
            valueLabel = label;
            trailImage = trail;
            suffix = textSuffix;
            showFraction = fraction;
        }

        public void SetValue(float current, float max, bool instant = false)
        {
            float safeMax = Mathf.Max(0.0001f, max);
            float clamped = Mathf.Clamp(current, 0f, safeMax);
            float targetNorm = clamped / safeMax;

            if (valueLabel != null)
            {
                valueLabel.text = showFraction
                    ? $"{Mathf.RoundToInt(clamped):N0}/{Mathf.RoundToInt(safeMax):N0}{suffix}"
                    : $"{Mathf.RoundToInt(clamped):N0}{suffix}";
            }

            if (instant || !Application.isPlaying || !gameObject.activeInHierarchy)
            {
                _currentNorm = targetNorm;
                _trailNorm = targetNorm;
                ApplyFill(_currentNorm, _trailNorm);
                return;
            }

            float startNorm = _currentNorm;
            float startTrail = _trailNorm;
            UITweener.Run(this, smoothDuration, EaseType.OutCubic, t =>
            {
                _currentNorm = Mathf.Lerp(startNorm, targetNorm, t);
                ApplyFill(_currentNorm, _trailNorm);
            });

            if (trailImage != null)
            {
                UITweener.Run(trailImage, smoothDuration * 2.2f, EaseType.OutCubic, t =>
                {
                    _trailNorm = Mathf.Lerp(startTrail, targetNorm, t);
                    ApplyFill(_currentNorm, _trailNorm);
                });
            }
        }

        private void ApplyFill(float main, float trail)
        {
            if (fillImage != null)
            {
                if (fillImage.type == Image.Type.Filled)
                    fillImage.fillAmount = main;
                else
                    fillImage.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(main), fillImage.rectTransform.anchorMax.y);
            }
            if (trailImage != null)
            {
                if (trailImage.type == Image.Type.Filled)
                    trailImage.fillAmount = Mathf.Max(main, trail);
                else
                    trailImage.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(Mathf.Max(main, trail)), trailImage.rectTransform.anchorMax.y);
            }
        }
    }
}
