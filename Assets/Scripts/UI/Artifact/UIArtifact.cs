using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifact : BaseUI
{
    #region UI요소 참조 변수
    [Header("UI 요소 참조")]
    [SerializeField] private UIArtifactEquipPanel _equipPanel;
    [SerializeField] private UIArtifactInventoryPanel _inventoryPanel;
    [SerializeField] private UIArtifactStatPanel _statPanel;

    [Header("UI간 이동 버튼")]
    [SerializeField] private Button _closeButton;   //지금 비활성화 되어있음
    [SerializeField] private Button _gotoCardDeckButton;

    private CanvasGroup _canvasGroup;
    #endregion

    #region 생명주기
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _closeButton.onClick.AddListener(() => SceneLoader.Instance.StartLoadScene(SceneState.BattleScene));
    }

    private void OnEnable()
    {
        ArtifactManager.Instance.OnEquippedArtifactChanged += RefreshAllArtifactDisplay;
    }

    private void Start()
    {
        _gotoCardDeckButton.onClick.AddListener(OnCardDeckClicked);
        RefreshAllArtifactDisplay();
    }

    private void OnDisable()
    {
        ArtifactManager.Instance.OnEquippedArtifactChanged -= RefreshAllArtifactDisplay;
    }
    #endregion

    private void RefreshAllArtifactDisplay()
    {
        _statPanel.RefreshArtifactStatDisplay();
        _equipPanel.RefreshAllArtifactEquipSlotDisplay();
    }

    #region 버튼
    private void OnCloseButtonClicked()
    {
        FadeManager.FadeOutUI(_canvasGroup);
    }

    private void OnCardDeckClicked()
    {
        FadeManager.Instance.SwitchGameObjects(gameObject, UIManager.Instance.GetUI<DeckPresetController>().gameObject);
    }
    #endregion

}