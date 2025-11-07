using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayerUnitSpawnPanel : BaseUI
{
    [Header("유닛 소환 패널 설정")]
    /*[SerializeField] GameObject spawnUnitSlotPrefab;
    [SerializeField] Transform spawnUnitSlotContainer;*/
    [SerializeField] List<UISpawnUnitSlot> spawnUnitSlotList;

    IEventPublisher<FinishTutorialDeckSettingEvent> finishTutorialDeckSettingEventPub;
    UnitSynergyType[] _allSynergyTypes = (UnitSynergyType[])Enum.GetValues(typeof(UnitSynergyType));
    SynergyGrade[] _allSynergyGrades = (SynergyGrade[])Enum.GetValues(typeof(SynergyGrade));
    // 시너지별 카운트 저장
    Dictionary<UnitSynergyType, int> synergyCounts = new Dictionary<UnitSynergyType, int>();
    /*List<List<int>> coutsForSynergyGrade = new List<List<int>>()
    {
        new List<int>(), // None
        new List<int>{2, 4, 6 }, // Kingdom
        new List<int> { 2, 4, 6 }, // Empire
        new List<int> { 2, 4, 6 }, // Mage
        new List<int> { 3, 4, 6 }, // Cleric
        new List<int> { 3, 5 },    // Berserker
        new List<int> { 3, 5 },    // Archer
        new List<int> { 2, 4, 6 }, // Hero
        new List<int> { 2, 3, 5 }, // Frost
        new List<int> { 1, 2, 3 }, // Burn
        new List<int> { 2, 3, 5 }, // Poison
    };*/
    Dictionary<(UnitSynergyType, SynergyGrade), int> coutsForSynergyGrade = new Dictionary<(UnitSynergyType, SynergyGrade), int>();
    private void Awake()
    {
        finishTutorialDeckSettingEventPub = EventManager.GetPublisher<FinishTutorialDeckSettingEvent>();

        int activeDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        //List<int> deckUnitIds = PlayerDataManager.Instance.DeckPresets[activeDeckIndex].UnitIds;
        List<BaseUnitData> deckBaseUnitDatas = PlayerDataManager.Instance.DeckPresets[activeDeckIndex].BaseUnitDatas;
        // 251029: 튜토리얼 진행시 덱이 징집병 방패병 도끼병 사냥꾼으로 고정되도록 변경
        if (!GameManager.IsTutorialCompleted)
        {
            Debug.Log("튜토리얼 덱 세팅");
            deckBaseUnitDatas[0] = DataManager.PlayerUnitData.GetData(100001);
            deckBaseUnitDatas[1] = DataManager.PlayerUnitData.GetData(100002);
            deckBaseUnitDatas[2] = DataManager.PlayerUnitData.GetData(100003);
            deckBaseUnitDatas[3] = DataManager.PlayerUnitData.GetData(100004);
            deckBaseUnitDatas[4] = null;
            deckBaseUnitDatas[5] = null;
            deckBaseUnitDatas[6] = null;
            deckBaseUnitDatas[7] = null;
            // 시너지 갱신: 원래는 카드 덱UI에서 처리하지만, 튜토리얼 덱 시너지는 여기서 강제 세팅
            CheckDeckUnitSynergy(deckBaseUnitDatas);
        }

        for (int i = 0; i < spawnUnitSlotList.Count; i++)
        {
            UISpawnUnitSlot unitSlot = spawnUnitSlotList[i];

            BaseUnitData cardData = deckBaseUnitDatas[i];

            unitSlot.InitSpawnUnitSlot(cardData);
        }

        // *******테스트용 코드
        /*BaseUnitData cardDatatt = DataManager.PlayerUnitData.GetData((int)PoolType.Allies_Unit4);
        spawnUnitSlotList[0].InitSpawnUnitSlot(cardDatatt.unitName, cardDatatt.idNumber, cardDatatt.poolType, cardDatatt.spawnCooldown, cardDatatt.cost);*/
        // **********
        //[Header("테스트용 플레이어 유닛")]
        //[SerializeField] List<PoolType> playerUnitList = new List<PoolType>();
        //private void Awake()
        //{
        //    if (!spawnUnitSlotPrefab || !spawnUnitSlotContainer) return;
        //    for (int i = 0; i < 9; i++)
        //    {
        //        UISpawnUnitSlot unitSlot = Instantiate(spawnUnitSlotPrefab, spawnUnitSlotContainer).GetComponent<UISpawnUnitSlot>();
        //        if (i >= playerUnitList.Count)
        //        {
        //            unitSlot.InitSpawnUnitSlot(null, -1, 0, -1);
        //            continue;
        //        }
        //        // 현재는 이렇게 가져오지만, 나중에는 플레이어 유닛 데이터 베이스에서 가져올 것
        //        GameObject unitPrefab = Resources.Load<GameObject>("Prefabs/ObjPooling/" + playerUnitList[i].ToString());

        //        PlayerUnit unit = unitPrefab.GetComponent<PlayerUnit>();
        //        // 변별을 위해 (int)playerUnitList[i] 사용 -> 인덱스화
        //        unitSlot.InitSpawnUnitSlot(null, (int)playerUnitList[i], unit.SpawnCooldown, unit.FoodConsumption);
        //    }
    }
    private void Start()
    {
        finishTutorialDeckSettingEventPub?.Publish();
    }
    void CheckDeckUnitSynergy(List<BaseUnitData> currentDeckUnitDatas)
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
        ApplySynergyForPlayerData();
    }
    void ApplySynergyForPlayerData()
    {
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
        // 저장된 시너지들 초기화
        PlayerDataManager.AppliedDeckUnitSynergies.Clear();
        foreach (UnitSynergyType type in _allSynergyTypes)
        {
            // 시너지 없음 패스
            if (type == UnitSynergyType.None) continue;
            // 개수 최소 시너지 미만 패스
            if (synergyCounts[type] < coutsForSynergyGrade[(type, SynergyGrade.Bronze)]) continue;

            // 제일 큰 것부터 체크
            for (int i = _allSynergyGrades.Length - 1; i >= 0; i--)
            {
                if (!coutsForSynergyGrade.TryGetValue((type, _allSynergyGrades[i]), out int synergyCnt)) continue;
                if (synergyCounts[type] < synergyCnt) continue; // 해당 등급을 만족하는 개수가 아니면 패스

                // 활성화된 시너지 저장
                PlayerDataManager.AppliedDeckUnitSynergies[type] = (SynergyGrade)i;
                break;
            }
        }
    }
}
public struct FinishTutorialDeckSettingEvent
{

}
