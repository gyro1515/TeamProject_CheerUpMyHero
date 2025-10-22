using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public struct DescriptionViewModel
{
    public bool IsPanelActive;
    public ArtifactData ArtifactData;
    public Sprite Icon;
    public Color BorderColor;
    public string GradeOrLevelText;
    public string StatTypeText;
    public string ValueOrCostText;

    public bool IsEquipped;
}

public class UIArtifactInventoryPanel : BasePopUpUI
{
    #region UI 참조 변수
    [Header("닫기 버튼")]
    [SerializeField] private Button _closeButton;

    [Header("인벤토리 버튼들")]
    [SerializeField] private Button _sortButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _unEquipButton;

    [Header("유물 설명창")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private Image descriptionIcon;
    [SerializeField] private Outline descriptionIconOutline;
    [SerializeField] private TextMeshProUGUI descriptionName;
    [SerializeField] private TextMeshProUGUI descriptionGrade;
    [SerializeField] private TextMeshProUGUI descriptionType;
    [SerializeField] private TextMeshProUGUI descriptionValue;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Button descriptionPanelButton;

    [Header("유물 설명창 비활성화 버튼")]
    [SerializeField] private Button _outerButton;
    [SerializeField] private Button _InnerButton;

    [Header("인벤토리 슬롯")]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _slotCreatPosition;

    private List<UIArtifactInventorySlot> _slotList = new List<UIArtifactInventorySlot>();        // 인벤토리 안에 생성된 슬롯들 담아두는 리스트
    
    public ArtifactData _selectedArtifact;
    private int _currentSlotIndex;
    #endregion

    #region 이벤트 시스템
    public event Action OnRequestClose;
    public event Action OnRequestSort;
    public event Action<ArtifactData, int> OnRequestEquip;
    public event Action<ArtifactData> OnRequestUnEquip;
    public event Action<ArtifactData> OnRequestSelectArtifact;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        _closeButton.onClick.AddListener(OnCloseButtonClicked);
        _equipButton.onClick.AddListener(OnEquipButtonClicked);
        _unEquipButton.onClick.AddListener(OnUnEquipButtonClicked);
        _sortButton.onClick.AddListener(OnSortButtonClicked);

        descriptionPanelButton.onClick.AddListener(CloseDescriptionPanel);
        _outerButton.onClick.AddListener(CloseDescriptionPanel);
        _InnerButton.onClick.AddListener(CloseDescriptionPanel);
    }

    public void OpenInventory(int slotIndex, List<InventorySlotViewModel> viewModels)        // 인벤토리 열기 + 지금 선택된 슬롯 어디인지 전달해주는 역할
    {
        _currentSlotIndex = slotIndex;
        _selectedArtifact = null;

        RefreshArtifactInventoryUI(viewModels);
        UpdateDescriptionPanel(new DescriptionViewModel { IsPanelActive = false });
        OpenUI();
    }

    public void RefreshArtifactInventoryUI(List<InventorySlotViewModel> viewModels)         // 인벤토리 UI 새로고침
    {
        for (int i = 0; i < viewModels.Count; i++)
        {
            UIArtifactInventorySlot slot;

            if (i >= _slotList.Count)
            {
                GameObject createdSlot = Instantiate(_slotPrefab, _slotCreatPosition);
                slot = createdSlot.GetComponent<UIArtifactInventorySlot>();
                slot.OnArtifactInventorySlotClicked += SelectArtifact;
                _slotList.Add(slot);
            }
            else
            {
                slot = _slotList[i];
            }

            slot.Init(viewModels[i]);
        }

        for (int i = viewModels.Count; i < _slotList.Count; i++)
        {
            _slotList[i].gameObject.SetActive(false);
        }
    }

    private void SelectArtifact(ArtifactData selectArtifact)
    {
        OnRequestSelectArtifact?.Invoke(selectArtifact);
    }

    public void UpdateDescriptionPanel(DescriptionViewModel vm)       // 유물 눌렀을 때 유물 정보 뜨게 하는 메서드임
    {
        _selectedArtifact = vm.ArtifactData;

        descriptionPanel.SetActive(vm.IsPanelActive);
        if (!vm.IsPanelActive) return;

        descriptionName.text = vm.ArtifactData.name;
        descriptionIcon.sprite = Resources.Load<Sprite>(vm.ArtifactData.iconSpritePath);
        descriptionIconOutline.effectColor = vm.BorderColor;
        description.text = vm.ArtifactData.description;
        descriptionGrade.text = vm.GradeOrLevelText;
        descriptionType.text = vm.StatTypeText;
        descriptionValue.text = vm.ValueOrCostText;

        _equipButton.interactable = (vm.IsPanelActive && !vm.IsEquipped);
        _unEquipButton.interactable = (vm.IsPanelActive && vm.IsEquipped);

        _outerButton.gameObject.SetActive(true);
        _InnerButton.gameObject.SetActive(true);
    }

    private void CloseDescriptionPanel()
    {
        OnRequestSelectArtifact?.Invoke(null);
    }


    #region 버튼 메서드
    private void OnCloseButtonClicked()
    {
        OnRequestClose?.Invoke();
    }

    private void OnEquipButtonClicked()
    {
        if (_selectedArtifact != null)
        {
            OnRequestEquip?.Invoke(_selectedArtifact, _currentSlotIndex);
        }
    }

    private void OnUnEquipButtonClicked()
    {
        if ( _selectedArtifact != null )
        {
            OnRequestUnEquip?.Invoke(_selectedArtifact);
        }
    }

    private void OnSortButtonClicked()
    {
        OnRequestSort?.Invoke();
    }
    #endregion
}
