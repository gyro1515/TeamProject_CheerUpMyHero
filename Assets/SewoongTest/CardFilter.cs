using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFilter : MonoBehaviour
{
    //모든 카드
    public List<int> AllCardList { get; private set; }

    //가능한 카드(못 얻은 카드, 중복 카드 제외)
    public List<int> UsableCardList { get; private set; } = new();
    //가능한 카드에 기반한 유닛 리스트
    private List<TempCardData> UsableUnitList = new();

    //수정사항 적용한 최종 출력 카드
    public List<int> ModifiedCardList { get; private set; } = new();

    
    //계산용
    private List<int> nowDeck = new();


    //카드 선택창이 켜질때만 실행
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
        nowDeck.Clear();
        nowDeck.AddRange(PlayerDataManager.Instance.DeckPresets[PlayerDataManager.Instance.ActiveDeckIndex].UnitIds); //현재 덱 불러오기
        for (int i = 0; i < nowDeck.Count; i++)
        {
            if (nowDeck[i] == -1)
                continue;
            UsableCardList.Remove(nowDeck[i]);
        }

        for (int i = 0; i <  UsableCardList.Count; i++)
        {
            UsableUnitList.Add(PlayerDataManager.Instance.cardDic[UsableCardList[i]]);
        }
        
        
        ModifiedCardList.Clear();
        ModifiedCardList.AddRange(UsableCardList);
    }

    public void AsendingOrder()
    {

    }

    public void DesendingOrder()
    {

    }

}
