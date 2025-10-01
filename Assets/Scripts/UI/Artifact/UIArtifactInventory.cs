using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UIArtifactInventory : BaseUI
{
    [Header("인벤토리 타이틀")]
    [SerializeField] private TextMeshProUGUI _title;

    [Header("닫기 버튼")]
    [SerializeField] private Button _closeButton;

    [Header("인벤토리 버튼들")]
    [SerializeField] private Button _sortButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _unEquipButton;

    [Header("유물 설명창")]
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TextMeshProUGUI descriptionName;
    [SerializeField] private TextMeshProUGUI descriptionGrade;
    [SerializeField] private TextMeshProUGUI descriptionType;
    [SerializeField] private TextMeshProUGUI descriptionValue;
    [SerializeField] private TextMeshProUGUI description;

    [Header("유물 설명창 비활성화 버튼")]
    [SerializeField] private Button _outerButton;
    [SerializeField] private Button _InnerButton;

    [Header("인벤토리 슬롯")]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _slotCreatPosition;

    private CanvasGroup _canvasGroup;

    private List<UIArtifactInvInventorySlot> _slotList = new List<UIArtifactInvInventorySlot>();        // 인벤토리 안에 생성된 슬롯들 담아두는 리스트
    private EffectTarget _currentTargetType;
    
    private int _currentSlotIndex;
    private bool isEquipped;

    public ArtifactData _selectedArtifact;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        _closeButton.onClick.AddListener(OnCloseButtonClicked);
        _equipButton.onClick.AddListener(OnEquipButtonClicked);
        _unEquipButton.onClick.AddListener(OnUnEquipButtonClicked);
        _sortButton.onClick.AddListener(OnSortButtonClicked);

        _outerButton.onClick.AddListener(CloseDescriptionPanel);
        _InnerButton.onClick.AddListener(CloseDescriptionPanel);
    }

    public void OpenInventory(int slotIndex)        // 인벤토리 열기 + 지금 선택된 슬롯 어디인지 전달해주는 역할
    {
        _currentSlotIndex = slotIndex;
        _selectedArtifact = null;

        UpdateInventory();
        UpdateDescriptionPanel();
        FadeManager.FadeInUI(_canvasGroup);
    }

    private void SelectArtifact(ArtifactData selectArtifact)
    {
        _selectedArtifact = selectArtifact;
        UpdateDescriptionPanel();
    }

    private void UpdateInventory()
    {
        // 지금 열린 슬롯에 어떤 유물이 있는 지 확인함
        ArtifactData currentSlotEquipped = ArtifactManager.Instance.EquippedArtifacts[_currentSlotIndex];

        List<ArtifactData> ownedList = ArtifactManager.Instance.OwnedArtifacts;

        while (_slotList.Count < ownedList.Count)    // 딱 데이터 개수만큼 인벤토리 슬롯을 준비해둠 슬롯마다 눌렀을 때 이벤트 추가
        {
            GameObject createdSlot = Instantiate(_slotPrefab, _slotCreatPosition);
            UIArtifactInvInventorySlot newSlot = createdSlot.GetComponent<UIArtifactInvInventorySlot>();
            newSlot.OnArtifactInventorySlotClicked += SelectArtifact;
            _slotList.Add(newSlot);
        }

        for (int i = 0; i < _slotList.Count; i++)       // 만든 슬롯에 걸러진 데이터 다 넣어주고 + 슬롯 만듦.
        {
            if (i < ownedList.Count)        // 어차피 슬롯 개수는 딱 맞춰서 생성되니까 필요 없을 것 같긴 한데....
            {
                bool isEquipedThisSlot = (ownedList[i] == currentSlotEquipped);
                _slotList[i].Init(ownedList[i], isEquipedThisSlot);
                _slotList[i].gameObject.SetActive(true);
            }
            else
            {
                _slotList[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateDescriptionPanel()       // 유물 눌렀을 때 유물 정보 뜨게 하는 메서드임
    {
        if (_selectedArtifact == null)
        {
            descriptionPanel.SetActive(false);
            return;
        }
        descriptionPanel.SetActive(true);                   // 선택한 유물 있으면 패널 띄우기
        descriptionName.text = _selectedArtifact.name;
        description.text = _selectedArtifact.description;

        if (_selectedArtifact is PassiveArtifactData passiveAf)     // 유물이 패시브일 때 출력 값
        {
            descriptionGrade.text = $"등급 : {passiveAf.grade}";
            descriptionType.text = $"스탯 타입 : {passiveAf.statType}";
            descriptionValue.text = $"효과 : + {passiveAf.value}%";
        }
        else if (_selectedArtifact is ActiveArtifactData activeAf)      // 유물이 액티브일 때 출력 값
        {
            descriptionGrade.text = $"Lv. {activeAf.levelData[activeAf.curLevel].level}";
            descriptionType.text = $"유형 : {activeAf.type}";
            descriptionValue.text = $"Cost : {activeAf.cost}";
        }

        isEquipped = ArtifactManager.Instance.EquippedArtifacts.Contains(_selectedArtifact);
        _equipButton.gameObject.SetActive(!isEquipped);
        _unEquipButton.gameObject.SetActive(isEquipped);

        _outerButton.gameObject.SetActive(true);
        _InnerButton.gameObject.SetActive(true);
    }

    private void CloseDescriptionPanel()
    {
        _selectedArtifact = null;
        descriptionPanel.SetActive(false);
        _outerButton.gameObject.SetActive(false);
        _InnerButton.gameObject.SetActive(false);
    }

    private void OnCloseButtonClicked()                     // 버튼 눌렀을 때 인벤토리 끄는 메서드
    {
        FadeManager.FadeOutUI(_canvasGroup);
    }

    private void OnEquipButtonClicked()
    {
        if (_selectedArtifact != null)
        {
            ArtifactManager.Instance.EquipArtifact(_selectedArtifact, _currentSlotIndex);
            _selectedArtifact = null;
            FadeManager.FadeOutUI( _canvasGroup);
        }
    }

    private void OnUnEquipButtonClicked()
    {
        if ( _selectedArtifact != null )
        {
            for (int i = 0; i < ArtifactManager.Instance.EquippedArtifacts.Count; i++)
            {
                if (ArtifactManager.Instance.EquippedArtifacts[i] == _selectedArtifact)
                {
                    ArtifactManager.Instance.UnEquipArtifact(i);
                    break;
                }
            }
            _selectedArtifact = null;
            UpdateDescriptionPanel();
        }
    }

    private void OnSortButtonClicked()
    {
        ArtifactManager.Instance.OwnedArtifacts.Sort((a, b) =>
        {
            bool isAActive = a is ActiveArtifactData;
            bool isBActive = b is ActiveArtifactData;

            if (isAActive && !isBActive)    // a는 액티브, b가 패시브면 a를 앞으로 둠.
            {
                return -1;
            }
            if (!isAActive && isBActive)    // b는 액티브, a는 패시브면 b를 앞으로 둠.
            {
                return 1;
            }

            if (!isAActive && !isBActive)    // 둘 다 패시브일 경우 
            {
                PassiveArtifactData passiveA = a as PassiveArtifactData;
                PassiveArtifactData passiveB = b as PassiveArtifactData;
                return passiveB.grade.CompareTo(passiveA.grade);    // 등급으로 비교함
            }

            return a.name.CompareTo(b.name);
        });

        UpdateInventory();
        _selectedArtifact = null;
        UpdateDescriptionPanel();
    }
}
