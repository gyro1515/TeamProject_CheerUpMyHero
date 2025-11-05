using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//public class UIUnitCardSelect : MonoBehaviour, IBackButtonHandler
public class UIUnitCardSelect : BasePopUpUI
{
    [Header("UI")]
    [SerializeField] Button closeButton;
    [SerializeField] Button emptySpaceButton;
    [SerializeField] TMP_Text desckNumText;
    [SerializeField] UICardSynergyExpanationPopup uiCardSynergyExpanationPopup;
    private CardFilter cardFilter;
    private int deckSlotNum;

    [Header("그리드 스크롤")]
    [SerializeField] private GameObject uiUnitCardSlotPrefab;
    [SerializeField] private Transform contentTransform;

    [Header("카드 팝업")]
    [SerializeField] private GameObject detailPopupPanel;
    [SerializeField] private UIUnitCardDetailPanel detailPopupPanelClass;
    [SerializeField] private UIUnitCardInScroll detailCardDisplay;
    [SerializeField] private Button detailCloseButton;

    [Header("등급 카운트 UI")]
    [SerializeField] private TextMeshProUGUI rareSlotText;
    [SerializeField] private TextMeshProUGUI epicSlotText;

    private List<UIUnitCardSlot> _slotList = new List<UIUnitCardSlot> ();
    private int _selectedUnitId = -1;

    private IEventSubscriber<SynergyDataUpdatedEvent> _synergyUpdateSubscriber;
    protected override void Awake()
    {
        base.Awake();
        cardFilter = GetComponent<CardFilter>();
        uiCardSynergyExpanationPopup.Init();
        detailPopupPanelClass.Init();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        closeButton.onClick.AddListener(OnCloseButtonPress);
        emptySpaceButton.onClick.AddListener(OnCloseButtonPress);
        detailCloseButton.enabled = false;
        //detailCloseButton.onClick.AddListener(HideDetailPopup);
        
        cardFilter.OnFilterUpdated += RefreshGrid;
        cardFilter.UpdateUsable();
        cardFilter.FilterAndSort();
        _synergyUpdateSubscriber = EventManager.GetSubscriber<SynergyDataUpdatedEvent>();
        UpdateSlotCountText(); // 켜질 때 텍스트 갱신
        _synergyUpdateSubscriber.Subscribe(OnBuildingRulesChanged);
        //HideDetailPopup();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        closeButton?.onClick.RemoveListener(OnCloseButtonPress);
        emptySpaceButton?.onClick.RemoveListener(OnCloseButtonPress);

        cardFilter.OnFilterUpdated -= RefreshGrid;
        //EventManager.Publish(new RemoveUIStackEvent());
        _synergyUpdateSubscriber.Unsubscribe(OnBuildingRulesChanged);
        HideDetailPopup();
    }
    private void OnBuildingRulesChanged(SynergyDataUpdatedEvent e)
    {
        // 병영 상태가 바뀌면 텍스트와 카드 목록을 모두 새로고침
        UpdateSlotCountText();
        cardFilter.FilterAndSort(); // 카드 목록 갱신 (비활성화 로직 포함)
    }

    private void UpdateSlotCountText()
    {
        // 1. PlayerDataManager에서 현재 최대 슬롯 수 가져오기
        int maxEpicUnits = PlayerDataManager.Instance.EpicUnitSlots;
        int maxRareUnits = PlayerDataManager.Instance.RareUnitSlots;

        int currentDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        // 3. '올바른' 덱 인덱스로 현재 덱 데이터 가져오기
        List <BaseUnitData> currentDeck = PlayerDataManager.Instance.DeckPresets[currentDeckIndex].BaseUnitDatas;

        int currentEpicCount = currentDeck.Count(data => data != null && data.rarity == Rarity.epic);
        int currentRareCount = currentDeck.Count(data => data != null && data.rarity == Rarity.rare);

        // 4. 텍스트 UI 업데이트
        if (rareSlotText != null)
        {
            rareSlotText.text = $"레어\n{currentRareCount}/{maxRareUnits}";
        }
        if (epicSlotText != null)
        {
            epicSlotText.text = $"에픽\n{currentEpicCount}/{maxEpicUnits}";
        }
    }
    private void RefreshGrid(List<int> cardIdList)
    {
        //Debug.Log($"[UIUnitCardSelect] RefreshGrid 호출됨. 카드 개수: {cardIdList.Count}");

        if (uiUnitCardSlotPrefab == null || contentTransform == null)
        {
            Debug.Log("인스펙터 세팅 제대로 안 됐음. 프리펩이나 생성위치 null임");
            return;
        }

        var ownedCardData = PlayerDataManager.Instance.OwnedCardData;

        for (int i = 0; i < cardIdList.Count; i++)
        {
            int cardId = cardIdList[i];
            if (!ownedCardData.ContainsKey(cardId)) return;

            BaseUnitData data = ownedCardData[cardId];

            UIUnitCardSlot slot;
            if (i < _slotList.Count)
            {
                slot = _slotList[i];
            }
            else
            {
                GameObject slotObject = Instantiate(uiUnitCardSlotPrefab, contentTransform);
                slot = slotObject.GetComponent<UIUnitCardSlot>();
                _slotList.Add(slot);
            }

            bool canSelect = !cardFilter.greyCardSet.Contains(cardId);

            slot.Initialize(data, this, canSelect);
            slot.gameObject.SetActive(true);
        }

        for (int i = cardIdList.Count; i < _slotList.Count; i++)
        {
            _slotList[i].gameObject.SetActive(false);
        }
    }

    public void OnCardSlotHold(BaseUnitData data, bool canSelect)
    {
        if (data == null || detailPopupPanel == null) return;

        //detailPopupPanel.SetActive(true);
        detailPopupPanelClass.OpenUI();
        detailCardDisplay.UpdateCardDataByData(data);
    }

    public void OnCardSlotHoldRelease()
    {
        HideDetailPopup();
    }

    //public async Task OnCardSlotShortClick(BaseUnitData data, bool canSelect)
    public void OnCardSlotShortClick(BaseUnitData data, bool canSelect)
    {
        if (data == null) return;
        if (!canSelect) return;

        int selectId = data.idNumber;
        //HideDetailPopup();
        CloseUI();
        //await UIManager.Instance.GetUI<DeckPresetController>().OnUnitSelected(deckSlotNum, selectId);
        UIManager.Instance.GetUI<DeckPresetController>().OnUnitSelected(deckSlotNum, selectId);
    }

    // 몇 번째 덱인지 표시
    public void SetDeckSlotNum(int slotNum)
    {
        deckSlotNum = slotNum;
        desckNumText.text = (deckSlotNum + 1).ToString();
    }

    private void HideDetailPopup()
    {
        if (detailPopupPanel != null)
        {
            //detailPopupPanel.SetActive(false);
            detailPopupPanelClass.CloseUI();
        }
    }

    //void OnSelectButtonPress()
    //{
    //    int selectedIndex = _selectedUnitId;

    //    if (selectedIndex == -1)
    //    {
    //        Debug.Log("카드 선택이 정상적으로 이루어지지 않았습니다");
    //    }
    //    else
    //    {
    //        Debug.Log($"현재 선택된 카드 {selectedIndex}번");
    //        HideDetailPopup();
    //        CloseUI();
    //        UIManager.Instance.GetUI<DeckPresetController>().OnUnitSelected(deckSlotNum, selectedIndex);
    //    }
    //}

    //public void OnDetailPopupClosed()
    //{
    //    _selectedUnitId = -1;
    //    ControllBlocker(false);
    //}

    //void ControllBlocker(bool canSelect)
    //{
    //    if (canSelect)
    //        SeleckBlocker.SetActive(false);
    //    else
    //        SeleckBlocker.SetActive(true);
    //}

    void OnCloseButtonPress()
    {
        CloseUI();
        //this.gameObject.SetActive(false);
    }

    public override void OnBackPressed()
    {
        if (detailPopupPanel != null && detailPopupPanel.gameObject.activeSelf)
        {
            HideDetailPopup();
        }

        base.OnBackPressed();
    }
}
