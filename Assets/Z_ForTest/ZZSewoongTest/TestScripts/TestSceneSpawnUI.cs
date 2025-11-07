using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSceneSpawnUI : MonoBehaviour
{
    [Header("유닛 소환 패널 설정")]
    [SerializeField] GameObject spawnUnitSlotPrefab;
    [SerializeField] Transform spawnUnitSlotContainer;
    [Header("테스트용 플레이어 유닛")]
    [SerializeField] List<PoolType> playerUnitList = new List<PoolType>();
    [Header("테스트용 적 유닛")]
    [SerializeField] List<PoolType> enemyUnitList = new List<PoolType>();
    private void Awake()
    {
        if (!spawnUnitSlotPrefab || !spawnUnitSlotContainer) return;
        for (int i = 0; i < 9; i++)
        {
            UISpawnUnitSlot unitSlot = Instantiate(spawnUnitSlotPrefab, spawnUnitSlotContainer).GetComponent<UISpawnUnitSlot>();
            if (i >= playerUnitList.Count)
            {
               // unitSlot.InitSpawnUnitSlot(null, -1, 0, -1);
                //unitSlot.InitSpawnUnitSlot("비었음", -1, PoolType.None, 0, -1);

                continue;
            }
            // 현재는 이렇게 가져오지만, 나중에는 플레이어 유닛 데이터 베이스에서 가져올 것
            GameObject unitPrefab = Resources.Load<GameObject>("Prefabs/ObjPooling/" + playerUnitList[i].ToString());

            PlayerUnit unit = unitPrefab.GetComponent<PlayerUnit>();
            // 변별을 위해 (int)playerUnitList[i] 사용 -> 인덱스화
           // unitSlot.InitSpawnUnitSlot(null, (int)playerUnitList[i], unit.SpawnCooldown, unit.FoodConsumption);//코스트 추가했습니다 빨간줄 뜨길래..
        }
    }
}
