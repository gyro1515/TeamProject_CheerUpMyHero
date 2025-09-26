using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitCardSelect : BaseUI
{
    [SerializeField] InfiniteScroll infiniteScroll;
    public InfiniteScroll InfiniteScroll {  get { return infiniteScroll; } }

    [SerializeField] Button selectButton;
    [SerializeField] GameObject SeleckBlocker;

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
        selectButton?.onClick.AddListener(onSelectButtonPress);
        cardFilter.UpdateUsable();
        infiniteScroll.ResetCardData(cardFilter.ModifiedCardList);
        infiniteScroll.OnCanSelectCard += ControllBlocker;
        cardFilter.FilterAndSort();
    }

    private void OnDisable()
    {
        selectButton?.onClick.RemoveListener(onSelectButtonPress);
        infiniteScroll.OnCanSelectCard -= ControllBlocker;
    }

    public void SetDeckSlotNum(int slotNum)
    {
        deckSlotNum = slotNum;
        desckNumText.text = (deckSlotNum + 1).ToString();
    }

    void onSelectButtonPress()
    {
        int selectedIndex = infiniteScroll.SendSelectedUnit();

        if (selectedIndex == -1)
        {
            Debug.Log("카드 선택이 정상적으로 이루어지지 않았습니다");
        }
        else
        {
            Debug.Log($"현재 선택된 카드 {selectedIndex}번");
            FadeManager.Instance.SwitchGameObjects(UIManager.Instance.GetUI<UIUnitCardSelect>().gameObject, UIManager.Instance.GetUI<DeckPresetController>().gameObject);
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

}
