using DG.Tweening;
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
    [SerializeField] AudioClip openSound;
    [SerializeField] AudioClip closeSound;
    protected CanvasGroup _canvasGroup;
    protected bool _isFade = false;
    /*IEventPublisher<AddUIStackEvent> onAddUIStack;
    IEventPublisher<RemoveUIStackEvent> onRemoveUIStack;*/
    protected virtual void Awake()
    {
        POPUP_UI_WARNING = "팝업이 오류났다면, 해당 오브젝트를 활성화하고 시작했는지 체크해주세요.";
        if(_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        
        /*onAddUIStack = EventManager.GetPublisher<AddUIStackEvent>();
        onRemoveUIStack = EventManager.GetPublisher<RemoveUIStackEvent>();*/
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
        if(openSound) AudioManager.PlayOneShot(openSound, 0.3f);
        _isFade = true;
        FadeManager.FadeInUI(_canvasGroup, SetFadeFalse);
    }
    // 다른 효과 후에 페이드 인을 하고 싶을 때 사용하는 오픈 함수
    public void OpenUI(TweenCallback afterFade)
    {
        if (_isFade) return;
        base.OpenUI();
        if(openSound) AudioManager.PlayOneShot(openSound, 0.3f);
        _isFade = true;
        afterFade += SetFadeFalse;
        FadeManager.FadeInUI(_canvasGroup, afterFade);
    }
    public override void CloseUI()
    {
        if (_isFade) return;
        if(closeSound) AudioManager.PlayOneShot(closeSound, 0.3f);
        _isFade = true;
        FadeManager.FadeOutUI(_canvasGroup, () => { base.CloseUI(); SetFadeFalse(); });
    }
    public virtual void OnBackPressed()
    {
        Debug.Log($"{gameObject.name} 뒤로가기: ");
        CloseUI();
    }
    protected void SetFadeFalse()
    {
        _isFade = false;
    }
    protected void JustOpenUI() // 페이드 없이 열기
    {
        base.OpenUI();
        if(openSound) AudioManager.PlayOneShot(openSound, 0.3f);
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }
}
