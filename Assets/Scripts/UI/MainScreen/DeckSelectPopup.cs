using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSelectPopup : BaseUI
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public override void OpenUI()
    {
        base.OpenUI();
        FadeManager.Instance.FadeInUI(_canvasGroup);
    }

    public override void CloseUI()
    {
        FadeManager.Instance.FadeOutUI(_canvasGroup);
    }
}
