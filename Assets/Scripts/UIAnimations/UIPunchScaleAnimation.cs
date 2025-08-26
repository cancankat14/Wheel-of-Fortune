// Assets/Scripts/UI/Animations/UIPunchScaleAnimation.cs
using System;
using UnityEngine;
using DG.Tweening;

namespace Vertigo.Wheel
{
    [DisallowMultipleComponent]
    public class UIPunchScaleAnimation : MonoBehaviour, IUIAnimation
    {
        [Header("Target (optional)")]
        [SerializeField] RectTransform target;   // defaults to self

        [Header("Punch Settings")]
        [Tooltip("How big the pop is (relative to current scale).")]
        [SerializeField] float punchAmount = 0.2f;      // 20% pop
        [SerializeField] float duration    = 0.25f;
        [SerializeField] int   vibrato     = 10;
        [SerializeField] float elasticity  = 0.6f;

        [Header("Options")]
        [SerializeField] bool  ignoreTimeScale = true;
        [SerializeField] bool  playOnEnable    = false;

        Tweener tween;
        Vector3 originalScale;

        public Action OnComplete { get; set; }

        void Awake()
        {
            if (!target) target = transform as RectTransform;
            if (target) originalScale = target.localScale;
        }

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        void OnDisable()
        {
            Stop();
            if (target) target.localScale = originalScale;
        }

        public void Play()
        {
            if (!target) return;

            Stop();
            originalScale = target.localScale;

            tween = target
                .DOPunchScale(new Vector3(punchAmount, punchAmount, 0f), duration, vibrato, elasticity)
                .SetUpdate(ignoreTimeScale)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
                .OnComplete(() =>
                {
                    // hard snap back to the original to avoid drift
                    target.localScale = originalScale;
                    OnComplete?.Invoke();
                });
        }

        public void Stop()
        {
            if (tween != null && tween.IsActive())
                tween.Kill(false);
            tween = null;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!target) target = transform as RectTransform;
        }
#endif
    }
}
