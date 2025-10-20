using UnityEngine;
using System.Collections;

public class DeckNameEditPanel : BaseUI, IBackButtonHandler
{
    private CanvasGroup _canvasGroup;
    [SerializeField] DeckPresetController _controller;
    bool _isFade = false;

    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
    }

    public override void OpenUI()
    {
        if (_isFade) return;
        base.OpenUI();
        _isFade = true;
        FadeManager.FadeInUI(_canvasGroup, SetFadeFalse);
        UIManager.PubishAddUIStackEvent(this);
    }

    public override void CloseUI()
    {
        if (_isFade) return;
        _isFade = true;
        FadeManager.FadeOutUI(_canvasGroup, SetFadeFalse);
        UIManager.PublishRemoveUIStackEvent();
        _controller.ExitEditMode();
    }
    void SetFadeFalse()
    {
        _isFade = false;
    }

    public void OnBackPressed()
    {
        Debug.Log($"{gameObject.name} 뒤로가기: ");
        CloseUI();
        //_controller.ExitEditMode();
    }
}