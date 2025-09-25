using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFilter : MonoBehaviour
{
    //모든 카드
    public List<int> AllCardList { get; private set; } = new (PlayerDataManager.Instance.cardDic.Keys);

    //가능한 카드(못 얻은 카드, 중복 카드 제외)
    public List<int> UsableCardList { get; private set; } = new();

    private List<int> nowDeck = new();


    public void UpdateUsable()
    {
        UsableCardList.Clear();

        //못 얻은 카드 빼기, 가챠 나오면 구현 예정

        //중복 카드 빼기
        


    }
}
