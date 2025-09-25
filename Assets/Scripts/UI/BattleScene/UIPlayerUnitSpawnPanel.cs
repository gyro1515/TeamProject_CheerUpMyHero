using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayerUnitSpawnPanel : BaseUI
{
    [Header("유닛 소환 패널 설정")]
    [SerializeField] GameObject spawnUnitSlotPrefab;
    [SerializeField] Transform spawnUnitSlotContainer;

    private void Awake()
    {
        if (!spawnUnitSlotPrefab || !spawnUnitSlotContainer) return;

        //PlayerDataManager에서 현재 활성화된 덱 정보를 가져옴
        int activeDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        List<int> deckUnitIds = PlayerDataManager.Instance.DeckPresets[activeDeckIndex].UnitIds;

        // 가져온 덱 정보로 슬롯을 생성
        for (int i = 0; i < deckUnitIds.Count; i++)
        {
            UISpawnUnitSlot unitSlot = Instantiate(spawnUnitSlotPrefab, spawnUnitSlotContainer).GetComponent<UISpawnUnitSlot>();

            int unitId = deckUnitIds[i];

            if (unitId == -1) // 빈 슬롯은 비활성화
            {
                unitSlot.InitSpawnUnitSlot(null, "비었음", -1, PoolType.None, 0, -1);
                continue;
            }

            // PlayerDataManager에서 unitId로 임시 카드 데이터를 가져옴
            TempCardData cardData = PlayerDataManager.Instance.GetUnitData(unitId);

            if (cardData != null)
            {
                // 카드 데이터를 사용해 전투 소환 슬롯을 초기화합니다.
                unitSlot.InitSpawnUnitSlot(null, cardData.unitName, unitId, cardData.poolType, cardData.coolTime, cardData.cost);
            }
        }

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
