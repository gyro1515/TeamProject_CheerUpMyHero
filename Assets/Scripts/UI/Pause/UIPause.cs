using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIPause : BaseUI
{
    [Header("일시정지 버튼")]
    [SerializeField] private Button _pauseButton;

    [Header("환경설정 메뉴")]
    [SerializeField] private GameObject _settingPanel;
    [SerializeField] private UISettingMenu _settingMenuScript;

    private CanvasGroup _settingPanelCanvasGroup;

    private void Awake()
    {
        _pauseButton.onClick.AddListener(OnPauseButtonClicked);

        _settingPanelCanvasGroup = _settingPanel.GetComponent<CanvasGroup>();
    }

    private void OnPauseButtonClicked()
    {
        Time.timeScale = 0.0f;
        _settingPanel.SetActive(true);
        _settingPanelCanvasGroup.alpha = 0.0f;
        _settingPanelCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
        _settingPanelCanvasGroup.interactable = true;
        _settingPanelCanvasGroup.blocksRaycasts = true;
    }
}
