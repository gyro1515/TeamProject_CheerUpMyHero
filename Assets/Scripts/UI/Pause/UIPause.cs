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
    [SerializeField] private GameObject _SettingPanel;

    private void Awake()
    {
        _pauseButton.onClick.AddListener(OnPauseButtonClicked);
    }

    private void OnPauseButtonClicked()
    {
        Time.timeScale = 0.0f;
        _SettingPanel.SetActive(true);
    }
}
