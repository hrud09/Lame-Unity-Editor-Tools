using System;
using UnityEngine;
using FantasyUI.Animation;

namespace FantasyUI.Core
{
    /// <summary>
    /// Base lifecycle controller for all Fantasy UI screens, HUD modules, and popups.
    /// Supports zero-dependency animated show/hide transitions and CanvasGroup input gating.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIView : MonoBehaviour
    {
        [Header("View Identity")]
        [SerializeField] private string viewId;
        [SerializeField] private UILayer layer = UILayer.GameplayHUD;
        [SerializeField] private bool startVisible = true;

        [Header("Transition")]
        [SerializeField] private UIViewTransition transition;

        private CanvasGroup _canvasGroup;
        private bool _isVisible;
        private bool _initialized;

        public string ViewId => string.IsNullOrEmpty(viewId) ? gameObject.name : viewId;
        public UILayer Layer => layer;
        public bool IsVisible => _isVisible;
        public CanvasGroup CanvasGroup => _canvasGroup != null ? _canvasGroup : (_canvasGroup = GetComponent<CanvasGroup>());

        public event Action<UIView> OnShown;
        public event Action<UIView> OnHidden;

        protected virtual void Awake()
        {
            EnsureInit();
        }

        private void EnsureInit()
        {
            if (_initialized) return;
            _initialized = true;
            _canvasGroup = GetComponent<CanvasGroup>();
            if (transition == null) transition = GetComponent<UIViewTransition>();
            _isVisible = startVisible && gameObject.activeSelf;
        }

        public virtual void Show(bool instant = false)
        {
            EnsureInit();
            gameObject.SetActive(true);
            _isVisible = true;
            CanvasGroup.interactable = true;
            CanvasGroup.blocksRaycasts = true;

            if (transition != null && !instant && Application.isPlaying)
            {
                transition.PlayShow(() => OnShown?.Invoke(this));
            }
            else
            {
                CanvasGroup.alpha = 1f;
                OnShown?.Invoke(this);
            }
        }

        public virtual void Hide(bool instant = false)
        {
            EnsureInit();
            _isVisible = false;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;

            if (transition != null && !instant && Application.isPlaying && gameObject.activeInHierarchy)
            {
                transition.PlayHide(() =>
                {
                    if (!_isVisible)
                    {
                        gameObject.SetActive(false);
                        OnHidden?.Invoke(this);
                    }
                });
            }
            else
            {
                CanvasGroup.alpha = 0f;
                gameObject.SetActive(false);
                OnHidden?.Invoke(this);
            }
        }

        public void Toggle(bool instant = false)
        {
            if (IsVisible) Hide(instant);
            else Show(instant);
        }

        public void ConfigureView(string id, UILayer targetLayer, bool visibleByDefault)
        {
            viewId = id;
            layer = targetLayer;
            startVisible = visibleByDefault;
        }
    }
}
