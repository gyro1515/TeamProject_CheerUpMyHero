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
    [SerializeField] Image unitTypeIcon;
    [SerializeField] UIUnitSynergeIconArea synergyIconArea;
    [SerializeField] UIRarityIconArea rarityIconArea;


    [SerializeField] GameObject GreyBlocker;

    Dictionary<UnitType, Sprite> unitTypeIconSprites;

    bool isinit = false;
    private void Awake()
    {
        CheckInit();
    }
    private void Start()
    {
        //cardData = PlayerDataManager.Instance.cardDic;
        cardData = PlayerDataManager.Instance.OwnedCardData;
    }
    public void UpdateCardDataByData(BaseUnitData baseUnitData) // 도감에서 사용하기
    {
        CheckInit();
        cardNameText.text = $"{baseUnitData.unitName}";
        unitType.text = $"{baseUnitData.unitType.ToString()}";
        rarityIconArea.SetIconCnt((int)baseUnitData.rarity);
        costText.text = $"식량\n{baseUnitData.cost.ToString("F0")}";
        healthText.text = $"체력\n{baseUnitData.health.ToString("F0")}";
        atkPowerText.text = $"공격력\n{baseUnitData.atkPower.ToString("F0")}";
        coolTimeText.text = $"쿨타임\n{baseUnitData.spawnCooldown.ToString("N1")}";
        descriptionText.text = $"{baseUnitData.description}";
        bgImg.sprite = baseUnitData.unitBGSprite;
        unitIconImg.sprite = baseUnitData.unitIconSprite;
        synergyIconArea.SetUnitSynergeIcon(baseUnitData);
        unitTypeIcon.sprite = unitTypeIconSprites[baseUnitData.unitType];
    }
    //카드 데이터 갱신
    public void UpdateCardData(int cardNum, bool canSelect)
    {
        CheckInit();
        //BaseUnitData data = cardData[cardNum];
        cardNameText.text = $"{cardData[cardNum].unitName}";
        unitType.text = $"{cardData[cardNum].unitType.ToString()}";
        //rarity.text = $"{cardData[cardNum].rarity.ToString()}";
        rarityIconArea.SetIconCnt((int)cardData[cardNum].rarity);
        costText.text = $"식량\n{cardData[cardNum].cost.ToString("F0")}";
        healthText.text = $"체력\n{cardData[cardNum].health.ToString("F0")}";
        atkPowerText.text = $"공격력\n{cardData[cardNum].atkPower.ToString("F0")}";
        //coolTimeText.text = $"쿨타임\n{cardData[cardNum].coolTime.ToString("N1")}";
        coolTimeText.text = $"쿨타임\n{cardData[cardNum].spawnCooldown.ToString("N1")}";
        descriptionText.text = $"{cardData[cardNum].description}";
        bgImg.sprite = cardData[cardNum].unitBGSprite;
        unitIconImg.sprite = cardData[cardNum].unitIconSprite;

        synergyIconArea.SetUnitSynergeIcon(cardData[cardNum]);
        unitTypeIcon.sprite = unitTypeIconSprites[cardData[cardNum].unitType];
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

    void CheckInit()
    {
        if (isinit && cardData != null) return;
        isinit = true;
        cardData = PlayerDataManager.Instance.OwnedCardData;
        _canvasGroup = GetComponent<CanvasGroup>();
        unitTypeIconSprites = DataManager.Instance.UnitTypeIconSprites;
    }
}
