using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeEffectManager : SingletonMono<FadeEffectManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    public void FadeInUI(CanvasGroup target)
    {
        target.DOFade(1f, 0.3f).SetUpdate(true);
        target.interactable = true;
        target.blocksRaycasts = true;
    }

    public void FadeOutUI(CanvasGroup target)
    {
        target.DOFade(0f, 0.3f).SetUpdate(true);
        target.interactable = false;
        target.blocksRaycasts = false;
    }
}
