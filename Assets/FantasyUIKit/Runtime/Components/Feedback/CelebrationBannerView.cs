using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FantasyUI.Animation;
using FantasyUI.Data;
using FantasyUI.Theme;

namespace FantasyUI.Components
{
    /// <summary>
    /// Exact Reference Image 2 Stylized Celebration Banners ("VICTORY!", "DEFEATED!", "AMAZING!", "AWESOME!")
    /// with arched typography, neon-fantasy outer glow, sparkle burst, and auto-dismiss.
    /// </summary>
    public class CelebrationBannerView : MonoBehaviour
    {
        [SerializeField] private UIEventChannels eventChannel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text bannerText;
        [SerializeField] private UIArchedText archedText;
        [SerializeField] private Image glowBackdrop;
        [SerializeField] private RectTransform[] sparkleTransforms;
        [SerializeField] private float displayDuration = 2.0f;

        private float _hideTimer;
        private bool _active;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (eventChannel != null) eventChannel.OnCelebrationRequested += Play;
        }

        private void OnDisable()
        {
            if (eventChannel != null) eventChannel.OnCelebrationRequested -= Play;
        }

        private void Update()
        {
            if (!_active) return;
            if (sparkleTransforms != null)
            {
                for (int i = 0; i < sparkleTransforms.Length; i++)
                {
                    if (sparkleTransforms[i] != null)
                        sparkleTransforms[i].Rotate(0f, 0f, (i % 2 == 0 ? 55f : -45f) * Time.unscaledDeltaTime);
                }
            }

            _hideTimer -= Time.unscaledDeltaTime;
            if (_hideTimer <= 0f)
            {
                _active = false;
                UITweener.Run(this, 0.25f, EaseType.InCubic, t =>
                {
                    if (canvasGroup != null) canvasGroup.alpha = 1f - t;
                    transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.15f, t);
                }, () => gameObject.SetActive(false));
            }
        }

        public void BindReferences(CanvasGroup cg, TMP_Text txt, UIArchedText arch, Image glow, RectTransform[] sparkles, UIEventChannels channel = null)
        {
            canvasGroup = cg; bannerText = txt; archedText = arch;
            glowBackdrop = glow; sparkleTransforms = sparkles; eventChannel = channel;
        }

        public void Play(CelebrationType type)
        {
            gameObject.SetActive(true);
            _active = true;
            _hideTimer = displayDuration;

            string label;
            Color glowCol;
            Color topCol;
            switch (type)
            {
                case CelebrationType.Victory:
                    label = "VICTORY!";
                    glowCol = new Color32(52, 211, 139, 220);
                    topCol = new Color32(236, 255, 240, 255);
                    UIThemeManager.Instance?.PlaySfx(UISfxCue.Victory);
                    break;
                case CelebrationType.Defeated:
                    label = "DEFEATED!";
                    glowCol = new Color32(224, 71, 91, 220);
                    topCol = new Color32(255, 232, 232, 255);
                    UIThemeManager.Instance?.PlaySfx(UISfxCue.Defeated);
                    break;
                case CelebrationType.Amazing:
                    label = "AMAZING!";
                    glowCol = new Color32(169, 75, 247, 220);
                    topCol = new Color32(248, 234, 255, 255);
                    UIThemeManager.Instance?.PlaySfx(UISfxCue.Victory);
                    break;
                default:
                    label = "AWESOME!";
                    glowCol = new Color32(255, 184, 46, 220);
                    topCol = new Color32(255, 246, 214, 255);
                    UIThemeManager.Instance?.PlaySfx(UISfxCue.Victory);
                    break;
            }

            if (bannerText != null)
            {
                bannerText.text = label;
                bannerText.color = topCol;
            }
            if (glowBackdrop != null) glowBackdrop.color = glowCol;
            archedText?.ApplyCurve();

            transform.localScale = Vector3.one * 0.35f;
            UITweener.Run(this, 0.38f, EaseType.OutBack, t =>
            {
                if (canvasGroup != null) canvasGroup.alpha = Mathf.Clamp01(t * 1.5f);
                transform.localScale = Vector3.LerpUnclamped(Vector3.one * 0.35f, Vector3.one, t);
            });
        }
    }

    /// <summary>
    /// Animated Status Ribbon Toast Stack (PROGRESS!, TIME IS UP!, COLLECT NOW! messages).
    /// </summary>
    public class ToastNotificationStack : MonoBehaviour
    {
        [SerializeField] private UIEventChannels eventChannel;
        [SerializeField] private Transform toastRoot;
        [SerializeField] private GameObject toastTemplate;
        [SerializeField] private Sprite ribbonPurple;
        [SerializeField] private Sprite ribbonCrimson;
        [SerializeField] private Sprite ribbonEmerald;

        private void OnEnable()
        {
            if (eventChannel != null) eventChannel.OnToastRequested += ShowToast;
        }

        private void OnDisable()
        {
            if (eventChannel != null) eventChannel.OnToastRequested -= ShowToast;
        }

        public void BindReferences(Transform root, GameObject template, Sprite purple, Sprite crimson, Sprite emerald, UIEventChannels channel = null)
        {
            toastRoot = root; toastTemplate = template;
            ribbonPurple = purple; ribbonCrimson = crimson; ribbonEmerald = emerald;
            eventChannel = channel;
        }

        public void ShowToast(string message, QuestStatus style = QuestStatus.CollectNow)
        {
            if (toastRoot == null || toastTemplate == null) return;
            toastTemplate.SetActive(false);

            var go = Instantiate(toastTemplate, toastRoot);
            go.SetActive(true);
            var img = go.GetComponent<Image>();
            var txt = go.GetComponentInChildren<TMP_Text>();
            var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();

            if (img != null)
            {
                img.sprite = style switch
                {
                    QuestStatus.InProgress => ribbonPurple,
                    QuestStatus.TimeIsUp   => ribbonCrimson,
                    _                      => ribbonEmerald
                };
            }
            if (txt != null) txt.text = message;

            go.transform.localScale = Vector3.one * 0.7f;
            UITweener.Run(go, 0.25f, EaseType.OutBack, t =>
            {
                cg.alpha = Mathf.Clamp01(t * 1.5f);
                go.transform.localScale = Vector3.LerpUnclamped(Vector3.one * 0.7f, Vector3.one, t);
            }, () =>
            {
                UITweener.Run(cg, 1.8f, EaseType.Linear, _ => { }, () =>
                {
                    UITweener.Run(go, 0.25f, EaseType.InCubic, t2 => cg.alpha = 1f - t2, () => { if (go != null) Destroy(go); });
                });
            });
        }
    }
}
