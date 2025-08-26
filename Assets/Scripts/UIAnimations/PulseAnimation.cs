using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Vertigo.Wheel
{
    [DisallowMultipleComponent]
    public class PulseAnimation : MonoBehaviour, IUIAnimation
    {
        [Header("Target")]
        [SerializeField] RectTransform target;          // default = this
        [SerializeField] bool playOnEnable = false;

        [Header("Look & Feel")]
        [SerializeField, Range(0f, 0.25f)] float amplitude = 0.08f; // 8% “breath”
        [SerializeField] float period = 1.1f;                       // full up+down time (s)
        [SerializeField] bool fadeGraphic = false;                  // optional soft fade with pulse
        [SerializeField, Range(0.3f, 1f)] float minAlpha = 0.8f;    // only if fadeGraphic=true

        [Header("Timing")]
        [SerializeField] bool ignoreTimeScale = true;

        Tween scaleTween, fadeTween;
        Vector3 baseScale;
        Graphic g;

        void Awake()
        {
            if (!target) target = transform as RectTransform;
            baseScale = target.localScale;
            g = GetComponent<Graphic>(); // used if fadeGraphic and placed on Image/Text
        }

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        void OnDisable()
        {
            KillTweens();
            target.localScale = baseScale;
            if (fadeGraphic && g)
            {
                var c = g.color; c.a = 1f; g.color = c;
            }
        }

        public void Stop()
        {
            KillTweens();
            target.localScale = baseScale;
            if (fadeGraphic && g)
            {
                var c = g.color; c.a = 1f; g.color = c;
            }
        }

        public void Play()
        {
            if (!target) return;
            KillTweens();
            target.localScale = baseScale;

            float upScale = 1f + Mathf.Max(0f, amplitude);
            float half = Mathf.Max(0.05f, period * 0.5f);

            // Smooth, continuous sine-like pulse
            scaleTween = target
                .DOScale(baseScale * upScale, half)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(ignoreTimeScale)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            if (fadeGraphic && g)
            {
                fadeTween = g
                    .DOFade(minAlpha, half)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetUpdate(ignoreTimeScale)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            }
        }

        public void NudgeOnce(float overshoot = 1.12f, float upTime = 0.12f, float downTime = 0.12f)
        {
            // Optional: call on click for a quick pop, then resume pulse
            KillTweens();
            target.localScale = baseScale;
            DOTween.Sequence()
                .Append(target.DOScale(baseScale * overshoot, upTime).SetEase(Ease.OutBack))
                .Append(target.DOScale(baseScale, downTime).SetEase(Ease.InOutSine))
                .OnComplete(Play)
                .SetUpdate(ignoreTimeScale)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
        }

        void KillTweens()
        {
            scaleTween?.Kill(false); scaleTween = null;
            fadeTween?.Kill(false);  fadeTween  = null;
        }
        public Action OnComplete { get; set; }
    }
}
