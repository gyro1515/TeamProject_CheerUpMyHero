using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGiveUpPanel : BaseUI
{
    [Header("버튼")]
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    [Header("패널")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _giveUpWarningPanel;
    [SerializeField] private GameObject _giveUpPanel;

    private void Awake()
    {
        _yesButton.onClick.AddListener(OnGiveUpYesButtonClicked);
        _noButton.onClick.AddListener(OnGiveUpNoButtonClicked);
    }

    private void OnGiveUpYesButtonClicked()
    {
        _pausePanel.SetActive(false);
        _giveUpWarningPanel.SetActive(false);
        _giveUpPanel.SetActive(false);
        Time.timeScale = 1.0f;
        Debug.Log("스테이지 실패 UI 뜨도록 하기");
    }

    private void OnGiveUpNoButtonClicked()
    {
        _pausePanel.SetActive(false);
        _giveUpWarningPanel.SetActive(false);
        _giveUpPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }
}
