using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

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
    private InfiniteScroll infiniteScroll;

    //모든 카드
    public List<int> AllCardList { get; private set; }

    //가능한 카드(못 얻은 카드, 중복 카드 제외)
    public List<int> UsableCardList { get; private set; } = new();

    //가능한 카드에 기반한 유닛 리스트
    private List<TempCardData> UsableUnitList = new();
    //LINQ용
    IEnumerable<TempCardData> query;

    //수정사항 적용한 최종 출력 카드
    public List<int> ModifiedCardList { get; private set; } = new();

    //필터 조건
    private bool isAsending = true;
    private SelectedUnitType selectedUnitType = SelectedUnitType.None;
    private SelectedFilter selectedFilter = SelectedFilter.None;
    private string searchText;

    #region CardLogic
    //카드 선택창이 켜질때만 실행 or 리셋
    public void UpdateUsable()
    {
        if (AllCardList == null)
        {
            AllCardList = new(PlayerDataManager.Instance.cardDic.Keys);

        }

        UsableCardList.Clear();
        UsableCardList.AddRange(AllCardList);

        //못 얻은 카드 빼기, 가챠 나오면 구현 예정

        //중복 카드 빼기
        List<int> nowDeck = new(PlayerDataManager.Instance.DeckPresets[PlayerDataManager.Instance.ActiveDeckIndex].UnitIds); //현재 덱 불러오기
        for (int i = 0; i < nowDeck.Count; i++)
        {
            if (nowDeck[i] == -1)
                continue;
            UsableCardList.Remove(nowDeck[i]);
        }

        UsableUnitList.Clear();
        for (int i = 0; i < UsableCardList.Count; i++)
        {
            UsableUnitList.Add(PlayerDataManager.Instance.cardDic[UsableCardList[i]]);
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
                    query = query.OrderBy(unit => unit.rarity).ThenBy(unit => unit.id);
                    break;
                case SelectedFilter.Cost:
                    query = query.OrderBy(unit => unit.cost).ThenBy(unit => unit.id);
                    break;
                case SelectedFilter.Health:
                    query = query.OrderBy(unit => unit.health).ThenBy(unit => unit.id);
                    break;
                case SelectedFilter.AtkPower:
                    query = query.OrderBy(unit => unit.atkPower).ThenBy(unit => unit.id);
                    break;
                case SelectedFilter.CoolTime:
                    query = query.OrderBy(unit => unit.coolTime).ThenBy(unit => unit.id);
                    break;
                default:
                    query = query.OrderBy(unit => unit.id);
                    break;
            }
        }

        //내림차순
        else
        {
            switch (selectedFilter)
            {
                case SelectedFilter.Rarity:
                    query = query.OrderByDescending(unit => unit.rarity).ThenByDescending(unit => unit.id);
                    break;
                case SelectedFilter.Cost:
                    query = query.OrderByDescending(unit => unit.cost).ThenByDescending(unit => unit.id);
                    break;
                case SelectedFilter.Health:
                    query = query.OrderByDescending(unit => unit.health).ThenByDescending(unit => unit.id);
                    break;
                case SelectedFilter.AtkPower:
                    query = query.OrderByDescending(unit => unit.atkPower).ThenByDescending(unit => unit.id);
                    break;
                case SelectedFilter.CoolTime:
                    query = query.OrderByDescending(unit => unit.coolTime).ThenByDescending(unit => unit.id);
                    break;
                default:
                    query = query.OrderByDescending(unit => unit.id);
                    break;
            }
        }

        List<TempCardData> filteredUnitList = new(query.ToList());

        ModifiedCardList.Clear();
        for (int i = 0; i < filteredUnitList.Count; i++)
        {
            ModifiedCardList.Add(filteredUnitList[i].id);
        }
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

    private void Awake()
    {
        filterList.GetComponent<UIDropDownInUnitCard>().Init(this);
        searchPanel.GetComponent<SearchPanelInUnitCard>().Init(this);
    }

    private void OnEnable()
    {
        filterSelect.onClick.AddListener(onFilterSelect);

        changeOrder.onValueChanged.AddListener(onChangeOrder);

        foreach (UnitSelectBtnInUnitCard button in unitButtonList)
        {
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                SetUnitType(button.unitType);
            });
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

    private void Start()
    {
        infiniteScroll = GetComponent<UIUnitCardSelect>().InfiniteScroll;
    }

    void onFilterSelect()
    {
        filterList.SetActive(true);
    }

    public void SetFilter(SelectedFilter filter)
    {
        selectedFilter = filter;
        FilterAndSort();
        infiniteScroll.ResetCardData(ModifiedCardList);

        switch (selectedFilter)
        {
            case SelectedFilter.None:
                filterText.text = "필터";
                break;
            case SelectedFilter.Rarity:
                filterText.text = "등급";
                break;
            case SelectedFilter.Cost:
                filterText.text = "코스트";
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
        infiniteScroll.ResetCardData(ModifiedCardList);
    }

    void SetUnitType(SelectedUnitType unit)
    {
        if (unit == selectedUnitType)
            selectedUnitType = SelectedUnitType.None;
        else
            selectedUnitType = unit;
        FilterAndSort();
        infiniteScroll.ResetCardData(ModifiedCardList);
    }

    void OnSearch()
    {
        searchPanel.SetActive(true);
    }

    public void SetSeacrh(string text)
    {
        searchText = text;
        FilterAndSort();
        infiniteScroll.ResetCardData(ModifiedCardList);
    }

    #endregion

}
