using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyUI.Core
{
    /// <summary>
    /// Central coordinator for all registered UIViews and the stacked Modal navigation system.
    /// Zero external dependencies; auto-discovers child UIViews on startup.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool autoDiscoverInScene = true;
        [SerializeField] private bool handleEscapeKey = true;

        private readonly Dictionary<string, UIView> _viewsById = new Dictionary<string, UIView>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Type, UIView> _viewsByType = new Dictionary<Type, UIView>();
        private readonly List<UIModal> _modalStack = new List<UIModal>();

        public int ActiveModalCount => _modalStack.Count;
        public UIModal TopModal => _modalStack.Count > 0 ? _modalStack[_modalStack.Count - 1] : null;

        public event Action<UIModal> OnModalOpened;
        public event Action<UIModal> OnModalClosed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (autoDiscoverInScene)
                RegisterAllSceneViews();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (!handleEscapeKey) return;
            if (Input.GetKeyDown(KeyCode.Escape) && _modalStack.Count > 0)
            {
                var top = TopModal;
                if (top != null && top.DismissOnEscape)
                    CloseModal(top);
            }
        }

        public void RegisterAllSceneViews()
        {
            var views = FindObjectsByType<UIView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var v in views)
                RegisterView(v);
        }

        public void RegisterView(UIView view)
        {
            if (view == null) return;
            _viewsById[view.ViewId] = view;
            _viewsByType[view.GetType()] = view;
        }

        public T GetView<T>() where T : UIView
        {
            if (_viewsByType.TryGetValue(typeof(T), out var view))
                return view as T;
            var found = FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (found != null) RegisterView(found);
            return found;
        }

        public UIView GetView(string viewId)
        {
            _viewsById.TryGetValue(viewId, out var view);
            return view;
        }

        public T OpenModal<T>(bool closeOthers = false) where T : UIModal
        {
            var modal = GetView<T>();
            if (modal != null) OpenModal(modal, closeOthers);
            return modal;
        }

        public void OpenModal(UIModal modal, bool closeOthers = false)
        {
            if (modal == null) return;
            if (closeOthers) CloseAllModals(instant: true);

            _modalStack.Remove(modal);
            _modalStack.Add(modal);
            modal.transform.SetAsLastSibling();
            modal.Show();
            UpdateTimeScale();
            OnModalOpened?.Invoke(modal);
        }

        public void CloseModal(UIModal modal, bool instant = false)
        {
            if (modal == null) return;
            _modalStack.Remove(modal);
            modal.Hide(instant);
            UpdateTimeScale();
            OnModalClosed?.Invoke(modal);
        }

        public void CloseAllModals(bool instant = false)
        {
            for (int i = _modalStack.Count - 1; i >= 0; i--)
            {
                var m = _modalStack[i];
                if (m != null) m.Hide(instant);
            }
            _modalStack.Clear();
            UpdateTimeScale();
        }

        private void UpdateTimeScale()
        {
            bool shouldPause = false;
            for (int i = 0; i < _modalStack.Count; i++)
            {
                if (_modalStack[i] != null && _modalStack[i].PauseTimeScaleWhenOpen)
                {
                    shouldPause = true;
                    break;
                }
            }
            Time.timeScale = shouldPause ? 0f : 1f;
        }
    }
}
