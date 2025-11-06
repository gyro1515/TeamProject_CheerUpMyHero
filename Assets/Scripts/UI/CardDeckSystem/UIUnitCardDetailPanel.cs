using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUnitCardDetailPanel : BasePopUpUI
{
    float fadeTime = 0.05f;
    public void Init()
    {
        _isFade = false;
        if (_canvasGroup) return;
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
    public override void OpenUI()
    {
        if (_isFade) return;
        gameObject.SetActive(true);
        _isFade = true;
        FadeManager.FadeInUI(_canvasGroup, SetFadeFalse, true, fadeTime);
    }
    public override void CloseUI()
    {
        if (_isFade) return;
        _isFade = true;
        FadeManager.FadeOutUI(_canvasGroup, () => { gameObject.SetActive(false); SetFadeFalse(); }, true, fadeTime);
    }   
}
