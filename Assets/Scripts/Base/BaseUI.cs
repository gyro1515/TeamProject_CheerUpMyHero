using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    public virtual void OpenUI()
    {
        gameObject.SetActive(true);
    }

    public virtual void CloseUI()
    {
        gameObject.SetActive(false);
    }

    protected virtual void FadeInUI(CanvasGroup target)
    {
        target.DOFade(1f, 0.3f).SetUpdate(true);
        target.interactable = true;
        target.blocksRaycasts = true;
    }

    protected virtual void FadeOutUI(CanvasGroup target)
    {
        target.DOFade(0f, 0.3f).SetUpdate(true);
        target.interactable = false;
        target.blocksRaycasts = false;
    }
}
