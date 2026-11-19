using System;
using UnityEngine;

namespace FantasyUI.Animation
{
    public enum UITransitionStyle
    {
        PopBounce,
        ScrollUnroll,
        ChainDrop,
        SlideFromRight,
        SlideFromLeft,
        SlideFromBottom,
        FadeOnly
    }

    /// <summary>
    /// Modular show/hide animation preset for any UIView or UIModal.
    /// </summary>
    public class UIViewTransition : MonoBehaviour
    {
        [SerializeField] private UITransitionStyle style = UITransitionStyle.PopBounce;
        [SerializeField] private RectTransform animatedTarget;
        [SerializeField] private float showDuration = 0.28f;
        [SerializeField] private float hideDuration = 0.18f;
        [SerializeField] private float slideDistance = 320f;

        private CanvasGroup _canvasGroup;
        private Vector2 _restPos;
        private Vector3 _restScale = Vector3.one;
        private bool _capturedRest;

        private RectTransform Target => animatedTarget != null ? animatedTarget : (animatedTarget = transform as RectTransform);
        private CanvasGroup Group => _canvasGroup != null ? _canvasGroup : (_canvasGroup = GetComponent<CanvasGroup>());

        private void CaptureRestPose()
        {
            if (_capturedRest || Target == null) return;
            _restPos = Target.anchoredPosition;
            _restScale = Target.localScale;
            _capturedRest = true;
        }

        public void Configure(UITransitionStyle transitionStyle, RectTransform target = null, float showSec = 0.28f, float hideSec = 0.18f)
        {
            style = transitionStyle;
            if (target != null) animatedTarget = target;
            showDuration = showSec;
            hideDuration = hideSec;
        }

        public void PlayShow(Action onComplete = null)
        {
            CaptureRestPose();
            var rt = Target;
            var cg = Group;

            switch (style)
            {
                case UITransitionStyle.PopBounce:
                    UITweener.Run(this, showDuration, EaseType.OutBack, t =>
                    {
                        if (cg != null) cg.alpha = Mathf.Clamp01(t * 1.4f);
                        if (rt != null) rt.localScale = Vector3.LerpUnclamped(_restScale * 0.72f, _restScale, t);
                    }, onComplete);
                    break;

                case UITransitionStyle.ScrollUnroll:
                    UITweener.Run(this, showDuration + 0.06f, EaseType.OutBack, t =>
                    {
                        if (cg != null) cg.alpha = Mathf.Clamp01(t * 1.6f);
                        if (rt != null) rt.localScale = new Vector3(_restScale.x, Mathf.LerpUnclamped(0.18f, _restScale.y, t), _restScale.z);
                    }, onComplete);
                    break;

                case UITransitionStyle.ChainDrop:
                    UITweener.Run(this, showDuration + 0.08f, EaseType.OutBack, t =>
                    {
                        if (cg != null) cg.alpha = Mathf.Clamp01(t * 1.5f);
                        if (rt != null) rt.anchoredPosition = Vector2.LerpUnclamped(_restPos + Vector2.up * slideDistance, _restPos, t);
                    }, onComplete);
                    break;

                case UITransitionStyle.SlideFromRight:
                    UITweener.Run(this, showDuration, EaseType.OutCubic, t =>
                    {
                        if (cg != null) cg.alpha = t;
                        if (rt != null) rt.anchoredPosition = Vector2.Lerp(_restPos + Vector2.right * slideDistance, _restPos, t);
                    }, onComplete);
                    break;

                case UITransitionStyle.SlideFromLeft:
                    UITweener.Run(this, showDuration, EaseType.OutCubic, t =>
                    {
                        if (cg != null) cg.alpha = t;
                        if (rt != null) rt.anchoredPosition = Vector2.Lerp(_restPos + Vector2.left * slideDistance, _restPos, t);
                    }, onComplete);
                    break;

                case UITransitionStyle.SlideFromBottom:
                    UITweener.Run(this, showDuration, EaseType.OutCubic, t =>
                    {
                        if (cg != null) cg.alpha = t;
                        if (rt != null) rt.anchoredPosition = Vector2.Lerp(_restPos + Vector2.down * slideDistance, _restPos, t);
                    }, onComplete);
                    break;

                default:
                    UITweener.Run(this, showDuration, EaseType.OutCubic, t =>
                    {
                        if (cg != null) cg.alpha = t;
                    }, onComplete);
                    break;
            }
        }

        public void PlayHide(Action onComplete = null)
        {
            CaptureRestPose();
            var rt = Target;
            var cg = Group;

            UITweener.Run(this, hideDuration, EaseType.InCubic, t =>
            {
                if (cg != null) cg.alpha = 1f - t;
                if (rt != null && style == UITransitionStyle.PopBounce)
                    rt.localScale = Vector3.Lerp(_restScale, _restScale * 0.82f, t);
                else if (rt != null && style == UITransitionStyle.ScrollUnroll)
                    rt.localScale = new Vector3(_restScale.x, Mathf.Lerp(_restScale.y, 0.25f, t), _restScale.z);
            }, () =>
            {
                if (rt != null)
                {
                    rt.localScale = _restScale;
                    rt.anchoredPosition = _restPos;
                }
                onComplete?.Invoke();
            });
        }
    }
}
