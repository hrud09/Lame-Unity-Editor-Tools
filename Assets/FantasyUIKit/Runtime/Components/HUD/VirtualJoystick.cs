using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FantasyUI.Components
{
    /// <summary>
    /// Responsive Fixed or Floating Fantasy Pirate Helm Virtual Joystick.
    /// Rotates the sculpted helm handle naturally with horizontal steering input.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float handleRange = 64f;
        [SerializeField] private float deadZone = 0.08f;
        [SerializeField] private bool rotateHelmWithInput = true;
        [SerializeField] private float maxHelmAngle = 75f;

        private Vector2 _input;
        private Canvas _rootCanvas;
        private Camera _uiCamera;

        public Vector2 InputVector => _input;
        public float Horizontal => _input.x;
        public float Vertical => _input.y;

        public event Action<Vector2> OnJoystickMoved;

        private void Awake()
        {
            _rootCanvas = GetComponentInParent<Canvas>();
            if (_rootCanvas != null && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                _uiCamera = _rootCanvas.worldCamera;
        }

        public void BindReferences(RectTransform bg, RectTransform hdl, float range = 64f)
        {
            background = bg;
            handle = hdl;
            handleRange = range;
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (background == null || handle == null) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, _uiCamera, out var localPoint))
                return;

            Vector2 normalized = localPoint / Mathf.Max(1f, handleRange);
            if (normalized.magnitude > 1f) normalized = normalized.normalized;
            _input = normalized.magnitude < deadZone ? Vector2.zero : normalized;

            handle.anchoredPosition = _input * handleRange;
            if (rotateHelmWithInput)
                handle.localRotation = Quaternion.Euler(0f, 0f, -_input.x * maxHelmAngle);

            OnJoystickMoved?.Invoke(_input);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _input = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
                handle.localRotation = Quaternion.identity;
            }
            OnJoystickMoved?.Invoke(Vector2.zero);
        }
    }
}
