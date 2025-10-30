using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIExitPanel : BaseUI
{
    [Header("버튼")]
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    [Header("패널")]
    [SerializeField] private CanvasGroup _settingMenuPanel;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _yesButton.onClick.AddListener(OnExitYesButtonClicked);
        _noButton.onClick.AddListener(OnExitNoButtonClicked);

        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnExitYesButtonClicked()
    {
#if UNITY_EDITOR
        // 에디터에서는 플레이 모드를 종료
        EditorApplication.isPlaying = false;
#else
        // 실제 빌드된 환경에서는 애플리케이션 종료
        Application.Quit();
#endif
    }

    private void OnExitNoButtonClicked()
    {
        FadeManager.FadeOutUI(_canvasGroup);
        FadeManager.FadeOutUI(_settingMenuPanel);
        CloseUI();
    }
}
