using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIUnitCardInScroll : MonoBehaviour
{
    [SerializeField] TMP_Text cardNameText;
    [SerializeField] TMP_Text descriptionText;

    //테스트용, 숫자 업데이트 only
    public void TestUpdateData(int cardNum)
    {

        cardNameText.text = $"Card{cardNum}";

        descriptionText.text = $"나는 유닛 {cardNum}이다!";
    }

    //public void UpdateCardData(카드데이터)
    //{

    //}
}
