using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class BasePopUpUI : BaseUI, IBackButtonHandler
{
    protected CanvasGroup _canvasGroup;
    bool _isFade = false;
    IEventPublisher<AddUIStackEvent> onAddUIStack;
    IEventPublisher<RemoveUIStackEvent> onRemoveUIStack;
    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        onAddUIStack = EventManager.GetPublisher<AddUIStackEvent>();
        onRemoveUIStack = EventManager.GetPublisher<RemoveUIStackEvent>();
    }
    protected virtual void OnEnable()
    {
        onAddUIStack.Publish(new AddUIStackEvent { ui = this });
    }
    protected virtual void OnDisable()
    {
        onRemoveUIStack.Publish(new RemoveUIStackEvent());
    }
    public override void OpenUI()
    {
        if (_isFade) return;
        base.OpenUI();
        _isFade = true;
        FadeManager.FadeInUI(_canvasGroup, SetFadeFalse);
    }

    public override void CloseUI()
    {
        if (_isFade) return;
        _isFade = true;
        FadeManager.FadeOutUI(_canvasGroup, () => { base.CloseUI(); SetFadeFalse(); });
    }
    public virtual void OnBackPressed()
    {
        Debug.Log($"{gameObject.name} 뒤로가기: ");
        CloseUI();
    }
    void SetFadeFalse()
    {
        _isFade = false;
    }
}
