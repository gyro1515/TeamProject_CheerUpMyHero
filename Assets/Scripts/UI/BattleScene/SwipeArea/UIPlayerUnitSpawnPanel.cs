using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayerUnitSpawnPanel : BaseUI
{
    [Header("유닛 소환 패널 설정")]
    /*[SerializeField] GameObject spawnUnitSlotPrefab;
    [SerializeField] Transform spawnUnitSlotContainer;*/
    [SerializeField] List<UISpawnUnitSlot> spawnUnitSlotList;

    private void Awake()
    {
        /*if (!spawnUnitSlotPrefab || !spawnUnitSlotContainer) return;*/

        //PlayerDataManager에서 현재 활성화된 덱 정보를 가져옴
        ////테스트 코드**********
        //PlayerDataManager.Instance.DeckPresets[1].UnitIds[0] = 100001;
        //PlayerDataManager.Instance.DeckPresets[1].UnitIds[1] = 100002;
        //PlayerDataManager.Instance.DeckPresets[1].UnitIds[2] = 100003;
        // PlayerDataManager.Instance.DeckPresets[1].UnitIds[0] = 100012;
        // PlayerDataManager.Instance.DeckPresets[1].UnitIds[1] = 100013;
        // PlayerDataManager.Instance.DeckPresets[1].UnitIds[2] = 100014;
        //********
        int activeDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        //List<int> deckUnitIds = PlayerDataManager.Instance.DeckPresets[activeDeckIndex].UnitIds;
        List<BaseUnitData> deckBaseUnitDatas = PlayerDataManager.Instance.DeckPresets[activeDeckIndex].BaseUnitDatas;
        // 251023: 배틀 씬으로 바로 진입 시 덱 정보가 없으므로 예외 처리 추가
        bool needSet = true;
        for (int i = 0; i < deckBaseUnitDatas.Count; i++)
        {
            if (deckBaseUnitDatas[i] == null) continue;
            needSet = false;
            break;
        }
        if (needSet)
        {
            Debug.Log("덱 정보 임의 세팅");
            deckBaseUnitDatas[0] = DataManager.PlayerUnitData.GetData(100001);
            deckBaseUnitDatas[1] = DataManager.PlayerUnitData.GetData(100002);
            deckBaseUnitDatas[2] = DataManager.PlayerUnitData.GetData(100003);
            deckBaseUnitDatas[3] = DataManager.PlayerUnitData.GetData(100004);
            deckBaseUnitDatas[4] = DataManager.PlayerUnitData.GetData(100005);
            deckBaseUnitDatas[5] = DataManager.PlayerUnitData.GetData(105010);
        }

        // 가져온 덱 정보로 슬롯을 세팅: 2501023 변경
        // 251023: 엑셀 데이터로 교체 완료
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
}
