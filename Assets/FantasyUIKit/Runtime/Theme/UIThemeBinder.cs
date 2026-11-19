using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FantasyUI.Theme
{
    /// <summary>
    /// Attach to any Graphic (Image, TMP_Text, RawImage) to bind its color and/or display font
    /// dynamically to semantic tokens in the active UIThemeSO.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UIThemeBinder : MonoBehaviour
    {
        [SerializeField] private UIThemeColorToken colorToken = UIThemeColorToken.None;
        [SerializeField] private bool preserveAlpha = false;
        [SerializeField] private bool applyDisplayFont = false;
        [SerializeField] private bool applyOutlineMaterial = false;

        private Graphic _graphic;
        private TMP_Text _tmpText;

        private void OnEnable()
        {
            CacheTargets();
            if (UIThemeManager.Instance != null)
                UIThemeManager.Instance.RegisterBinder(this);
        }

        private void OnDisable()
        {
            if (UIThemeManager.Instance != null)
                UIThemeManager.Instance.UnregisterBinder(this);
        }

        private void CacheTargets()
        {
            if (_graphic == null) _graphic = GetComponent<Graphic>();
            if (_tmpText == null) _tmpText = GetComponent<TMP_Text>();
        }

        public void Configure(UIThemeColorToken token, bool keepAlpha = false, bool bindFont = false, bool bindOutlineMat = false)
        {
            colorToken = token;
            preserveAlpha = keepAlpha;
            applyDisplayFont = bindFont;
            applyOutlineMaterial = bindOutlineMat;
            if (UIThemeManager.Instance != null && UIThemeManager.Instance.ActiveTheme != null)
                ApplyTheme(UIThemeManager.Instance.ActiveTheme);
        }

        public void ApplyTheme(UIThemeSO theme)
        {
            if (theme == null) return;
            CacheTargets();

            if (colorToken != UIThemeColorToken.None && _graphic != null)
            {
                Color c = theme.ResolveColor(colorToken);
                if (preserveAlpha) c.a = _graphic.color.a;
                _graphic.color = c;
            }

            if (_tmpText != null)
            {
                if (applyDisplayFont && theme.displayFont != null)
                    _tmpText.font = theme.displayFont;
                if (applyOutlineMaterial && theme.displayOutlineMaterial != null)
                    _tmpText.fontSharedMaterial = theme.displayOutlineMaterial;
            }
        }
    }
}
