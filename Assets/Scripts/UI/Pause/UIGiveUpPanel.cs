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
    [SerializeField] private CanvasGroup _settingMenuPanel;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _yesButton.onClick.AddListener(OnGiveUpYesButtonClicked);
        _noButton.onClick.AddListener(OnGiveUpNoButtonClicked);

        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnGiveUpYesButtonClicked()
    {
        FadeEffectManager.Instance.FadeOutUI(_canvasGroup);
        FadeEffectManager.Instance.FadeOutUI(_settingMenuPanel);
        Time.timeScale = 1.0f;
        GameManager.Instance.ShowResultUI(false);
    }

    private void OnGiveUpNoButtonClicked()
    {
        FadeEffectManager.Instance.FadeOutUI(_canvasGroup);
        FadeEffectManager.Instance.FadeOutUI(_settingMenuPanel);
        Time.timeScale = 1.0f;
    }
}
