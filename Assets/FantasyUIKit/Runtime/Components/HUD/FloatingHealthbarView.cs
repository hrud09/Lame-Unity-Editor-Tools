using UnityEngine;
using UnityEngine.UI;

namespace FantasyUI.Components
{
    /// <summary>
    /// Screen-space target-following floating healthbar & offscreen directional enemy edge indicator.
    /// Covers GameplayScene's Healthbar Show Canvas & Enemy Mark Show Canvas.
    /// </summary>
    public class FloatingHealthbarView : MonoBehaviour
    {
        [SerializeField] private FantasyProgressBar progressBar;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
        [SerializeField] private Camera worldCamera;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = transform as RectTransform;
            if (progressBar == null) progressBar = GetComponent<FantasyProgressBar>();
            if (worldCamera == null) worldCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (followTarget == null || worldCamera == null || _rect == null) return;
            Vector3 screenPos = worldCamera.WorldToScreenPoint(followTarget.position + worldOffset);
            bool visible = screenPos.z > 0f;
            gameObject.SetActive(visible);
            if (visible) _rect.position = screenPos;
        }

        public void SetTarget(Transform target, float current, float max)
        {
            followTarget = target;
            progressBar?.SetValue(current, max, instant: true);
        }

        public void UpdateHealth(float current, float max) => progressBar?.SetValue(current, max);
    }

    public class OffscreenEdgeMarker : MonoBehaviour
    {
        [SerializeField] private RectTransform arrowRect;
        [SerializeField] private Transform target;
        [SerializeField] private float edgePadding = 56f;
        [SerializeField] private Camera worldCamera;

        private RectTransform _selfRect;

        private void Awake()
        {
            _selfRect = transform as RectTransform;
            if (worldCamera == null) worldCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (target == null || worldCamera == null || _selfRect == null) return;
            Vector3 sp = worldCamera.WorldToScreenPoint(target.position);
            bool onScreen = sp.z > 0 && sp.x > 0 && sp.x < Screen.width && sp.y > 0 && sp.y < Screen.height;
            if (arrowRect != null) arrowRect.gameObject.SetActive(!onScreen);
            if (onScreen) return;

            if (sp.z < 0) sp = -sp;
            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 dir = ((Vector2)sp - center).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

            Vector2 clamped = new Vector2(
                Mathf.Clamp(sp.x, edgePadding, Screen.width - edgePadding),
                Mathf.Clamp(sp.y, edgePadding, Screen.height - edgePadding));
            _selfRect.position = clamped;
            if (arrowRect != null) arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void BindReferences(RectTransform arrow) => arrowRect = arrow;
        public void SetTarget(Transform t) => target = t;
    }
}
