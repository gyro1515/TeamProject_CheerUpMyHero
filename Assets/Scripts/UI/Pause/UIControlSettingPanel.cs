using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIControlSettingPanel : BaseUI
{
    [Header("패널 변경 버튼")]
    [SerializeField] private Button _changeButton;

    [Header("돌아가기 버튼")]
    [SerializeField] private Button _cancelButton;

    [Header("조작 패널 옵션 미리보기 이미지")]
    [SerializeField] private Image _currentLayoutImage;
    [SerializeField] private Image _nextLayoutImage;

    [Header("조작 패널 ")]
    [SerializeField] private Sprite _defaultLayoutSprite;
    [SerializeField] private Sprite _changedLayoutSprite;

    private bool isMoveButtonTop;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _changeButton.onClick.AddListener(OnChangeButtonClicked);
        _cancelButton.onClick.AddListener(OnCancelButtonClicked);

        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        SettingDataManager.OnControlLayoutChanged += UpdatePreviewUI;
    }

    private void Start()
    {
        UpdatePreviewUI();
    }

    private void OnDisable()
    {
        SettingDataManager.OnControlLayoutChanged -= UpdatePreviewUI;
    }

    private void OnChangeButtonClicked()
    {
        int currentType = SettingDataManager.Instance.ControlPanelLayoutType;
        int nextType = 1 - currentType;     // currentType이 1이면 0, 0이면 1
        SettingDataManager.Instance.SetLayoutSetting(nextType);

        FadeManager.FadeOutUI(_canvasGroup);
    }

    private void OnCancelButtonClicked()
    {
        FadeManager.FadeOutUI(_canvasGroup);
    }

    private void UpdatePreviewUI()
    {
        int currentType = SettingDataManager.Instance.ControlPanelLayoutType;

        if (currentType == 0)
        {
            _currentLayoutImage.sprite = _defaultLayoutSprite;
            _nextLayoutImage.sprite = _changedLayoutSprite;
        }
        else
        {
            _currentLayoutImage.sprite = _changedLayoutSprite;
            _nextLayoutImage.sprite = _defaultLayoutSprite;
        }
    }
}
