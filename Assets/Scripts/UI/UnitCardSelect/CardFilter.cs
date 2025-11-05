using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum SelectedUnitType
{
    Tanker,
    Dealer,
    Healer,
    None
}

public enum SelectedFilter
{
    Rarity,
    Cost,
    Health,
    AtkPower,
    CoolTime,
    None
}


public class CardFilter : MonoBehaviour
{
    // 이벤트 시스템
    public event Action<List<int>> OnFilterUpdated;

    //모든 카드
    public List<int> AllCardList { get; private set; } = new();

    //가능한 카드(못 얻은 카드, 중복 카드 제외)
    public List<int> UsableCardList { get; private set; } = new();

    //가능한 카드에 기반한 유닛 리스트
    //private List<TempCardData> UsableUnitList = new();
    private List<BaseUnitData> UsableUnitList = new();
    //LINQ용
    //IEnumerable<TempCardData> query;
    IEnumerable<BaseUnitData> query;

    //수정사항 적용한 최종 출력 카드
    public List<int> ModifiedCardList { get; private set; } = new();

    //선택 불가 카드
    public HashSet<int> greyCardSet { get; private set; } = new();
    
    //필터 조건
    private bool isAsending = true;
    private SelectedUnitType selectedUnitType = SelectedUnitType.None;
    private SelectedFilter selectedFilter = SelectedFilter.None;
    private string searchText;

    #region CardLogic
    //카드 선택창이 켜질때만 실행 or 리셋
    public void UpdateUsable()
    {
        //편성 가능한 최대 레어, 에픽 수
        int maxRareInDeck = PlayerDataManager.Instance.RareUnitSlots;
        int maxEpicInDeck = PlayerDataManager.Instance.EpicUnitSlots;

        //현재 덱의 레어, 에픽 수
        int rareInDeck = 0;
        int epicInDeck = 0;


        AllCardList.Clear();
        AllCardList.AddRange(PlayerDataManager.Instance.OwnedCardData.Keys);

        UsableCardList.Clear();
        UsableCardList.AddRange(AllCardList);

        //중복 카드 빼기
        List<int> nowDeck = new(PlayerDataManager.Instance.DeckPresets[PlayerDataManager.Instance.ActiveDeckIndex].UnitIds); //현재 덱 불러오기
        for (int i = 0; i < nowDeck.Count; i++)
        {
            if (nowDeck[i] == -1)
                continue;
            UsableCardList.Remove(nowDeck[i]);

            //편성된 레어, 에픽 개수 세기
            //if (PlayerDataManager.Instance.cardDic[nowDeck[i]].rarity == Rarity.rare)
            if (PlayerDataManager.Instance.OwnedCardData[nowDeck[i]].rarity == Rarity.rare)
            {
                rareInDeck++;
            }
            //if (PlayerDataManager.Instance.cardDic[nowDeck[i]].rarity == Rarity.epic)
            if (PlayerDataManager.Instance.OwnedCardData[nowDeck[i]].rarity == Rarity.epic)
            {
                epicInDeck++;
            }
        }

        UsableUnitList.Clear();
        for (int i = 0; i < UsableCardList.Count; i++)
        {
            //UsableUnitList.Add(PlayerDataManager.Instance.cardDic[UsableCardList[i]]);
            UsableUnitList.Add(PlayerDataManager.Instance.OwnedCardData[UsableCardList[i]]);
        }

        greyCardSet.Clear();

        //레어, 에픽 카드 선택 제한
        if (rareInDeck >= maxRareInDeck || epicInDeck >= maxEpicInDeck)
        {
            for (int i = 0; i < UsableUnitList.Count; i++)
            {
                if (UsableUnitList[i].rarity == Rarity.rare && rareInDeck >= maxRareInDeck)
                {
                    greyCardSet.Add(UsableUnitList[i].idNumber);
                }
                else if (UsableUnitList[i].rarity == Rarity.epic && epicInDeck >= maxEpicInDeck)
                {
                    greyCardSet.Add(UsableUnitList[i].idNumber);
                }
            }
        }

        ModifiedCardList.Clear();
        ModifiedCardList.AddRange(UsableCardList);
    }

    //카드 필터 및 정렬
    //LINQ 안쓸려고 했는데 압도적으로 편해 보임
    public void FilterAndSort()
    {
        query = UsableUnitList;

        //탱딜힐 선택
        switch (selectedUnitType)
        {
            case SelectedUnitType.Tanker:
                query = query.Where(unit => unit.unitType == UnitType.Tanker);
                break;
            case SelectedUnitType.Dealer:
                query = query.Where(unit => unit.unitType == UnitType.Dealer);
                break;
            case SelectedUnitType.Healer:
                query = query.Where(unit => unit.unitType == UnitType.Healer);
                break;
            default:
                break;
        }

        //검색어 조건
        if (!string.IsNullOrEmpty(searchText))
        {
            query = query.Where(unit => unit.unitName.Contains(searchText));
        }

        //오름차순
        if (isAsending)
        {
            switch (selectedFilter)
            {
                case SelectedFilter.Rarity:
                    query = query.OrderBy(unit => unit.rarity).ThenBy(unit => unit.idNumber);
                    break;
                case SelectedFilter.Cost:
                    query = query.OrderBy(unit => unit.cost).ThenBy(unit => unit.idNumber);
                    break;
                case SelectedFilter.Health:
                    query = query.OrderBy(unit => unit.health).ThenBy(unit => unit.idNumber);
                    break;
                case SelectedFilter.AtkPower:
                    query = query.OrderBy(unit => unit.atkPower).ThenBy(unit => unit.idNumber);
                    break;
                case SelectedFilter.CoolTime:
                    query = query.OrderBy(unit => unit.spawnCooldown).ThenBy(unit => unit.idNumber);
                    break;
                default:
                    query = query.OrderBy(unit => unit.idNumber);
                    break;
            }
        }

        //내림차순
        else
        {
            switch (selectedFilter)
            {
                case SelectedFilter.Rarity:
                    query = query.OrderByDescending(unit => unit.rarity).ThenByDescending(unit => unit.idNumber);
                    break;
                case SelectedFilter.Cost:
                    query = query.OrderByDescending(unit => unit.cost).ThenByDescending(unit => unit.idNumber);
                    break;
                case SelectedFilter.Health:
                    query = query.OrderByDescending(unit => unit.health).ThenByDescending(unit => unit.idNumber);
                    break;
                case SelectedFilter.AtkPower:
                    query = query.OrderByDescending(unit => unit.atkPower).ThenByDescending(unit => unit.idNumber);
                    break;
                case SelectedFilter.CoolTime:
                    query = query.OrderByDescending(unit => unit.spawnCooldown).ThenByDescending(unit => unit.idNumber);
                    break;
                default:
                    query = query.OrderByDescending(unit => unit.idNumber);
                    break;
            }


        }

        List<BaseUnitData> filteredUnitList = new(query.ToList());

        ModifiedCardList.Clear();
        for (int i = 0; i < filteredUnitList.Count; i++)
        {
            ModifiedCardList.Add(filteredUnitList[i].idNumber);
        }

        OnFilterUpdated?.Invoke(ModifiedCardList);
    }
    #endregion 

    #region ButtonLogic
    [SerializeField] Button filterSelect;
    [SerializeField] GameObject filterList;
    [SerializeField] TMP_Text filterText;

    [SerializeField] Toggle changeOrder;
    [SerializeField] GameObject downArrow;

    [SerializeField] List<UnitSelectBtnInUnitCard> unitButtonList;

    [SerializeField] Button search;
    [SerializeField] GameObject searchPanel;
    SearchPanelInUnitCard searchPanelInUnitCard;

    private void Awake()
    {
        filterList.GetComponent<UIDropDownInUnitCard>().Init(this);
        searchPanelInUnitCard = searchPanel.GetComponent<SearchPanelInUnitCard>();
        searchPanelInUnitCard.Init(this);
    }

    private void OnEnable()
    {
        filterSelect.onClick.AddListener(onFilterSelect);

        changeOrder.onValueChanged.AddListener(onChangeOrder);
        selectedUnitType = SelectedUnitType.None;
        FilterAndSort();
        foreach (UnitSelectBtnInUnitCard button in unitButtonList)
        {
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                SetUnitType(button.unitType);
            });
        }
        foreach (UnitSelectBtnInUnitCard button in unitButtonList)
        {
            button.btnImg.color = Color.white;
        }
        search.onClick.AddListener(OnSearch);
    }

    private void OnDisable()
    {
        filterSelect.onClick.RemoveListener(onFilterSelect);
        changeOrder.onValueChanged.RemoveListener(onChangeOrder);

        foreach (UnitSelectBtnInUnitCard button in unitButtonList)
        {
            button.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        search.onClick.RemoveListener(OnSearch);
    }

    void onFilterSelect()
    {
        filterList.SetActive(true);
    }

    public void SetFilter(SelectedFilter filter)
    {
        selectedFilter = filter;
        FilterAndSort();

        switch (selectedFilter)
        {
            case SelectedFilter.None:
                filterText.text = "필터";
                break;
            case SelectedFilter.Rarity:
                filterText.text = "등급";
                break;
            case SelectedFilter.Cost:
                filterText.text = "식량";
                break;
            case SelectedFilter.Health:
                filterText.text = "체력";
                break;
            case SelectedFilter.AtkPower:
                filterText.text = "공격력";
                break;
            case SelectedFilter.CoolTime:
                filterText.text = "쿨타임";
                break;
        }

        filterList.SetActive(false);
    }

    void onChangeOrder(bool toggleOn)
    {
        isAsending = toggleOn;
        downArrow.SetActive(!toggleOn);
        FilterAndSort();
    }

    void SetUnitType(SelectedUnitType unit)
    {
        if (unit == selectedUnitType)
        {
            selectedUnitType = SelectedUnitType.None;
            foreach (UnitSelectBtnInUnitCard button in unitButtonList)
            {
                button.btnImg.color = Color.white;
            }
        }
        else
        {
            selectedUnitType = unit;

            foreach (UnitSelectBtnInUnitCard button in unitButtonList)
            {
                if(button.unitType == unit)
                    button.btnImg.color = Color.white;
                else
                    button.btnImg.color = Color.gray;
            }
        }    
        FilterAndSort();
    }

    void OnSearch()
    {
        //searchPanel.SetActive(true);
        searchPanelInUnitCard.OpenUI();
    }

    public void SetSeacrh(string text)
    {
        searchText = text;
        FilterAndSort();
    }

    public void ResetFilter()
    {
        filterText.text = "필터";
        changeOrder.isOn = true;
        isAsending = true;
        selectedFilter = SelectedFilter.None;
        selectedUnitType = SelectedUnitType.None;
        searchText = string.Empty;
        FilterAndSort();
    }

    #endregion

}
