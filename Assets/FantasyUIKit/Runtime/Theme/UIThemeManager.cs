using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyUI.Theme
{
    /// <summary>
    /// Applies and broadcasts active UIThemeSO changes dynamically at runtime or in Edit Mode.
    /// </summary>
    [ExecuteAlways]
    public class UIThemeManager : MonoBehaviour
    {
        public static UIThemeManager Instance { get; private set; }

        [SerializeField] private UIThemeSO activeTheme;
        [SerializeField] private UIThemeSO[] availableThemes;
        [SerializeField] private AudioSource uiAudioSource;

        private readonly HashSet<UIThemeBinder> _binders = new HashSet<UIThemeBinder>();

        public UIThemeSO ActiveTheme => activeTheme;
        public UIThemeSO[] AvailableThemes => availableThemes;

        public event Action<UIThemeSO> OnActiveThemeSwapped;

        private void OnEnable()
        {
            Instance = this;
            if (activeTheme != null)
            {
                activeTheme.OnThemeChanged -= HandleThemeAssetEdited;
                activeTheme.OnThemeChanged += HandleThemeAssetEdited;
            }
            ApplyThemeToScene();
        }

        private void OnDisable()
        {
            if (activeTheme != null)
                activeTheme.OnThemeChanged -= HandleThemeAssetEdited;
            if (Instance == this) Instance = null;
        }

        public void RegisterBinder(UIThemeBinder binder)
        {
            if (binder == null) return;
            _binders.Add(binder);
            if (activeTheme != null) binder.ApplyTheme(activeTheme);
        }

        public void UnregisterBinder(UIThemeBinder binder)
        {
            _binders.Remove(binder);
        }

        public void SetTheme(UIThemeSO theme)
        {
            if (theme == null) return;
            if (activeTheme != null) activeTheme.OnThemeChanged -= HandleThemeAssetEdited;
            activeTheme = theme;
            activeTheme.OnThemeChanged += HandleThemeAssetEdited;
            ApplyThemeToScene();
            OnActiveThemeSwapped?.Invoke(activeTheme);
        }

        public void CycleNextTheme()
        {
            if (availableThemes == null || availableThemes.Length == 0) return;
            int idx = Array.IndexOf(availableThemes, activeTheme);
            int next = (idx + 1) % availableThemes.Length;
            SetTheme(availableThemes[next]);
        }

        public void ConfigureThemes(UIThemeSO primary, UIThemeSO[] presets)
        {
            activeTheme = primary;
            availableThemes = presets;
            ApplyThemeToScene();
        }

        public void ApplyThemeToScene()
        {
            if (activeTheme == null) return;
            var all = FindObjectsByType<UIThemeBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
                all[i].ApplyTheme(activeTheme);
        }

        public void PlaySfx(UISfxCue cue)
        {
            if (activeTheme == null || activeTheme.sfxProfile == null) return;
            var clip = activeTheme.sfxProfile.GetClip(cue);
            if (clip == null) return;
            if (uiAudioSource == null)
            {
                uiAudioSource = GetComponent<AudioSource>();
                if (uiAudioSource == null) uiAudioSource = gameObject.AddComponent<AudioSource>();
                uiAudioSource.playOnAwake = false;
            }
            uiAudioSource.PlayOneShot(clip, activeTheme.sfxProfile.volume);
        }

        private void HandleThemeAssetEdited() => ApplyThemeToScene();
    }
}
