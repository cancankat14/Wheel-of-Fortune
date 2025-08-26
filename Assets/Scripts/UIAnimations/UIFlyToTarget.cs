using System;
using UnityEngine;
using DG.Tweening;

namespace Vertigo.Wheel
{ 
    [DisallowMultipleComponent]
public class UIFlyToTargetExact : MonoBehaviour,IUIAnimation
{
    public RectTransform target;        // ui_chest_anchor
    public RectTransform animLayer;     // ui_anim_layer (stretch full)
    private Canvas rootCanvas;           // same Canvas

    public float duration = 0.65f;
    public bool  ignoreTimeScale = true;
    public bool  deactivateOnComplete = true;

    RectTransform card;
    Sequence seq;

    public bool playAtOnEnable = false;
    void Awake()
    {
        card = (RectTransform)transform;
        if (UIManager.Instance!=null)
        {
            rootCanvas = UIManager.Instance.MainCanvas;
        }
        else
        {
            rootCanvas = GetComponentInParent<Canvas>();
        }

        if (target == null)
        {
            target = UIManager.Instance.ChestAnchorTransform as RectTransform;
        }
       
    }

    void OnDisable(){ seq?.Kill(false); seq = null; }
    void OnDestroy(){ seq?.Kill(false); seq = null; }

    public void Stop()
    {
        seq?.Kill(false); seq = null;
    }

    private void OnEnable()
    {
        if (playAtOnEnable)
        {
            Play();
        }
    }

    public void Play()
    {
        if (!card || !target || !animLayer || !rootCanvas) return;

        // Make sure all layouts are up-to-date this frame
        Canvas.ForceUpdateCanvases();

        // Put the card in the same stable space we’ll compute into
        card.SetParent(animLayer, worldPositionStays:false);

        // Ensure card uses center anchors so anchoredPosition == local space
        card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
        card.pivot     = new Vector2(0.5f, 0.5f);
        card.localScale = Vector3.one;

        // Compute chest center in animLayer local space (NO screen math)
        Vector3 worldCenter = target.TransformPoint(target.rect.center);
        Vector3 local       = animLayer.InverseTransformPoint(worldCenter);
        Vector2 dest        = new Vector2(local.x, local.y);

        // Kill any previous tweens on this card
        DOTween.Kill(card, false);
        seq?.Kill(false);

        // Tween linearly so it finishes exactly at 'duration'
        var moveT  = card.DOAnchorPos(dest, duration, snapping: true).SetEase(Ease.Linear);
        var scaleT = card.DOScale(0f, duration).SetEase(Ease.Linear);
        if (ignoreTimeScale) { moveT.SetUpdate(true); scaleT.SetUpdate(true); }

        seq = DOTween.Sequence().Join(moveT).Join(scaleT)
              .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
              .OnComplete(() =>
              {
                  // Final hard snap (removes any floating residue)
                  card.anchoredPosition = dest;
                  card.localScale = Vector3.zero;
                  if (deactivateOnComplete && this) gameObject.SetActive(false);
                  OnComplete?.Invoke();
              });
    }
    public Action OnComplete { get; set; }
}
}


