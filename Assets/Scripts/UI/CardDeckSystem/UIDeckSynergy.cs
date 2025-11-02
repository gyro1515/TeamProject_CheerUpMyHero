using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public enum SynergyIcon
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
public class UIDeckSynergy : MonoBehaviour
{
    
    [Header("시너지 UI 설정")]
    [SerializeField] GameObject synergyIconPrefab;
    [SerializeField] Transform synergyIconParent;
    [SerializeField] HorizontalLayoutGroup synergyLayoutGroup;
    [SerializeField] List<GameObject> synergyIconGOList = new List<GameObject>();
    [SerializeField] RectTransform layoutGropRT;
    [SerializeField] UISynegyToolTipPanel uISynegyToolTipPanel;
    // 시너지별 카운트 저장
    Dictionary<UnitSynergyType, int> synergyCounts = new Dictionary<UnitSynergyType, int>();
    //List<int> synergyCounts;
    // 아이콘 저장 
    Dictionary<SynergyIcon, Sprite> synergyIcon = new Dictionary<SynergyIcon, Sprite>();
    // 아이콘 GO 저장용 딕셔너리
    List<SynergyIconSlot> synergyIconGOListForAuto = new List<SynergyIconSlot>();

    // 힙 할당 줄이기위한 자료구조
    // enum 배열
    UnitSynergyType[] _allSynergyTypes = (UnitSynergyType[])Enum.GetValues(typeof(UnitSynergyType));
    SynergyIcon[] _allSynergies = (SynergyIcon[])Enum.GetValues(typeof(SynergyIcon));
    SynergyGrade[] _allSynergyGrades = (SynergyGrade[])Enum.GetValues(typeof(SynergyGrade));

    // 시너지 체크용 자료구조
    Dictionary<(UnitSynergyType, SynergyGrade), int> coutsForSynergyGrade = new Dictionary<(UnitSynergyType, SynergyGrade), int>();
    /*List<List<int>> coutsForSynergyGrade = new List<List<int>>() 
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
    };*/
    // UnitSynergyType, count -> SynergyIcon
    private Dictionary<(UnitSynergyType, int), SynergyIcon> _iconMap = new()
    {
        // 프리즘 등급 (Index = 2)
        { (UnitSynergyType.Kingdom, 2), SynergyIcon.Kingdom_2 },
        { (UnitSynergyType.Empire, 2), SynergyIcon.Empire_2 },
        { (UnitSynergyType.Mage, 2), SynergyIcon.Mage_2 },
        { (UnitSynergyType.Cleric, 2), SynergyIcon.Cleric_2 },
        { (UnitSynergyType.Hero, 2), SynergyIcon.Hero_2 },
        { (UnitSynergyType.Frost, 2), SynergyIcon.Frost_2 },
        { (UnitSynergyType.Burn, 2), SynergyIcon.Burn_2 },
        { (UnitSynergyType.Poison, 2), SynergyIcon.Poison_2 },

        // 골드 등급 (Index = 1)
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

        // 브론즈 등급 (Index = 0)
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
    // SynergyIcon -> UnitSynergyType, count
    private Dictionary<SynergyIcon, (UnitSynergyType, int)> synergyIconToTypeAndCount = new()
    {
        // 프리즘 등급 (Index = 2)
        { SynergyIcon.Kingdom_2, (UnitSynergyType.Kingdom, 2) },
        { SynergyIcon.Empire_2,  (UnitSynergyType.Empire, 2) },
        { SynergyIcon.Mage_2,    (UnitSynergyType.Mage, 2) },
        { SynergyIcon.Cleric_2,  (UnitSynergyType.Cleric, 2) },
        { SynergyIcon.Hero_2,    (UnitSynergyType.Hero, 2) },
        { SynergyIcon.Frost_2,   (UnitSynergyType.Frost, 2) },
        { SynergyIcon.Burn_2,    (UnitSynergyType.Burn, 2) },
        { SynergyIcon.Poison_2,  (UnitSynergyType.Poison, 2) },
        
        // 골드 등급 (Index = 1)
        { SynergyIcon.Kingdom_1,   (UnitSynergyType.Kingdom, 1) },
        { SynergyIcon.Empire_1,    (UnitSynergyType.Empire, 1) },
        { SynergyIcon.Mage_1,      (UnitSynergyType.Mage, 1) },
        { SynergyIcon.Cleric_1,    (UnitSynergyType.Cleric, 1) },
        { SynergyIcon.Berserker_1, (UnitSynergyType.Berserker, 1) },
        { SynergyIcon.Archer_1,    (UnitSynergyType.Archer, 1) },
        { SynergyIcon.Hero_1,      (UnitSynergyType.Hero, 1) },
        { SynergyIcon.Frost_1,     (UnitSynergyType.Frost, 1) },
        { SynergyIcon.Burn_1,      (UnitSynergyType.Burn, 1) },
        { SynergyIcon.Poison_1,    (UnitSynergyType.Poison, 1) },
        
        // 브론즈 등급 (Index = 0)
        { SynergyIcon.Kingdom_0,   (UnitSynergyType.Kingdom, 0) },
        { SynergyIcon.Empire_0,    (UnitSynergyType.Empire, 0) },
        { SynergyIcon.Mage_0,      (UnitSynergyType.Mage, 0) },
        { SynergyIcon.Cleric_0,    (UnitSynergyType.Cleric, 0) },
        { SynergyIcon.Berserker_0, (UnitSynergyType.Berserker, 0) },
        { SynergyIcon.Archer_0,    (UnitSynergyType.Archer, 0) },
        { SynergyIcon.Hero_0,      (UnitSynergyType.Hero, 0) },
        { SynergyIcon.Frost_0,     (UnitSynergyType.Frost, 0) },
        { SynergyIcon.Burn_0,      (UnitSynergyType.Burn, 0) },
        { SynergyIcon.Poison_0,    (UnitSynergyType.Poison, 0) },
    };

    // 시너지 클릭시 툴팁 출력
    //IEventPublisher<SynergyClickedEvent> synergyClickedEvent;


    // 초기화
    // 너무 무거워질거 같아서 그냥 노가다로 세팅해야 하지 않을까 싶습니다...
    // 일단 자동화와 노가다 방식 둘 다 준비해두었습니다.
    public void Init()
    {
        //synergyClickedEvent = EventManager.GetPublisher<SynergyClickedEvent>();

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
            // iconGO에 GetComponent많이 할 수록, 스크립트로 추가하여 한번에 초기화하는 것보다 성능이 안좋아짐****
            GameObject iconGO = Instantiate(synergyIconPrefab, synergyIconParent);
            /*Image iconImage = iconGO.GetComponent<Image>();
            iconImage.sprite = synergyIcon[type];
            Button iconBtn = iconGO.GetComponent<Button>();
            (UnitSynergyType synergyType, int count) = synergyIconToTypeAndCount[type];
            // 해당 버튼 클릭시 SynergyClickedEvent발행: 툴팁 표시용
            iconBtn.onClick.AddListener(() => synergyClickedEvent.Publish(new SynergyClickedEvent(synergyType, count)));*/

            // 스크립트로 한번에 초기화
            SynergyIconSlot iconSlot = iconGO.GetComponent<SynergyIconSlot>();
            (UnitSynergyType synergyType, int count) = synergyIconToTypeAndCount[type];
            iconSlot.Init(synergyIcon[type], () =>
            {
                //synergyClickedEvent.Publish(new SynergyClickedEvent(synergyType, count));
                uISynegyToolTipPanel.OnSynergyClicked(synergyType, count, synergyCounts[synergyIconToTypeAndCount[type].Item1]);
                //Debug.Log("시너지 아이콘 클릭됨");
            });
            synergyIconGOListForAuto.Add(iconSlot);
            iconGO.SetActive(false);
        }
        // 데이터 매니저에서 시너지 등급별 필요 유닛 수 불러와서 저장
        foreach (UnitSynergyType type in _allSynergyTypes)
        {
            foreach (var grade in _allSynergyGrades)
            {
                // 기존에 적용된 시너지 모두 초기화
                SynergyData data = DataManager.SynergyEffectData.GetData((int)type * 1000 + (int)grade);
                if (data == null) break;
                coutsForSynergyGrade[(type, grade)] = data.requiredUnitCount;
            }
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
        /*foreach (var kvp in synergyCounts)
        {
            Debug.Log($"Synergy: {kvp.Key}, Count: {kvp.Value}");
        }*/
        // 정보를 바탕으로 UI 업데이트
        UpdateSynergyUI();
    }
    void UpdateSynergyUI()
    {
        // 저장된 시너지들 초기화
        PlayerDataManager.AppliedDeckUnitSynergies.Clear();

        int activeSynergyCount = 0;
        foreach (UnitSynergyType type in _allSynergyTypes)
        {
            // 시너지 없음 패스
            if (type == UnitSynergyType.None) continue;

            // 우선 해당 계열 전부 비활성화
            for (int i = 0; i < _allSynergyGrades.Length; i++)
            {
                if (!_iconMap.TryGetValue((type, i), out SynergyIcon iconKey)) continue;
                //synergyIconGOList[(int)iconKey].SetActive(false);
                synergyIconGOListForAuto[(int)iconKey].gameObject.SetActive(false);
            }

            // 개수 최소 시너지 미만 패스
            if (synergyCounts[type] < coutsForSynergyGrade[(type, SynergyGrade.Bronze)]) continue;

            // 제일 큰 것부터 체크
            for (int i = _allSynergyGrades.Length - 1; i >= 0; i--)
            {
                if (!coutsForSynergyGrade.TryGetValue((type, _allSynergyGrades[i]), out int synergyCnt)) continue;
                if (synergyCounts[type] < synergyCnt) continue; // 해당 등급을 만족하는 개수가 아니면 패스

                // 해당 등급 아이콘 활성화
                SynergyIcon iconKey = _iconMap[(type, i)];
                //synergyIconGOList[(int)iconKey].SetActive(true);
                synergyIconGOListForAuto[(int)iconKey].gameObject.SetActive(true);
                activeSynergyCount++;
                // 활성화된 시너지 저장
                PlayerDataManager.AppliedDeckUnitSynergies[type] = (SynergyGrade)i;
                break;
            }
        }
        /*foreach (var item in PlayerDataManager.AppliedDeckUnitSynergies)
        {
            Debug.Log($"{item.Key}시너지의 {item.Value}등급 활성화");
        }*/

        UpdateSynergyUISize(activeSynergyCount);
    }
    void UpdateSynergyUISize(int activeSynergyCount)
    {
        // 활성화된 시너지 아이콘 개수에 따라 시너지 아이콘 크기 조정
        float rectSizeX = layoutGropRT.rect.size.x;
        float rectSizeY = layoutGropRT.rect.size.y;
        rectSizeX -= synergyLayoutGroup.padding.left + synergyLayoutGroup.padding.right;
        rectSizeX -= synergyLayoutGroup.spacing * (activeSynergyCount - 1);
        float iconSize = rectSizeX / activeSynergyCount;
        iconSize = Mathf.Min(iconSize, rectSizeY); // 세로 크기보다 커지면 안됨
        for (int i = 0; i < synergyIconGOListForAuto.Count; i++)
        {
            synergyIconGOListForAuto[i].ResizeIcon(iconSize);
        }

    }

}
#region 시너지 아이콘 클릭시 툴팁 출력 이벤트 데이터
public struct SynergyClickedEvent
{
    public UnitSynergyType synergyType;
    public int currentCount;
    public SynergyClickedEvent(UnitSynergyType type, int count)
    {
        synergyType = type;
        currentCount = count;
    }
}
#endregion
