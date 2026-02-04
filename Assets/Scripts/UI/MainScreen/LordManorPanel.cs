using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LordManorPanel : BasePopUpUI
{
    [SerializeField] private Button _artifactInventoryPanelButton;
    [SerializeField] private Button _artifactUpgradePanelButton;
    [SerializeField] private Button _closeButton;

    private MainScreenUI _mainScreenUI;
    private UIArtifactUpgrade _artifactUpgradeUI;
    private UIArtifact _artifactUI;

    protected override void Awake()
    {
        base.Awake();

        _artifactInventoryPanelButton.onClick.AddListener(OnArtifactInventoryButtonClicked);
        _artifactUpgradePanelButton.onClick.AddListener(OnArtifactUpgradePanelButtonClicked);
        _closeButton.onClick.AddListener(CloseUI);
    }

    private void Start()
    {
        _mainScreenUI = UIManager.Instance.GetUI<MainScreenUI>();
        _artifactUI = UIManager.Instance.GetUI<UIArtifact>();
        _artifactUpgradeUI = UIManager.Instance.GetUI<UIArtifactUpgrade>();
    }

    private void OnArtifactUpgradePanelButtonClicked()
    {
        if (_artifactUpgradeUI != null && _mainScreenUI != null)
        {
            UIManager.Instance.fromUI = FromUI.MainScreen;
            FadeManager.Instance.SwitchGameObjects(_mainScreenUI.gameObject, _artifactUpgradeUI.gameObject);

            CloseUI();
        }
        else
        {
            Debug.Log("_artifactUpgradeUI, _mainScreenUI 둘 중 뭔가 null임");
        }
    }

    private void OnArtifactInventoryButtonClicked()
    {
        if (_artifactUI != null && _mainScreenUI != null)
        {
            UIManager.Instance.fromUI = FromUI.MainScreen;
            FadeManager.Instance.SwitchGameObjects(_mainScreenUI.gameObject, _artifactUI.gameObject);

            CloseUI();
        }
        else
        {
            Debug.Log("_artifactUI, _mainScreenUI 둘 중 뭔가 null임");
        }
    }
}
