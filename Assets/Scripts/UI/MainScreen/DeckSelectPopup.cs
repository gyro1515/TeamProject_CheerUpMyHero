using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSelectPopup : BaseUI
{
   [SerializeField] private CanvasGroup _canvasGroup;

    private void Awake()
    {
        if(_canvasGroup == null)
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        FadeManager.FadeInUI(_canvasGroup);
    }

    public override void CloseUI()
    {
        FadeManager.FadeOutUI(_canvasGroup);
    }
}
