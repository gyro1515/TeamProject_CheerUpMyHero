using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitCardInScroll : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    
    //private Dictionary<int, TempCardData> cardData;
    private Dictionary<int, BaseUnitData> cardData;
    
    [SerializeField] TMP_Text cardNameText;
    [SerializeField] TMP_Text unitType;
    [SerializeField] TMP_Text rarity;
    [SerializeField] TMP_Text costText;
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text atkPowerText;
    [SerializeField] TMP_Text coolTimeText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] Image bgImg;
    [SerializeField] Image unitIconImg;
    [SerializeField] Image synergyIcon;
    [SerializeField] UIUnitSynergeIconArea synergyIconArea;


    [SerializeField] GameObject GreyBlocker;
    

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    private void Start()
    {
        //cardData = PlayerDataManager.Instance.cardDic;
        cardData = PlayerDataManager.Instance.OwnedCardData;
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
        //coolTimeText.text = $"쿨타임\n{cardData[cardNum].coolTime.ToString("N1")}";
        coolTimeText.text = $"쿨타임\n{cardData[cardNum].spawnCooldown.ToString("N1")}";
        descriptionText.text = $"{cardData[cardNum].description}";
        bgImg.sprite = cardData[cardNum].unitBGSprite;
        unitIconImg.sprite = cardData[cardNum].unitIconSprite;

        synergyIconArea.SetUnitSynergeIcon(cardData[cardNum]);

        Grey(!canSelect);

    }
    void Grey(bool isGrey)
    {
        if (isGrey)
            GreyBlocker.SetActive(true);
        else
            GreyBlocker.SetActive(false);
    }

    public void SetAlpha(float alpha)
    {
        _canvasGroup.alpha = alpha;
    }

}
