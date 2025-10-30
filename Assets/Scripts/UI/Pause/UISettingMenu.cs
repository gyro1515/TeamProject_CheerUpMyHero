using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public enum UIState
{
    Main,
    CardDeck,
    Battle
}
public class UISettingMenu : BaseUI
{
    #region 사운드 패널
    [Header("사운드 버튼")]
    [SerializeField] private Button _soundSettingButton;

    [Header("사운드 패널")]
    [SerializeField] private BasePopUpUI _soundSettingPanel;
    
    private void OnSoundSettingButtonClicked()
    {
        _soundSettingPanel.OpenUI();
        //showPanel(_soundSettingPanel);
    }
    #endregion

    #region 성능 관리 패널
    [Header("성능 관리 버튼")]
    [SerializeField] private Button _fpsSettingButton;

    [Header("성능 관리 패널")]
    [SerializeField] private BasePopUpUI _fpsSettingPanel;

    private void OnFPSSettingButtonClicked()
    {
        _fpsSettingPanel.OpenUI();
    }
    #endregion

    #region 조작 패널 변경 패널
    [Header("튜토리얼 다시 보기 패널 버튼")]
    [SerializeField] private Button _tutorialRetryButton;
    [SerializeField] private UIState uiState;

    //[Header("튜토리얼 다시보기 패널")]
    //[SerializeField] private BasePopUpUI _tutorialRetryPanel;

    private void OnTutorialRetryButtonClicked()
    {
        Debug.Log("튜토리얼 다시 보기 버튼입니다");
        switch (uiState)
        {
            case UIState.Main:
                UIManager.Instance.GetUI<UITutorialMain>().OpenUI();
                break;
            case UIState.CardDeck:
                UIManager.Instance.GetUI<UITutorialDeck>().OpenUI();
                break;
            case UIState.Battle:
                UIManager.Instance.GetUI<UITutorialBattle>().OpenUI();
                break;
            default:
                break;
        }

    }
    #endregion

    #region 전투 포기 패널
    [Header("전투 포기 버튼")]
    [SerializeField] private Button _giveUpButton;
    [SerializeField] private Button _exitButton;

    [Header("포기 선택 패널")]
    [SerializeField] private BasePopUpUI _giveUpPanel;
    [SerializeField] private BasePopUpUI _exitPanel;

    private void OnGiveUpButtonClicked()
    {
        _giveUpPanel.OpenUI();
    }
    #endregion

    #region 메인 메뉴 닫기
    [Header("돌아가기 버튼")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private BasePopUpUI _settingPanel;
    public event Action OnResumeButton;

    private void OnResumeButtonClicked()
    {
        _settingPanel.CloseUI();
        //OnResumeButton?.Invoke();
    }
    #endregion

    private List<CanvasGroup> _allPanels;
    // 이 스크립트에 캔버스 그룹이 없어서, 인스펙터창에서 직접 연결해줘야 함
    [SerializeField] CanvasGroup _canvasGroup;
    private void Awake()
    {
        _soundSettingButton.onClick.AddListener(OnSoundSettingButtonClicked);
        _fpsSettingButton.onClick.AddListener(OnFPSSettingButtonClicked);
        _tutorialRetryButton.onClick.AddListener(OnTutorialRetryButtonClicked);
        _giveUpButton.onClick.AddListener(OnGiveUpButtonClicked);
        if (_exitButton != null)
        {
            _exitButton.onClick.AddListener(OnExitButtonClicked);
        }
        else
        {
            Debug.LogWarning("_exitButton이 현재 씬에 할당(연결)되지 않았습니다.");
        }
        _resumeButton.onClick.AddListener(OnResumeButtonClicked);

        /*_allPanels = new List<CanvasGroup>
        {
            _soundSettingPanel, 
            _fpsSettingPanel,
            _controlSettingPanel,
            _giveUpPanel
        };*/

        /*foreach (CanvasGroup panel in _allPanels )
        {
            if ( panel != null )
            {
                panel.alpha = 0.0f;
                panel.interactable = false;
                panel.blocksRaycasts = false;
            }
        }*/
    }
    private void OnExitButtonClicked()
    {
        if (_exitPanel != null)
        {
            _exitPanel.OpenUI();
        }
        else
        {
            Debug.LogWarning("Exit Panel이 현재 씬에 할당(연결)되지 않았습니다.");
        }
    }
    private void OnDisable()
    {
        GameManager.IsPaused = false;
        OnResumeButton?.Invoke();
    }
    /*public void showPanel(CanvasGroup target)
    {
        foreach (CanvasGroup panel in _allPanels )
        {
            if (panel == null) continue;

            if (panel != target)
            {
                if (panel.alpha > 0.0f)
                {
                    FadeManager.FadeOutUI(panel);
                }
            }
        }

        if(target != null) FadeManager.FadeInUI(target);
    }*/
    /*public void ShowPausePanel()
    {
        _canvasGroup.alpha = 0.0f;
        _canvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        EventManager.Publish(new AddUIStackEvent { ui = this });
    }*/
    /*public void OnBackPressed()
    {
        Debug.Log("[UISettingMenu] 뒤로 가기 버튼 눌림");
        OnResumeButtonClicked();
        //EventManager.Publish(new RemoveUIStackEvent());
    }*/
}