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

    [Header("자동 장착 버튼")]
    [SerializeField] private Button _passiveButton;
    [SerializeField] private Button _activeButton;
    [SerializeField] private Button _confirmButton;

    [Header("UI간 이동 버튼")]
    [SerializeField] private Button _closeButton;   //지금 비활성화 되어있음
    [SerializeField] private Button _gotoCardDeckButton;

    private CanvasGroup _canvasGroup;

    public ArtifactData SelectedArtifact { get; private set; }
    public int CurrentEquipSlotIndex { get; private set; } = -1;
    #endregion

    #region Artifact 이벤트
    public event Action<int> OnEquipSlotClicked;        // 장착 슬롯 클릭했을 때
    public event Action OnEquippedArtifactChanged;      // 장착 슬롯 변경했을 때

    public event Action<ArtifactData> OnInventorySlotClicked;   // 인벤토리 슬롯 클릭했을 때
    public event Action<ArtifactData> OnArtifactSelected;       // 인벤토리 유물 선택했을 때
    public event Action OnInventoryClosed;                      // 인벤토리 닫혔을 때
    #endregion

    #region 생명주기
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _closeButton.onClick.AddListener(() => SceneLoader.Instance.StartLoadScene(SceneState.BattleScene));
    }

    private void Start()
    {
        _gotoCardDeckButton.onClick.AddListener(OnCardDeckClicked);
    }
    #endregion

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
