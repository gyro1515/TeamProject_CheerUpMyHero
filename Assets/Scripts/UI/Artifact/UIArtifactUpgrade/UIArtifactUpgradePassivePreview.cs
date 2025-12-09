using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactUpgradePassivePreview : BasePopUpUI
{
    [Header("결과 유물 표시 UI")]
    [SerializeField] private Image _resultIcon;
    [SerializeField] private Outline _resultIconOutline;

    [Header("합성 전 효과 표시 UI")]
    [SerializeField] private Image _sourceIcon;
    [SerializeField] private Outline _sourceIconOutline;
    [SerializeField] private TextMeshProUGUI _sourceEffectText;

    [Header("합성 후 효과 표시 UI")]
    [SerializeField] private Image _upgradeIcon;
    [SerializeField] private Outline _upgradeIconOutline;
    [SerializeField] private TextMeshProUGUI _upgradeEffectText;

    [Header("버튼")]
    [SerializeField] private Button _confirmButton;

    public event Action OnConfirm;

    protected override void Awake()
    {
        base.Awake();
        _confirmButton.onClick.AddListener(OnConfirmButtonClicked);
    }

    public void OpenPassivePreview(PassivePreviewViewModel vm)
    {
        _resultIcon.sprite = vm.ResultIcon;
        _resultIconOutline.effectColor = vm.ResultBorderColor;

        _sourceIcon.sprite = vm.SourceIcon;
        _sourceIconOutline.effectColor = vm.SourceBorderColor;
        _sourceEffectText.text = vm.SourceEffectText;

        _upgradeIcon.sprite = vm.ResultIcon;
        _upgradeIconOutline.effectColor = vm.ResultBorderColor;
        _upgradeEffectText.text = vm.ResultEffectText;

        OpenUI();
    }

    private void OnConfirmButtonClicked()
    {
        OnConfirm?.Invoke();
        CloseUI();
    }
}
