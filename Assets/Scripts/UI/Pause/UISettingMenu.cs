using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISettingMenu : BaseUI
{
    [Header("사운드 버튼")]
    [SerializeField] private Button _SoundSettingButton;

    [Header("사운드 패널")]
    [SerializeField] private GameObject _SoundSetting;

    // ========================================================

    [Header("성능 관리 버튼")]
    [SerializeField] private Button _FPSSettingButton;

    [Header("성능 관리 패널")]
    [SerializeField] private GameObject _FPSSettingPanel;

    // ========================================================

    [Header("조작 패널 변경 버튼")]
    [SerializeField] private Button _ControlSettingButton;

    [Header("조작 패널 변경 패널")]
    [SerializeField] private GameObject _ControlSettingPanel;

    // ========================================================

    [Header("전투포기 버튼")]
    [SerializeField] private Button _giveUpButton;

    [Header("포기 선택 패널")]
    [SerializeField] private GameObject _giveUpPanel;

    // ========================================================

    [Header("포기 경고 패널")]
    [SerializeField] private GameObject _giveUpWarningPanel;

    [Header("돌아가기 버튼")]
    [SerializeField] private Button _resumeButton;

    
    private void Awake()
    {
        _SoundSettingButton.onClick.AddListener(OnSoundSettingButtonClicked);
        _FPSSettingButton.onClick.AddListener(OnFPSSettingButtonClicked);
        _ControlSettingButton.onClick.AddListener(OnControlSettingButtonClicked);
        _giveUpButton.onClick.AddListener(OnGiveUpButtonClicked);
        _resumeButton.onClick.AddListener(OnResumeButtonClicked);
    }

    private void OnSoundSettingButtonClicked()
    {
        _SoundSetting.SetActive(true);
    }

    private void OnFPSSettingButtonClicked()
    {
        _FPSSettingPanel.SetActive(true);
    }

    private void OnControlSettingButtonClicked()
    {
        _ControlSettingPanel.SetActive(true);
    }

    private void OnGiveUpButtonClicked()
    {
        _giveUpPanel.SetActive(true);
        _giveUpWarningPanel.SetActive(true);
    }

    private void OnResumeButtonClicked()
    {
        CloseUI();
        Time.timeScale = 1.0f;
    }
}
