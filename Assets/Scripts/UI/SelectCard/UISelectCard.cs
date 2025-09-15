using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectCard : BaseUI
{
    [SerializeField] UIScrollCard uIScrollCard;
    [SerializeField] UICardDeckHolder uICardDeckHolder;
    [SerializeField] Button adviserButton;
    [SerializeField] GameObject popUpUI;

    //이 UI가 알아야 하는것
    //플레이어가 보유하고 있는 카드들

    //편성된 카드 리스트

    //임시 플레이어 보유 카드 리스트
    //나중에 int -> CardData 등으로 변경
    public List<int> CardList { get; private set; } = new();

    //보유 카드의 선택 여부
    private List<bool> cardBoolList  = new();

    //임시 편성 카드 리스트
    //나중에 int -> CardData 등으로 변경
    public List<int> DeckList { get; private set; } = new();

    //보유 리스트와 편성 리스트를 연결하는 리스트. 편성 n번이 보유 m번째 카드이다. 
    private List<int> matchList = new();

    private void Awake()
    {
        adviserButton.onClick.AddListener(ActivePopUP);
    }

    private void OnEnable()
    {
        CardList.Clear();
        
        //어디선가로부터 카드 정보 불러와야 함
        //임시로 생성
        CardList.Add(0);
        CardList.Add(1);
        CardList.Add(2);
        CardList.Add(3);
        CardList.Add(4);

        for (int i = 0; i < CardList.Count; i++)
        {
            cardBoolList.Add(false);
        }

        uIScrollCard.Init(this);
        uICardDeckHolder.Init(this);
    }

    public void SetDeck(int index, bool isSelected)
    {
        Debug.Log($"{index}번 카드 선택여부: {isSelected}");

        cardBoolList[index] = isSelected;

        DeckList.Clear();
        matchList.Clear();
        for (int i = 0; i < cardBoolList.Count; i++)
        {
            if (cardBoolList[i] == false)
                continue;

            DeckList.Add(CardList[i]);
            matchList.Add(i);
            Debug.Log($"{matchList.Count}번째 슬롯에 {i+1}번째 카드 매칭");
        }

        uICardDeckHolder.DePloyDeck(DeckList);
    }

    //편성된 슬롯 클릭 -> 카드 리스트의 해당하는 카드의 버튼 누름
    public void CancelDeckBySlot(int index)
    {
        int cardNum = matchList[index];

        uIScrollCard.SpawnedSlots[cardNum].ButtonFormExternal();

    }


    private void ActivePopUP()
    {
        popUpUI.SetActive(true);
    }

    private void OnDestroy()
    {
        adviserButton.onClick.RemoveAllListeners();
    }

}
