using UnityEngine;
using TMPro;

namespace FantasyUI.Animation
{
    /// <summary>
    /// Smoothly rolls numeric values (Gold, Gems, Score, HP) with optional comma formatting and punch scale feedback.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class UINumberRoller : MonoBehaviour
    {
        [SerializeField] private string format = "N0";
        [SerializeField] private string prefix = "";
        [SerializeField] private string suffix = "";
        [SerializeField] private float rollDuration = 0.45f;
        [SerializeField] private bool punchOnIncrease = true;

        private TMP_Text _label;
        private long _displayed;
        private long _target;

        private TMP_Text Label => _label != null ? _label : (_label = GetComponent<TMP_Text>());

        public void SetFormat(string numFormat, string pre = "", string suf = "")
        {
            format = numFormat;
            prefix = pre;
            suffix = suf;
            RefreshText(_displayed);
        }

        public void SetValue(long newValue, bool instant = false)
        {
            long start = _displayed;
            _target = newValue;

            if (instant || !Application.isPlaying || !gameObject.activeInHierarchy)
            {
                _displayed = newValue;
                RefreshText(_displayed);
                return;
            }

            UITweener.Run(this, rollDuration, EaseType.OutCubic, t =>
            {
                _displayed = (long)Mathf.Round(Mathf.Lerp(start, _target, t));
                RefreshText(_displayed);
            }, () =>
            {
                _displayed = _target;
                RefreshText(_displayed);
            });

            if (punchOnIncrease && newValue > start)
            {
                Transform tr = transform;
                UITweener.Run(tr, 0.22f, EaseType.OutBack, t =>
                {
                    float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.14f;
                    tr.localScale = new Vector3(s, s, 1f);
                }, () => tr.localScale = Vector3.one);
            }
        }

        private void RefreshText(long val)
        {
            if (Label != null)
                Label.text = $"{prefix}{val.ToString(format)}{suffix}";
        }
    }
}
