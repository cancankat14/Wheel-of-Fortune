using System;
using UnityEngine;
using DG.Tweening;

namespace Vertigo.Wheel
{
    [DisallowMultipleComponent]
    public class UIScalePop : MonoBehaviour, IUIAnimation
    {
        [Header("Pop Settings")] [SerializeField]
        float scaleUpFactor = 1.08f; // how big it gets

        [SerializeField] float duration = 0.15f; // how fast it grows
        [SerializeField] float startDelay = 0f;
        [SerializeField] bool ignoreTimeScale = true;
        [SerializeField] Ease easeUp = Ease.OutBack;

        public Action OnComplete { get; set; }

        Sequence _seq;
        Vector3 _baseScale;
        RectTransform _rt;

        void Awake()
        {
            _rt = transform as RectTransform;
            _baseScale = transform.localScale; // remember original size
        }

        void OnDisable()
        {
            _seq?.Kill(false);
            _seq = null;
        }

        public void Play()
        {
            Stop(); 
            var t = transform;

            _seq = DOTween.Sequence()
                .SetUpdate(ignoreTimeScale)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

            if (startDelay > 0f) _seq.AppendInterval(startDelay);

            _seq.Append(t.DOScale(_baseScale * scaleUpFactor, duration).SetEase(easeUp))
                .OnComplete(() => OnComplete?.Invoke());
        }

        public void Stop()
        {
            _seq?.Kill(false);
            _seq = null;
            if (transform) transform.localScale = _baseScale; // reset for next spin/zone
        }
    }
}