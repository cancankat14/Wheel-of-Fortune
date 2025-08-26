
using System;
using UnityEngine;
using DG.Tweening;

namespace Vertigo.Wheel
{
    public class FadeOutAnimation : MonoBehaviour, IUIAnimation
    {
        [SerializeField] CanvasGroup cg;
        [SerializeField] float hold = 0.65f;
        [SerializeField] float fadeOut = 0.25f;
        [SerializeField] bool ignoreTimeScale = true;

        Sequence seq;
        public Action OnComplete { get; set; }

        void Reset()
        {
            if (!cg) cg = GetComponent<CanvasGroup>();
            if (!cg) cg = gameObject.AddComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            GetComponent<CanvasGroup>().alpha = 1f;
        }

        void OnDisable()
        {
            seq?.Kill(false);
            seq = null;
        }

        public void Play()
        {
            if (!cg) cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            gameObject.SetActive(true);

            seq?.Kill(false);
            seq = DOTween.Sequence()
                .AppendInterval(hold)
                .Append(cg.DOFade(0f, fadeOut))
                .OnComplete(() => OnComplete?.Invoke())
                .SetUpdate(ignoreTimeScale)
                .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        }

        public void Stop()
        {
            seq?.Kill(false);
            seq = null;
        }
    }
}