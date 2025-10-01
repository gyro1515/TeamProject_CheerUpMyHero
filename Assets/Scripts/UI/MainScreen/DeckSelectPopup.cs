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
    private void OnEnable()
    {
        _canvasGroup.alpha = 0.0f;
        //CloseUI(); 이걸 안 쓰는 이유는 CloseUI()는 페이드 아웃을 하기 때문에
    }
    public override void OpenUI()
    {
        base.OpenUI();
        if (_canvasGroup == null) { Debug.LogWarning("OpenUI: 캔버스 그룹 없음"); return; }
        FadeManager.FadeInUI(_canvasGroup);
    }

    public override void CloseUI()
    {
        if (_canvasGroup == null) { Debug.LogWarning("CloseUI: 캔버스 그룹 없음"); return; }
        FadeManager.FadeOutUI(_canvasGroup);
    }
}
