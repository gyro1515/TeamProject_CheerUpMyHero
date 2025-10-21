using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class BasePopUpUI : BaseUI, IBackButtonHandler
{
    [Header("해당 UI는 팝업입니다. 작업 후 비활성화 해주세요.")]
    [SerializeField, ReadOnly] string POPUP_UI_WARNING = "해당 UI는 팝업입니다. 작업 후 비활성화 해주세요.";
    protected CanvasGroup _canvasGroup;
    bool _isFade = false;
    /*IEventPublisher<AddUIStackEvent> onAddUIStack;
    IEventPublisher<RemoveUIStackEvent> onRemoveUIStack;*/
    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        /*onAddUIStack = EventManager.GetPublisher<AddUIStackEvent>();
        onRemoveUIStack = EventManager.GetPublisher<RemoveUIStackEvent>();*/

        // 게임 오브젝트 비활성화해야 합니다. 그래야 뒤로가기가 정상 작동합니다.
    }
    protected virtual void OnEnable()
    {
        //onAddUIStack.Publish(new AddUIStackEvent { ui = this });
        UIManager.PubishAddUIStackEvent(this);
    }
    protected virtual void OnDisable()
    {
        //onRemoveUIStack.Publish(new RemoveUIStackEvent());
        UIManager.PublishRemoveUIStackEvent();
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
