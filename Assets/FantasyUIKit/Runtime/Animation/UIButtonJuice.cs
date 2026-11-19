using UnityEngine;
using UnityEngine.EventSystems;
using FantasyUI.Theme;

namespace FantasyUI.Animation
{
    /// <summary>
    /// Adds crisp comic-fantasy tactile feedback (hover lift, press squash, release spring bounce, SFX) to any Button or Tile.
    /// </summary>
    public class UIButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float pressScale = 0.92f;
        [SerializeField] private float duration = 0.12f;
        [SerializeField] private UISfxCue clickSfx = UISfxCue.Click;

        private Vector3 _baseScale = Vector3.one;
        private bool _captured;

        private void Awake() => Capture();
        private void OnEnable()
        {
            Capture();
            transform.localScale = _baseScale;
        }

        private void Capture()
        {
            if (_captured) return;
            _baseScale = transform.localScale;
            _captured = true;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Capture();
            AnimateTo(_baseScale * hoverScale, EaseType.OutCubic);
            UIThemeManager.Instance?.PlaySfx(UISfxCue.Hover);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Capture();
            AnimateTo(_baseScale, EaseType.OutCubic);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Capture();
            AnimateTo(_baseScale * pressScale, EaseType.OutCubic);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Capture();
            AnimateTo(_baseScale * hoverScale, EaseType.OutBack);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            UIThemeManager.Instance?.PlaySfx(clickSfx);
        }

        private void AnimateTo(Vector3 target, EaseType ease)
        {
            Vector3 start = transform.localScale;
            UITweener.Run(this, duration, ease, t =>
            {
                if (this != null) transform.localScale = Vector3.LerpUnclamped(start, target, t);
            });
        }
    }
}
