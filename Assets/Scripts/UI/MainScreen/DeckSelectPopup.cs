using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSelectPopup : BaseUI, IBackButtonHandler
{
   [SerializeField] private CanvasGroup _canvasGroup;

    //bool isInit = false;
    
    private void Awake()
    {
        if(_canvasGroup == null)
        _canvasGroup = GetComponent<CanvasGroup>();

        //if(gameObject.activeSelf) base.CloseUI();
        
    }
    private void OnEnable()
    {
        _canvasGroup.alpha = 0.0f;
        //CloseUI(); 이걸 안 쓰는 이유는 CloseUI()는 페이드 아웃을 하기 때문에
        
        UIManager.PubishAddUIStackEvent(this);
        //if (!isInit)
        //{
        //    isInit = true;
        //    base.CloseUI();
        //}
    }
    private void OnDisable()
    {
        UIManager.PublishRemoveUIStackEvent();
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
        FadeManager.FadeOutUI(_canvasGroup, base.CloseUI);
    }

    public void OnBackPressed()
    {
        Debug.Log("DeckSelectPopup: 뒤로가기 버튼 눌림");
        CloseUI();
    }
}
