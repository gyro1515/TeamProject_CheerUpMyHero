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
}
