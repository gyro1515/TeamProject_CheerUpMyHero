using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactUpgradeActivePanel : BasePopUpUI
{
    [Header("현재 유물")]
    [SerializeField] private Image _currentIcon;
    [SerializeField] private TextMeshProUGUI _currentLevelText;
    [SerializeField] private TextMeshProUGUI _currentEffectText;

    [Header("강화 유물")]
    [SerializeField] private Image _nextIcon;
    [SerializeField] private TextMeshProUGUI _nextLevelText;
    [SerializeField] private TextMeshProUGUI _nextEffectText;

    [Header("강화 비용")]
    [SerializeField] private GameObject _goldCostGroup;
    [SerializeField] private TextMeshProUGUI _goldCostText;

    [SerializeField] private GameObject _woodCostGroup;
    [SerializeField] private TextMeshProUGUI _woodCostText;

    [SerializeField] private GameObject _ironCostGroup;
    [SerializeField] private TextMeshProUGUI _ironCostText;

    [SerializeField] private GameObject _magicStoneCostGroup;
    [SerializeField] private TextMeshProUGUI _magicStoneCostText;

    [Header("버튼")]
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private Button _closeButton;

    [Header("강화 확인 패널")]
    [SerializeField] private GameObject _confirmPanel;
    [SerializeField] private Button _confirmUpgradeButton;
    [SerializeField] private Button _confirmCancleButton;

    private ActiveArtifactData _selectedArtifact;

    public event Action<ActiveArtifactData> OnRequestUpgrade;
    public event Action OnRequestClose;

    protected override void Awake()
    {
        base.Awake();
        _upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        _closeButton.onClick.AddListener(OnCloseButtonClicked);

        _confirmUpgradeButton.onClick.AddListener(OnConfirmUpgradeButtonClicked);
        _confirmCancleButton.onClick.AddListener(OnCancleUpgradeButtonClicked);
        _confirmPanel.SetActive(false);
    }

    public void OpenActivePanel(ActiveUpgradeViewModel vm)
    {
        RefreshUI(vm);
        OpenUI();
    }

    public void RefreshUI(ActiveUpgradeViewModel vm)
    {
        _selectedArtifact = vm.Artifact;

        _currentIcon.sprite = vm.Icon;
        _currentLevelText.text = vm.CurrentLevelText;
        _currentEffectText.text = vm.CurrentEffectText;

        _nextIcon.sprite = vm.Icon;
        _nextLevelText.text = vm.NextLevelText;
        _nextEffectText.text = vm.NextEffectText;

        UpdateCostDisplay(vm);

        _upgradeButton.interactable = vm.CanUpgrade;
    }

    private void UpdateCostDisplay(ActiveUpgradeViewModel vm)
    {
        bool hasColdCost = !string.IsNullOrEmpty (vm.GoldCostText);
        bool hasWoodCost = !string.IsNullOrEmpty (vm.WoodCostText);
        bool hasIronCost = !string.IsNullOrEmpty (vm.IronCostText);
        bool hasMagicStoneCost = !string.IsNullOrEmpty (vm.MagicStoneCostText);

        if (hasColdCost)
        {
            _goldCostGroup.SetActive (true);
            _goldCostText.text = vm.GoldCostText;
            _goldCostText.color = vm.HasEnoughGold ? Color.black : Color.red;
        }
        else
        {
            _goldCostGroup.SetActive (false);
        }

        if (hasWoodCost)
        {
            _woodCostGroup.SetActive (true);
            _woodCostText.text = vm.WoodCostText;
            _woodCostText.color = vm.HasEnoughWood ? Color.black : Color.red;
        }
        else
        {
            _woodCostGroup?.SetActive (false);
        }

        if (hasIronCost)
        {
            _ironCostGroup.SetActive (true);
            _ironCostText.text = vm.IronCostText;
            _ironCostText.color = vm.HasEnoughIron ? Color.black : Color.red;
        }
        else
        {
            _ironCostGroup?.SetActive (false);
        }

        if (hasMagicStoneCost)
        {
            _magicStoneCostGroup.SetActive (true);
            _magicStoneCostText.text= vm.MagicStoneCostText;
            _magicStoneCostText.color = vm.HasEnoughMagicStone ? Color.black : Color.red;
        }
        else
        {
            _magicStoneCostGroup?.SetActive (false);
        }
    }

    private void OnUpgradeButtonClicked()
    {
        if (_selectedArtifact == null) return;
        _confirmPanel.SetActive (true);
    }

    private void OnCloseButtonClicked()
    {
        OnRequestClose?.Invoke();
    }

    private void OnConfirmUpgradeButtonClicked()
    {
        _confirmPanel.SetActive(false);
        if (_selectedArtifact != null)
        {
            OnRequestUpgrade?.Invoke(_selectedArtifact);
        }
    }

    private void OnCancleUpgradeButtonClicked()
    {
        _confirmPanel?.SetActive (false);
    }
}
