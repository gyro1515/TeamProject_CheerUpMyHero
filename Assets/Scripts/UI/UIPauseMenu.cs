using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPauseMenu : BaseUI
{
    [Header("환경설정 버튼")]
    [SerializeField] private Button _SettingButton;

    [Header("기능 없는 버튼")]
    [SerializeField] private Button _1Button;
    [SerializeField] private Button _2Button;

    [Header("포기하기 버튼")]
    [SerializeField] private Button _giveUpButton;

    [Header("돌아가기 버튼")]
    [SerializeField] private Button _resumeButton;

    [Header("포기 선택 패널")]
    [SerializeField] private GameObject _giveUpPanel;

    [Header("포기 경고 패널")]
    [SerializeField] private GameObject _giveUpWarningPanel;

    private void Awake()
    {
        _SettingButton.onClick.AddListener(OnSettingButtonClicked);
        _giveUpButton.onClick.AddListener(OnGiveUpButtonClicked);
        _resumeButton.onClick.AddListener(OnResumeButtonClicked);
    }

    private void OnSettingButtonClicked()
    {
        Debug.Log("환경설정 UI 출력하기");
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
