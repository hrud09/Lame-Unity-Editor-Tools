using UnityEngine;
using UnityEngine.UI;

namespace FantasyUI.Core
{
    /// <summary>
    /// Dynamically computes and applies the exact optimal Image.pixelsPerUnitMultiplier
    /// for 9-sliced UI sprites so beveled corners, rivets, and dark-ink outlines never
    /// crush or pinch when stretched or placed inside responsive LayoutGroups.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public class UI9SliceAutoPPU : MonoBehaviour
    {
        [Tooltip("Maximum fraction of the RectTransform width/height that 9-slice borders may occupy before scaling up PPU multiplier.")]
        [Range(0.35f, 0.85f)]
        [SerializeField] private float maxBorderCoverage = 0.62f;

        [Tooltip("How strongly to scale borders down when the widget is smaller than the sprite's native height (0 = only prevent overlap, 1 = strict height ratio).")]
        [Range(0f, 1f)]
        [SerializeField] private float compactScaleStrength = 0.72f;

        [Tooltip("Optional manual multiplier bias (1 = auto).")]
        [Range(0.5f, 3f)]
        [SerializeField] private float multiplierBias = 1f;

        private Image _image;
        private RectTransform _rect;
        private Vector2 _lastSize;
        private Sprite _lastSprite;

        private void OnEnable()
        {
            Cache();
            ApplyNow();
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyNow();
        }

        private void LateUpdate()
        {
            if (_rect == null || _image == null) return;
            Vector2 sz = GetEffectiveSize(_rect);
            if (_image.sprite != _lastSprite || (sz - _lastSize).sqrMagnitude > 0.25f)
                ApplyNow();
        }

        private void Cache()
        {
            if (_image == null) _image = GetComponent<Image>();
            if (_rect == null) _rect = transform as RectTransform;
        }

        public void Configure(float coverage = 0.62f, float scaleStrength = 0.72f, float bias = 1f)
        {
            maxBorderCoverage = coverage;
            compactScaleStrength = scaleStrength;
            multiplierBias = bias;
            ApplyNow();
        }

        public void ApplyNow()
        {
            Cache();
            if (_image == null || _rect == null || _image.sprite == null) return;
            if (_image.type != Image.Type.Sliced) return;

            Vector2 size = GetEffectiveSize(_rect);
            _lastSize = size;
            _lastSprite = _image.sprite;

            if (size.x <= 1f || size.y <= 1f) return;

            float optimal = ComputeOptimalMultiplier(_image.sprite, size, maxBorderCoverage, compactScaleStrength, multiplierBias);
            if (!Mathf.Approximately(_image.pixelsPerUnitMultiplier, optimal))
                _image.pixelsPerUnitMultiplier = optimal;
        }

        public static Vector2 GetEffectiveSize(RectTransform rt)
        {
            if (rt == null) return Vector2.zero;
            Rect r = rt.rect;
            float w = r.width > 1f ? r.width : Mathf.Abs(rt.sizeDelta.x);
            float h = r.height > 1f ? r.height : Mathf.Abs(rt.sizeDelta.y);
            return new Vector2(w, h);
        }

        public static float ComputeOptimalMultiplier(
            Sprite sprite,
            Vector2 rectSize,
            float maxCoverage = 0.62f,
            float compactStrength = 0.72f,
            float bias = 1f)
        {
            if (sprite == null || rectSize.x <= 1f || rectSize.y <= 1f)
                return 1f;

            Vector4 border = sprite.border; // x=L, y=B, z=R, w=T
            float sumH = border.x + border.z;
            float sumV = border.y + border.w;

            if (sumH <= 0.01f && sumV <= 0.01f)
                return 1f;

            float cov = Mathf.Clamp(maxCoverage, 0.35f, 0.85f);
            float needH = sumH > 0f ? (sumH / (rectSize.x * cov)) : 1f;
            float needV = sumV > 0f ? (sumV / (rectSize.y * cov)) : 1f;

            float spriteH = sprite.rect.height;
            float spriteW = sprite.rect.width;
            float ratioH = (spriteH > 1f && rectSize.y < spriteH)
                ? Mathf.Lerp(1f, spriteH / rectSize.y, compactStrength)
                : 1f;
            float ratioW = (spriteW > 1f && rectSize.x < spriteW)
                ? Mathf.Lerp(1f, spriteW / rectSize.x, compactStrength * 0.5f)
                : 1f;

            float result = Mathf.Max(1f, needH, needV, ratioH, ratioW) * Mathf.Max(0.5f, bias);
            // Snap cleanly to 0.05 increments for tidy Inspector values
            result = Mathf.Round(result * 20f) / 20f;
            return Mathf.Clamp(result, 0.5f, 8f);
        }
    }
}
