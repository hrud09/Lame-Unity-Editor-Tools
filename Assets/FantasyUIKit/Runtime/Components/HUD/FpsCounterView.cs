using UnityEngine;
using TMPro;

namespace FantasyUI.Components
{
    /// <summary>
    /// Compact stone-framed FPS & frame-time counter (upgraded from GameplayScene Temporary Tools Canvas).
    /// </summary>
    public class FpsCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text fpsText;
        [SerializeField] private float refreshInterval = 0.25f;

        private float _accum;
        private int _frames;
        private float _timer;

        public void BindReferences(TMP_Text label) => fpsText = label;

        private void Update()
        {
            _accum += Time.unscaledDeltaTime;
            _frames++;
            _timer += Time.unscaledDeltaTime;
            if (_timer < refreshInterval) return;

            float fps = _frames / Mathf.Max(0.0001f, _accum);
            if (fpsText != null)
                fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";

            _accum = 0f;
            _frames = 0;
            _timer = 0f;
        }
    }
}
