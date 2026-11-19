using System;
using System.Collections.Generic;
using UnityEngine;

namespace FantasyUI.Animation
{
    public enum EaseType
    {
        Linear,
        OutCubic,
        InCubic,
        InOutQuad,
        OutBack,
        InBack,
        OutElastic
    }

    /// <summary>
    /// Zero-dependency, unscaled-time UI animation engine so modals animate smoothly even while paused (Time.timeScale == 0).
    /// </summary>
    public class UITweener : MonoBehaviour
    {
        private static UITweener _instance;
        public static UITweener Instance
        {
            get
            {
                if (_instance == null && Application.isPlaying)
                {
                    var go = new GameObject("[FantasyUI Tweener]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<UITweener>();
                }
                return _instance;
            }
        }

        private class TweenJob
        {
            public object Target;
            public float Elapsed;
            public float Duration;
            public EaseType Ease;
            public Action<float> OnUpdate;
            public Action OnComplete;
        }

        private readonly List<TweenJob> _jobs = new List<TweenJob>(32);

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = _jobs.Count - 1; i >= 0; i--)
            {
                var job = _jobs[i];
                if (job.Target is UnityEngine.Object uo && uo == null)
                {
                    _jobs.RemoveAt(i);
                    continue;
                }

                job.Elapsed += dt;
                float rawT = job.Duration <= 0.0001f ? 1f : Mathf.Clamp01(job.Elapsed / job.Duration);
                float eased = Evaluate(job.Ease, rawT);
                job.OnUpdate?.Invoke(eased);

                if (rawT >= 1f)
                {
                    _jobs.RemoveAt(i);
                    job.OnComplete?.Invoke();
                }
            }
        }

        public static void CancelTarget(object target)
        {
            if (_instance == null || target == null) return;
            for (int i = _instance._jobs.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_instance._jobs[i].Target, target))
                    _instance._jobs.RemoveAt(i);
            }
        }

        public static void Run(object target, float duration, EaseType ease, Action<float> onUpdate, Action onComplete = null, bool replaceExisting = true)
        {
            var inst = Instance;
            if (inst == null)
            {
                onUpdate?.Invoke(1f);
                onComplete?.Invoke();
                return;
            }
            if (replaceExisting && target != null) CancelTarget(target);
            inst._jobs.Add(new TweenJob
            {
                Target = target,
                Duration = duration,
                Ease = ease,
                OnUpdate = onUpdate,
                OnComplete = onComplete
            });
        }

        public static float Evaluate(EaseType ease, float t)
        {
            switch (ease)
            {
                case EaseType.OutCubic:
                    return 1f - Mathf.Pow(1f - t, 3f);
                case EaseType.InCubic:
                    return t * t * t;
                case EaseType.InOutQuad:
                    return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
                case EaseType.OutBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                }
                case EaseType.InBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    return c3 * t * t * t - c1 * t * t;
                }
                case EaseType.OutElastic:
                {
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    const float c4 = (2f * Mathf.PI) / 3f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
                }
                default:
                    return t;
            }
        }
    }
}
