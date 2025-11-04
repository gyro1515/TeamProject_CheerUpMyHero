using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//public class UIUnitCardSelect : MonoBehaviour, IBackButtonHandler
public class UIUnitCardSelect : BasePopUpUI
{
    [Header("UI")]
    [SerializeField] Button selectButton;
    [SerializeField] GameObject SeleckBlocker;
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
    [SerializeField] private UIUnitCardInScroll detailCardDisplay;
    [SerializeField] private Button detailCloseButton;

    private List<UIUnitCardSlot> _slotList = new List<UIUnitCardSlot> ();
    private int _selectedUnitId = -1;

    protected override void Awake()
    {
        base.Awake();
        cardFilter = GetComponent<CardFilter>();
        uiCardSynergyExpanationPopup.Init();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        selectButton.onClick.AddListener(OnSelectButtonPress);
        closeButton.onClick.AddListener(OnCloseButtonPress);
        emptySpaceButton.onClick.AddListener(OnCloseButtonPress);
        detailCloseButton.onClick.AddListener(HideDetailPopup);
        
        cardFilter.OnFilterUpdated += RefreshGrid;
        cardFilter.UpdateUsable();
        cardFilter.FilterAndSort();

        HideDetailPopup();
        ControllBlocker(false);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        selectButton?.onClick.RemoveListener(OnSelectButtonPress);
        closeButton?.onClick.RemoveListener(OnCloseButtonPress);
        emptySpaceButton?.onClick.RemoveListener(OnCloseButtonPress);

        cardFilter.OnFilterUpdated -= RefreshGrid;
        //EventManager.Publish(new RemoveUIStackEvent());
    }

    private void RefreshGrid(List<int> cardIdList)
    {
        Debug.Log($"[UIUnitCardSelect] RefreshGrid 호출됨. 카드 개수: {cardIdList.Count}");

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

    public void OnCardSlotClicked(BaseUnitData data, bool canSelect)
    {
        if (data == null || detailPopupPanel == null) return;

        detailPopupPanel.SetActive(true);
        detailCardDisplay.UpdateCardDataByData(data);
        ControllBlocker(canSelect);
        _selectedUnitId = data.idNumber;
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
            detailPopupPanel.SetActive(false);
        }

        _selectedUnitId = -1;
        ControllBlocker(false);
    }

    void OnSelectButtonPress()
    {
        int selectedIndex = _selectedUnitId;

        if (selectedIndex == -1)
        {
            Debug.Log("카드 선택이 정상적으로 이루어지지 않았습니다");
        }
        else
        {
            Debug.Log($"현재 선택된 카드 {selectedIndex}번");
            HideDetailPopup();
            CloseUI();
            UIManager.Instance.GetUI<DeckPresetController>().OnUnitSelected(deckSlotNum, selectedIndex);
        }
    }

    public void OnDetailPopupClosed()
    {
        _selectedUnitId = -1;
        ControllBlocker(false);
    }

    void ControllBlocker(bool canSelect)
    {
        if (canSelect)
            SeleckBlocker.SetActive(false);
        else
            SeleckBlocker.SetActive(true);
    }

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
