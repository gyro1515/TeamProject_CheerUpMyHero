using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UIPassiveArtifactInventory : BaseUI
{
    [Header("인벤토리 타이틀")]
    [SerializeField] private TextMeshProUGUI _title;

    [Header("닫기 버튼")]
    [SerializeField] private Button _closeButton;

    [Header("인벤토리 버튼들")]
    [SerializeField] private Button _sortButton;
    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _noEquipButton;

    [Header("인벤토리 슬롯")]
    [SerializeField] private GameObject _slotPrefab;
    [SerializeField] private Transform _slotCreatPosition;

    private CanvasGroup _canvasGroup;

    private List<UIArtifactInvInventorySlot> _slotList = new List<UIArtifactInvInventorySlot>();        // 인벤토리 안에 생성된 슬롯들 담아두는 리스트
    private EffectTarget _currentTargetType;
    private int _currentSlotIndex;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        _closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    public void OpenInventory(EffectTarget target, int slotIndex)
    {
        _currentTargetType = target;
        _currentSlotIndex = slotIndex;

        UpdateUI();

        FadeManager.Instance.FadeInUI(_canvasGroup);
    }

    private void UpdateUI()
    {
        // 지금 열린 슬롯에 어떤 유물이 있는 지 확인함
        PassiveArtifactData currentSlotEquipped = PlayerDataManager.Instance.EquippedArtifacts[_currentTargetType][_currentSlotIndex];

        // 지금 가진 유물 중에 패시브 아이템임 && 지금 선택된 타겟 타입임 조건을 만족하는 유물만 골라냄
        List<ArtifactData> ownedList = PlayerDataManager.Instance.OwnedArtifacts;
        List<PassiveArtifactData> filteredData = ownedList.OfType<PassiveArtifactData>()
                                                          .Where(artifact => artifact.effectTarget == _currentTargetType)
                                                          .ToList();

        while (_slotList.Count < filteredData.Count)    // 딱 데이터 개수만큼 인벤토리 슬롯을 준비해둠 슬롯마다 눌렀을 때 이벤트 추가
        {
            GameObject createdSlot = Instantiate(_slotPrefab, _slotCreatPosition);
            UIArtifactInvInventorySlot newSlot = createdSlot.GetComponent<UIArtifactInvInventorySlot>();
            newSlot.OnArtifactInventorySlotClicked += SelectArtifact;
            _slotList.Add(newSlot);
        }

        for (int i = 0; i < _slotList.Count; i++)       // 만든 슬롯에 걸러진 데이터 다 넣어주고 + 슬롯 만듦.
        {
            if (i < filteredData.Count)        // 어차피 슬롯 개수는 딱 맞춰서 생성되니까 필요 없을 것 같긴 한데....
            {
                bool isEquipedThisSlot = (filteredData[i] == currentSlotEquipped);
                _slotList[i].Init(filteredData[i], isEquipedThisSlot);
                _slotList[i].gameObject.SetActive(true);
            }
            else
            {
                _slotList[i].gameObject.SetActive(false);
            }
        }
    }

    private void SelectArtifact(PassiveArtifactData selectArtifact)
    {
        PlayerDataManager.Instance.EquipArtifact(selectArtifact, _currentSlotIndex);
        FadeManager.Instance.FadeOutUI(_canvasGroup);
    }

    private void OnCloseButtonClicked()                     // 버튼 눌렀을 때 인벤토리 끄는 메서드
    {
        FadeManager.Instance.FadeOutUI(_canvasGroup);
    }
}
