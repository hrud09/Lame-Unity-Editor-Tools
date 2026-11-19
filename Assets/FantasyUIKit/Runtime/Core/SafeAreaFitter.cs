using UnityEngine;

namespace FantasyUI.Core
{
    /// <summary>
    /// Adapts a full-stretch HUD container to Screen.safeArea for notches, punch-holes, and ultrawide devices.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        [SerializeField] private bool conformX = true;
        [SerializeField] private bool conformY = true;

        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        private void Awake() => _rect = GetComponent<RectTransform>();
        private void OnEnable() => Apply();

        private void Update()
        {
            if (_lastSafeArea != Screen.safeArea || _lastScreen.x != Screen.width || _lastScreen.y != Screen.height)
                Apply();
        }

        public void Apply()
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();
            if (Screen.width <= 0 || Screen.height <= 0) return;

            var safe = Screen.safeArea;
            _lastSafeArea = safe;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);

            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x = conformX ? min.x / Screen.width : 0f;
            min.y = conformY ? min.y / Screen.height : 0f;
            max.x = conformX ? max.x / Screen.width : 1f;
            max.y = conformY ? max.y / Screen.height : 1f;

            _rect.anchorMin = min;
            _rect.anchorMax = max;
        }
    }
}
