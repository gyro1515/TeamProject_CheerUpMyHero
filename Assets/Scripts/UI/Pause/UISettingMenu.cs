using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISettingMenu : BaseUI
{
    #region 사운드 패널
    [Header("사운드 버튼")]
    [SerializeField] private Button _soundSettingButton;

    [Header("사운드 패널")]
    [SerializeField] private CanvasGroup _soundSettingPanel;

    private void OnSoundSettingButtonClicked()
    {
        showPanel(_soundSettingPanel);
    }
    #endregion

    #region 성능 관리 패널
    [Header("성능 관리 버튼")]
    [SerializeField] private Button _fpsSettingButton;

    [Header("성능 관리 패널")]
    [SerializeField] private CanvasGroup _fpsSettingPanel;

    private void OnFPSSettingButtonClicked()
    {
        showPanel(_fpsSettingPanel);
    }
    #endregion

    #region 조작 패널 변경 패널
    [Header("조작 패널 변경 버튼")]
    [SerializeField] private Button _controlSettingButton;

    [Header("조작 패널 변경 패널")]
    [SerializeField] private CanvasGroup _controlSettingPanel;

    private void OnControlSettingButtonClicked()
    {
        showPanel(_controlSettingPanel);
    }
    #endregion

    #region 전투 포기 패널
    [Header("전투 포기 버튼")]
    [SerializeField] private Button _giveUpButton;

    [Header("포기 선택 패널")]
    [SerializeField] private CanvasGroup _giveUpPanel;

    private void OnGiveUpButtonClicked()
    {
        showPanel(_giveUpPanel);
    }
    #endregion

    #region 메인 메뉴 닫기
    [Header("돌아가기 버튼")]
    [SerializeField] private Button _resumeButton;
    public event Action OnResumeButton;

    private void OnResumeButtonClicked()
    {
        FadeManager.Instance.FadeOutUI(_canvasGroup);
        OnResumeButton?.Invoke();
    }
    #endregion

    private List<CanvasGroup> _allPanels;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _soundSettingButton.onClick.AddListener(OnSoundSettingButtonClicked);
        _fpsSettingButton.onClick.AddListener(OnFPSSettingButtonClicked);
        _controlSettingButton.onClick.AddListener(OnControlSettingButtonClicked);
        _giveUpButton.onClick.AddListener(OnGiveUpButtonClicked);
        _resumeButton.onClick.AddListener(OnResumeButtonClicked);

        _canvasGroup = GetComponent<CanvasGroup>();

        _allPanels = new List<CanvasGroup>
        {
            _soundSettingPanel, 
            _fpsSettingPanel,
            _controlSettingPanel,
            _giveUpPanel
        };

        foreach (CanvasGroup panel in _allPanels )
        {
            if ( panel != null )
            {
                panel.alpha = 0.0f;
                panel.interactable = false;
                panel.blocksRaycasts = false;
            }
        }
    }

    private void showPanel(CanvasGroup target)
    {
        foreach (CanvasGroup panel in _allPanels )
        {
            if (panel == null) continue;

            if (panel != target)
            {
                if (panel.alpha > 0.0f)
                {
                    FadeManager.Instance.FadeOutUI(panel);
                }
            }
        }

        FadeManager.Instance.FadeInUI(target);
    }
}