using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIUnitCardInScroll : MonoBehaviour
{
    private Dictionary<int, TempCardData> cardData;
    
    [SerializeField] TMP_Text cardNameText;
    [SerializeField] TMP_Text unitType;
    [SerializeField] TMP_Text rarity;
    [SerializeField] TMP_Text costText;
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text atkPowerText;
    [SerializeField] TMP_Text coolTimeText;
    [SerializeField] TMP_Text descriptionText;

    [SerializeField] GameObject GreyBlocker;

    private void Start()
    {
        cardData = PlayerDataManager.Instance.cardDic;
    }

    //카드 데이터 갱신
    public void UpdateCardData(int cardNum, bool canSelect)
    {
        cardNameText.text = $"{cardData[cardNum].unitName}";
        unitType.text = $"{cardData[cardNum].unitType.ToString()}";
        rarity.text = $"{cardData[cardNum].rarity.ToString()}";
        costText.text = $"코스트\n{cardData[cardNum].cost.ToString("F0")}";
        healthText.text = $"체력\n{cardData[cardNum].health.ToString("F0")}";
        atkPowerText.text = $"공격력\n{cardData[cardNum].atkPower.ToString("F0")}";
        coolTimeText.text = $"쿨타임\n{cardData[cardNum].coolTime.ToString("F0")}";
        descriptionText.text = $"{cardData[cardNum].description}";
        Grey(!canSelect);
    }

    void Grey(bool isGrey)
    {
        if (isGrey)
            GreyBlocker.SetActive(true);
        else
            GreyBlocker.SetActive(false);

    }

}
