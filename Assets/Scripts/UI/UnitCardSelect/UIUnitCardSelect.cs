using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitCardSelect : MonoBehaviour, IBackButtonHandler
{
    [SerializeField] InfiniteScroll infiniteScroll;
    public InfiniteScroll InfiniteScroll {  get { return infiniteScroll; } }

    [SerializeField] Button selectButton;
    [SerializeField] GameObject SeleckBlocker;
    [SerializeField] Button closeButton;
    [SerializeField] Button emptySpaceButton;

    [SerializeField] TMP_Text desckNumText;
    private CardFilter cardFilter;

    private int deckSlotNum;

    private void Awake()
    {
        cardFilter = GetComponent<CardFilter>();
        infiniteScroll.InitRef(cardFilter);
    }


    private void OnEnable()
    {
        selectButton?.onClick.AddListener(OnSelectButtonPress);
        closeButton?.onClick.AddListener(OnCloseButtonPress);
        emptySpaceButton?.onClick.AddListener(OnCloseButtonPress);
        cardFilter.UpdateUsable();
        infiniteScroll.ResetCardData(cardFilter.ModifiedCardList);
        infiniteScroll.OnCanSelectCard += ControllBlocker;
        cardFilter.FilterAndSort();
        EventManager.Publish(new AddUIStackEvent { ui = this });
    }

    private void OnDisable()
    {
        selectButton?.onClick.RemoveListener(OnSelectButtonPress);
        closeButton?.onClick.RemoveListener(OnCloseButtonPress);
        emptySpaceButton?.onClick.RemoveListener(OnCloseButtonPress);
        infiniteScroll.OnCanSelectCard -= ControllBlocker;
        EventManager.Publish(new RemoveUIStackEvent());
    }

    public void SetDeckSlotNum(int slotNum)
    {
        deckSlotNum = slotNum;
        desckNumText.text = (deckSlotNum + 1).ToString();
    }

    void OnSelectButtonPress()
    {
        int selectedIndex = infiniteScroll.SendSelectedUnit();

        if (selectedIndex == -1)
        {
            Debug.Log("카드 선택이 정상적으로 이루어지지 않았습니다");
        }
        else
        {
            Debug.Log($"현재 선택된 카드 {selectedIndex}번");
            this.gameObject.SetActive(false);
            UIManager.Instance.GetUI<DeckPresetController>().OnUnitSelected(deckSlotNum, selectedIndex);
        }
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
        this.gameObject.SetActive(false);
    }

    public void OnBackPressed()
    {
        Debug.Log($"{gameObject.name} 뒤로가기: ");
        OnCloseButtonPress();
    }
}
