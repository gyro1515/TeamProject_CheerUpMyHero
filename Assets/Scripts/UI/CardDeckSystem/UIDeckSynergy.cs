using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIDeckSynergy : MonoBehaviour
{
    enum SynergyIcon
    {
        // 프리즘
        Kingdom_2,
        Empire_2,
        Mage_2,
        Cleric_2,
        Hero_2,
        Frost_2,
        Burn_2,
        Poison_2,
        // 골드
        Kingdom_1,
        Empire_1,
        Mage_1,
        Cleric_1,
        Berserker_1,
        Archer_1,
        Hero_1,
        Frost_1,
        Burn_1,
        Poison_1,
        // 브론즈
        Kingdom_0,
        Empire_0,
        Mage_0,
        Cleric_0,
        Berserker_0,
        Archer_0,
        Hero_0,
        Frost_0,
        Burn_0,
        Poison_0,
    }
    [Header("시너지 UI 설정")]
    [SerializeField] GameObject synergyIconPrefab;
    [SerializeField] Transform synergyIconParent;
    [SerializeField] HorizontalLayoutGroup synergyLayoutGroup;
    [SerializeField] List<GameObject> synergyIconGOList = new List<GameObject>();
    // 시너지별 카운트 저장
    Dictionary<UnitSynergyType, int> synergyCounts = new Dictionary<UnitSynergyType, int>();
    //List<int> synergyCounts;
    // 아이콘 저장 
    Dictionary<SynergyIcon, Sprite> synergyIcon = new Dictionary<SynergyIcon, Sprite>();
    // 아이콘 GO 저장용 딕셔너리
    List<GameObject> synergyIconGOListForAuto = new List<GameObject>();

    // enum 배열
    UnitSynergyType[] _allSynergyTypes = (UnitSynergyType[])Enum.GetValues(typeof(UnitSynergyType));
    SynergyIcon[] _allSynergies = (SynergyIcon[])Enum.GetValues(typeof(SynergyIcon));
    // 시너지 체크용 배열
    List<List<int>> coutsForSynergyGrade = new List<List<int>>() 
    {
        new List<int>(), // None
        new List<int>{2, 4, 6 }, // Kingdom
        new List<int>{2, 4, 6 }, // Empire
        new List<int>{2, 4, 6 }, // Mage
        new List<int>{3, 4, 6 }, // Cleric
        new List<int>{3, 5 },    // Berserker
        new List<int>{3, 5 },    // Archer
        new List<int>{2, 4, 6 }, // Hero
        new List<int>{2, 3, 5 }, // Frost
        new List<int>{1, 2, 3 }, // Burn
        new List<int>{2, 3, 5 }, // Poison
    };
    // 힙 할당 줄이기위한 맵
    private Dictionary<(UnitSynergyType, int), SynergyIcon> _iconMap = new()
    {
        // 🟣 프리즘 등급 (Index = 2)
        { (UnitSynergyType.Kingdom, 2), SynergyIcon.Kingdom_2 },
        { (UnitSynergyType.Empire, 2), SynergyIcon.Empire_2 },
        { (UnitSynergyType.Mage, 2), SynergyIcon.Mage_2 },
        { (UnitSynergyType.Cleric, 2), SynergyIcon.Cleric_2 },
        { (UnitSynergyType.Hero, 2), SynergyIcon.Hero_2 },
        { (UnitSynergyType.Frost, 2), SynergyIcon.Frost_2 },
        { (UnitSynergyType.Burn, 2), SynergyIcon.Burn_2 },
        { (UnitSynergyType.Poison, 2), SynergyIcon.Poison_2 },

        // 🟡 골드 등급 (Index = 1)
        { (UnitSynergyType.Kingdom, 1), SynergyIcon.Kingdom_1 },
        { (UnitSynergyType.Empire, 1), SynergyIcon.Empire_1 },
        { (UnitSynergyType.Mage, 1), SynergyIcon.Mage_1 },
        { (UnitSynergyType.Cleric, 1), SynergyIcon.Cleric_1 },
        { (UnitSynergyType.Berserker, 1), SynergyIcon.Berserker_1 },
        { (UnitSynergyType.Archer, 1), SynergyIcon.Archer_1 },
        { (UnitSynergyType.Hero, 1), SynergyIcon.Hero_1 },
        { (UnitSynergyType.Frost, 1), SynergyIcon.Frost_1 },
        { (UnitSynergyType.Burn, 1), SynergyIcon.Burn_1 },
        { (UnitSynergyType.Poison, 1), SynergyIcon.Poison_1 },

        // 🟤 브론즈 등급 (Index = 0)
        { (UnitSynergyType.Kingdom, 0), SynergyIcon.Kingdom_0 },
        { (UnitSynergyType.Empire, 0), SynergyIcon.Empire_0 },
        { (UnitSynergyType.Mage, 0), SynergyIcon.Mage_0 },
        { (UnitSynergyType.Cleric, 0), SynergyIcon.Cleric_0 },
        { (UnitSynergyType.Berserker, 0), SynergyIcon.Berserker_0 },
        { (UnitSynergyType.Archer, 0), SynergyIcon.Archer_0 },
        { (UnitSynergyType.Hero, 0), SynergyIcon.Hero_0 },
        { (UnitSynergyType.Frost, 0), SynergyIcon.Frost_0 },
        { (UnitSynergyType.Burn, 0), SynergyIcon.Burn_0 },
        { (UnitSynergyType.Poison, 0), SynergyIcon.Poison_0 },

    };


    // 너무 무거워질거 같아서 그냥 노가다로 세팅해야 하지 않을까 싶습니다...
    // 일단 자동화와 노가다 방식 둘 다 준비해두었습니다.
    public void Init()
    {
        //synergyCounts = new List<int>(new int[_allSynergyTypes.Length]);
        // 미리 시너지에 사용할 스프라이트 로딩
        foreach (UnitSynergyType type in _allSynergyTypes)
        {
            if (type == UnitSynergyType.None) continue;
            Sprite[] sprites = Resources.LoadAll<Sprite>($"Synergy/{type.ToString()}");
            for (int i = 0; i < sprites.Length; i++)
            {
                SynergyIcon iconKey = _iconMap[(type, i)];
                //SynergyIcon iconKey = (SynergyIcon)Enum.Parse(typeof(SynergyIcon), $"{type.ToString()}_{i}");
                synergyIcon[iconKey] = sprites[i];
            }
        }
        // 순서대로 시너지 아이콘 생성, 스트라이프 넣어주기
        foreach (SynergyIcon type in _allSynergies)
        {
            GameObject iconGO = Instantiate(synergyIconPrefab, synergyIconParent);
            Image iconImage = iconGO.GetComponent<Image>();
            iconImage.sprite = synergyIcon[type];
            synergyIconGOListForAuto.Add(iconGO);
            iconGO.SetActive(false);
        }
    }
    public void CheckDeckUnitSynergy(List<BaseUnitData> currentDeckUnitDatas)
    {
        // 시너시 개수 초기화
        foreach (UnitSynergyType type in _allSynergyTypes)
        {
            if (type == UnitSynergyType.None) continue;
            synergyCounts[type] = 0;
        }

        for (int i = 0; i < currentDeckUnitDatas.Count; i++)
        {
            if (currentDeckUnitDatas[i] == null) continue;

            // 비트 플래그 기반으로 모든 시너지 확인
            UnitSynergyType synergyType = currentDeckUnitDatas[i].synergyType;
            foreach (UnitSynergyType type in _allSynergyTypes)
            {
                if ((synergyType & type) != 0)
                    synergyCounts[type]++;
            }
        }
        // 디버그 출력
        foreach (var kvp in synergyCounts)
        {
            Debug.Log($"Synergy: {kvp.Key}, Count: {kvp.Value}");
        }
        // 정보를 바탕으로 UI 업데이트
        UpdateSynergyUI();
    }
    void UpdateSynergyUI()
    {
        for(int typeIdx = 0; typeIdx < _allSynergyTypes.Length; typeIdx++)
        {
            // 시너지 없음 패스
            if (_allSynergyTypes[typeIdx] == UnitSynergyType.None) continue;

            // 우선 해당 계열 전부 비활성화
            for (int i = 0; i < coutsForSynergyGrade[typeIdx].Count; i++)
            {
                SynergyIcon iconKey = _iconMap[(_allSynergyTypes[typeIdx], i)];
                //synergyIconGOList[(int)iconKey].SetActive(false);
                synergyIconGOListForAuto[(int)iconKey].SetActive(false);
            }

            // 개수 최소 시너지 미만 패스
            if (synergyCounts[_allSynergyTypes[typeIdx]] < coutsForSynergyGrade[typeIdx][0]) continue;

            // 제일 큰 것부터 체크
            for (int i = coutsForSynergyGrade[typeIdx].Count - 1; i >= 0; i-- )
            {
                if (synergyCounts[_allSynergyTypes[typeIdx]] < coutsForSynergyGrade[typeIdx][i]) continue; // 해당 등급을 만족하는 개수가 아니면 패스

                // 해당 등급 아이콘 활성화
                SynergyIcon iconKey = _iconMap[(_allSynergyTypes[typeIdx], i)];
                //synergyIconGOList[(int)iconKey].SetActive(true);
                synergyIconGOListForAuto[(int)iconKey].SetActive(true);
                break;
            }

        }
    }

}
