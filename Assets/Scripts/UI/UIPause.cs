using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIPause : BaseUI
{
    [Header("일시정지 버튼")]
    [SerializeField] private Button _pauseButton;

    [Header("일시정지 메뉴")]
    [SerializeField] private GameObject _pausePanel;

    private void Awake()
    {
        _pauseButton.onClick.AddListener(OnPauseButtonClicked);
    }

    private void OnPauseButtonClicked()
    {
        Time.timeScale = 0.0f;
        _pausePanel.SetActive(true);
    }
}
