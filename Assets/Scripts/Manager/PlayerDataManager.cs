using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class PlayerDataManager : SingletonMono<PlayerDataManager>
{


    //편성된 덱 정보
    public List<int> DeckList { get; private set; } = new();

    public void SetDeckList(List<int> deckList)
    {
        DeckList.Clear();
        DeckList = deckList;
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < DeckList.Count; i++)
        {
            sb.Append(DeckList[i].ToString());
            sb.Append(", ");
        }
        sb.Length -= 2;
        Debug.Log($"[PlayerDataManager] 덱 세팅 완료: {sb.ToString()}");
    }

    //자원


}
