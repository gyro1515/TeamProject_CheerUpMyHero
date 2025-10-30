using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDeckSynergyForBattleScene : MonoBehaviour
{
    [SerializeField] RectTransform slotContainer;
    [SerializeField] GameObject synergyIconPrefab;
    [SerializeField] RectTransform layoutGropRT;
    [SerializeField] HorizontalLayoutGroup synergyLayoutGroup;

    UnitSynergyType[] _allSynergyTypes = (UnitSynergyType[])Enum.GetValues(typeof(UnitSynergyType));
    List<GameObject> synergyIconGOList = new List<GameObject>();
    private Dictionary<SynergyIcon, (UnitSynergyType, SynergyGrade)> synergyIconToTypeAndGrade = new()
    {
        // 프리즘 등급 (Index = 2)
        { SynergyIcon.Kingdom_2, (UnitSynergyType.Kingdom, SynergyGrade.Prism) },
        { SynergyIcon.Empire_2,  (UnitSynergyType.Empire, SynergyGrade.Prism) },
        { SynergyIcon.Mage_2,    (UnitSynergyType.Mage, SynergyGrade.Prism) },
        { SynergyIcon.Cleric_2,  (UnitSynergyType.Cleric, SynergyGrade.Prism) },
        { SynergyIcon.Hero_2,    (UnitSynergyType.Hero, SynergyGrade.Prism) },
        { SynergyIcon.Frost_2,   (UnitSynergyType.Frost, SynergyGrade.Prism) },
        { SynergyIcon.Burn_2,    (UnitSynergyType.Burn, SynergyGrade.Prism) },
        { SynergyIcon.Poison_2,  (UnitSynergyType.Poison, SynergyGrade.Prism) },
        
        // 골드 등급 (Index = 1)
        { SynergyIcon.Kingdom_1,   (UnitSynergyType.Kingdom, SynergyGrade.Gold) },
        { SynergyIcon.Empire_1,    (UnitSynergyType.Empire, SynergyGrade.Gold) },
        { SynergyIcon.Mage_1,      (UnitSynergyType.Mage, SynergyGrade.Gold) },
        { SynergyIcon.Cleric_1,    (UnitSynergyType.Cleric, SynergyGrade.Gold) },
        { SynergyIcon.Berserker_1, (UnitSynergyType.Berserker, SynergyGrade.Gold) },
        { SynergyIcon.Archer_1,    (UnitSynergyType.Archer, SynergyGrade.Gold) },
        { SynergyIcon.Hero_1,      (UnitSynergyType.Hero, SynergyGrade.Gold) },
        { SynergyIcon.Frost_1,     (UnitSynergyType.Frost, SynergyGrade.Gold) },
        { SynergyIcon.Burn_1,      (UnitSynergyType.Burn, SynergyGrade.Gold) },
        { SynergyIcon.Poison_1,    (UnitSynergyType.Poison, SynergyGrade.Gold) },
        
        // 브론즈 등급 (Index = 0)
        { SynergyIcon.Kingdom_0,   (UnitSynergyType.Kingdom, SynergyGrade.Bronze) },
        { SynergyIcon.Empire_0,    (UnitSynergyType.Empire, SynergyGrade.Bronze) },
        { SynergyIcon.Mage_0,      (UnitSynergyType.Mage, SynergyGrade.Bronze) },
        { SynergyIcon.Cleric_0,    (UnitSynergyType.Cleric, SynergyGrade.Bronze) },
        { SynergyIcon.Berserker_0, (UnitSynergyType.Berserker, SynergyGrade.Bronze) },
        { SynergyIcon.Archer_0,    (UnitSynergyType.Archer, SynergyGrade.Bronze) },
        { SynergyIcon.Hero_0,      (UnitSynergyType.Hero, SynergyGrade.Bronze) },
        { SynergyIcon.Frost_0,     (UnitSynergyType.Frost, SynergyGrade.Bronze) },
        { SynergyIcon.Burn_0,      (UnitSynergyType.Burn, SynergyGrade.Bronze) },
        { SynergyIcon.Poison_0,    (UnitSynergyType.Poison, SynergyGrade.Bronze) },
    };
    private Dictionary<(UnitSynergyType, SynergyGrade), SynergyIcon> typeAndGradeTosynergyIcon = new()
    {
        // 프리즘 등급 (Index = 2)
        { (UnitSynergyType.Kingdom, SynergyGrade.Prism), SynergyIcon.Kingdom_2 },
        { (UnitSynergyType.Empire, SynergyGrade.Prism), SynergyIcon.Empire_2 },
        { (UnitSynergyType.Mage, SynergyGrade.Prism), SynergyIcon.Mage_2 },
        { (UnitSynergyType.Cleric, SynergyGrade.Prism), SynergyIcon.Cleric_2 },
        { (UnitSynergyType.Hero, SynergyGrade.Prism), SynergyIcon.Hero_2 },
        { (UnitSynergyType.Frost, SynergyGrade.Prism), SynergyIcon.Frost_2 },
        { (UnitSynergyType.Burn, SynergyGrade.Prism), SynergyIcon.Burn_2 },
        { (UnitSynergyType.Poison, SynergyGrade.Prism), SynergyIcon.Poison_2 },

        // 골드 등급 (Index = 1)
        { (UnitSynergyType.Kingdom, SynergyGrade.Gold), SynergyIcon.Kingdom_1 },
        { (UnitSynergyType.Empire, SynergyGrade.Gold), SynergyIcon.Empire_1 },
        { (UnitSynergyType.Mage, SynergyGrade.Gold), SynergyIcon.Mage_1 },
        { (UnitSynergyType.Cleric, SynergyGrade.Gold), SynergyIcon.Cleric_1 },
        { (UnitSynergyType.Berserker, SynergyGrade.Gold), SynergyIcon.Berserker_1 },
        { (UnitSynergyType.Archer, SynergyGrade.Gold), SynergyIcon.Archer_1 },
        { (UnitSynergyType.Hero, SynergyGrade.Gold), SynergyIcon.Hero_1 },
        { (UnitSynergyType.Frost, SynergyGrade.Gold), SynergyIcon.Frost_1 },
        { (UnitSynergyType.Burn, SynergyGrade.Gold), SynergyIcon.Burn_1 },
        { (UnitSynergyType.Poison, SynergyGrade.Gold), SynergyIcon.Poison_1 },

        // 브론즈 등급 (Index = 0)
        { (UnitSynergyType.Kingdom, SynergyGrade.Bronze), SynergyIcon.Kingdom_0 },
        { (UnitSynergyType.Empire, SynergyGrade.Bronze), SynergyIcon.Empire_0 },
        { (UnitSynergyType.Mage, SynergyGrade.Bronze), SynergyIcon.Mage_0 },
        { (UnitSynergyType.Cleric, SynergyGrade.Bronze), SynergyIcon.Cleric_0 },
        { (UnitSynergyType.Berserker, SynergyGrade.Bronze), SynergyIcon.Berserker_0 },
        { (UnitSynergyType.Archer, SynergyGrade.Bronze), SynergyIcon.Archer_0 },
        { (UnitSynergyType.Hero, SynergyGrade.Bronze), SynergyIcon.Hero_0 },
        { (UnitSynergyType.Frost, SynergyGrade.Bronze), SynergyIcon.Frost_0 },
        { (UnitSynergyType.Burn, SynergyGrade.Bronze), SynergyIcon.Burn_0 },
        { (UnitSynergyType.Poison, SynergyGrade.Bronze), SynergyIcon.Poison_0 },

    };

    IEventSubscriber<FinishTutorialDeckSettingEvent> finishTutorialDeckSettingEventSub;
    private void Awake()
    {
        finishTutorialDeckSettingEventSub = EventManager.GetSubscriber<FinishTutorialDeckSettingEvent>();
        finishTutorialDeckSettingEventSub.Subscribe(UpdateSynergyUI);
        // 테스트
        /*Button btn = gameObject.AddComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            int cnt = 0;
            for (int i = 0; i < slotContainer.childCount; i++)
            {
                if (slotContainer.GetChild(i).gameObject.activeSelf)
                    cnt++;
            }
            UpdateSynergyUISize(cnt);
        });*/
        foreach (SynergyIcon type in (SynergyIcon[])Enum.GetValues(typeof(SynergyIcon)))
        {
            GameObject iconGO = Instantiate(synergyIconPrefab, slotContainer);
            Image iconImage = iconGO.GetComponent<Image>();
            iconImage.sprite = DataManager.Instance.SynergyIconSprites[synergyIconToTypeAndGrade[type]];
            synergyIconGOList.Add(iconGO);
            iconGO.SetActive(false);
        }
        UpdateSynergyUI();
    }
    private void OnDisable()
    {
        finishTutorialDeckSettingEventSub.Unsubscribe(UpdateSynergyUI);
    }
    void UpdateSynergyUI(FinishTutorialDeckSettingEvent e)
    {
        UpdateSynergyUI();
    }
    void UpdateSynergyUI()
    {
        int activeSynergyCount = 0;
        foreach (var typeAndGrade in PlayerDataManager.AppliedDeckUnitSynergies)
        {
            int idx = (int)typeAndGradeTosynergyIcon[(typeAndGrade.Key, typeAndGrade.Value)];
            synergyIconGOList[idx].SetActive(true);
            activeSynergyCount++;
        }
        UpdateSynergyUISize(activeSynergyCount);
    }
    void UpdateSynergyUISize(int activeSynergyCount)
    {
        if(activeSynergyCount == 0)
        {
            slotContainer.gameObject.SetActive(false);
            return;
        }
        else
        {
            slotContainer.gameObject.SetActive(true);
        }
        float rectSizeX = layoutGropRT.rect.size.x;
        float rectSizeY = layoutGropRT.rect.size.y;
        rectSizeX -= synergyLayoutGroup.padding.left + synergyLayoutGroup.padding.right;
        rectSizeX -= synergyLayoutGroup.spacing * (activeSynergyCount - 1);
        rectSizeY -= synergyLayoutGroup.padding.top + synergyLayoutGroup.padding.bottom;
        float iconSize = rectSizeX / activeSynergyCount;
        iconSize = Mathf.Min(iconSize, rectSizeY); // 세로 크기보다 커지면 안됨
        for (int i = 0; i < synergyIconGOList.Count; i++)
        {
            synergyIconGOList[i].GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);
        }
        float sizeY = synergyLayoutGroup.padding.top + synergyLayoutGroup.padding.bottom + iconSize;
        slotContainer.sizeDelta = new Vector2(slotContainer.sizeDelta.x, sizeY);
    }
}
