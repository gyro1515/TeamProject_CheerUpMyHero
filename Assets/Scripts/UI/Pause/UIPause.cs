using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        
        InitSpeedBtn();
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

    [Header("속도조절 버튼")]
    [SerializeField] private TextMeshProUGUI speedText; // 배속 텍스트
    [SerializeField] private Button _speedButton;

    public enum SpeedState { X1 = 1, X2 = 2, X3 = 3 }
    public SpeedState CurrentSpeed { get; private set; } = SpeedState.X1;


    private void InitSpeedBtn()
    {
        _speedButton.onClick.AddListener(OnClickSpeed);
        _settingMenuScript.OnResumeButton += () => ApplySpeed(CurrentSpeed); // 일시정지 해제 시 현재 배속 적용
        ApplySpeed(CurrentSpeed);
    }

    private void OnClickSpeed()
    {
        if(_settingPanelCanvasGroup.interactable)
        {
            return;
        }

        ToggleSpeed();
    }

    private void ToggleSpeed()
    {
        switch (CurrentSpeed)
        {
            case SpeedState.X1:
                SetSpeed(SpeedState.X2);
                break;
            case SpeedState.X2:
                SetSpeed(SpeedState.X3);
                break;
            case SpeedState.X3:
                SetSpeed(SpeedState.X1);
                break;
        }
    }

    private void SetSpeed(SpeedState speed)
    {
        CurrentSpeed = speed;
        ApplySpeed(speed);
    }

    private void ApplySpeed(SpeedState speed)
    {
        Time.timeScale = (int)speed;
        speedText.text = $"x{(int)speed}";
        Debug.Log($"[SpeedBtn] 현재 배속: {speed}");
    }
}
