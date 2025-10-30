using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMiniMap : MonoBehaviour
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Transform unitsTransform;
    [SerializeField] Transform playerTransform;

    [SerializeField] UIMiniMapIcon playerIconPrefab;

    private float playerHQPos;
    private float enemyHQPos;
    //아마 HQ가 좌우대칭이 아니여서 UI가 삐져나오는 것 같은데, 일단 위치수정하기 전까지 보정치 주기
    //private float offsetPlus = 3.5f;

    private Color32 playerColor = new Color32(255, 255, 255, 100);
    private Color32 heroColor = new Color32(255, 255, 0, 255);
    private Color32 enemyColor = new Color32(255, 0, 0, 135);
    private Color32 bossColor = new Color32(180, 0, 255, 255);

    private float wordWidth;
    private float uIWidth;
    private Dictionary<BaseCharacter, UIMiniMapIcon> playerUnitIconsPair = new();
    private Dictionary<BaseCharacter, UIMiniMapIcon> enemyUnitIconsPair = new();
    private Dictionary<BaseCharacter, UIMiniMapIcon> playerPair = new();

    // 버그 테스트 용입니다(원진)
    private float worldCenter;

    private void OnEnable()
    {
        UnitManager.Instance.onUnitSpawn += AddToMinMapUnitList;
        UnitManager.Instance.onUnitDeSpawn += RemoveFromMiniMapUnitList;
    }
    private void OnDisable()
    {
        // 251021 수정: 씬 전환과 함께 유닛 매니저도 이 오브젝트도 파괴되므로 구독 해제 불필요
        /*UnitManager.Instance.onUnitSpawn -= AddToMinMapUnitList;
        UnitManager.Instance.onUnitDeSpawn -= RemoveFromMiniMapUnitList;*/
    }


    private void Start()
    {
        AddToMinmapPlayer(GameManager.Instance.Player);

        playerHQPos = GameManager.Instance.PlayerHQ.transform.position.x;
        enemyHQPos = GameManager.Instance.enemyHQ.transform.position.x;

        //wordWidth = (enemyHQPos + offsetPlus) - playerHQPos;
        wordWidth = enemyHQPos - playerHQPos;
        uIWidth = rectTransform.rect.width;
        uIWidth *= 0.93f; // 캔버스에 있는 HQ 위치 보정 

        //Debug.Log($"월드 크기: {wordWidth}");
        //Debug.Log($"UI 크기: {uIWidth}");
        //********
        worldCenter = enemyHQPos + playerHQPos;
        worldCenter /= 2;

    }

    //한박자 늦게 업데이트
    private void LateUpdate()
    {
        UpdateUnitPos(playerUnitIconsPair);
        UpdateUnitPos(enemyUnitIconsPair);

        if (GameManager.Instance.Player  != null)
            UpdateUnitPos(playerPair);
    }

    void UpdateUnitPos(Dictionary<BaseCharacter, UIMiniMapIcon> unitIconPair)
    {
        foreach (var unitPair in unitIconPair)
        {
            BaseCharacter unit = unitPair.Key;
            Image unitIcon = unitPair.Value.IconImage;

            if(unit != null)
                unitIcon.rectTransform.anchoredPosition = WorldPosToUIPos(unit.transform.position.x);
        }
    }

    Vector2 WorldPosToUIPos(float wordXPos)
    {
        /*float xRatio = wordXPos / wordWidth;
        float minimapX = xRatio * uIWidth;*/
        // 수정 버전************
        float curPos = wordXPos - worldCenter; // 가운데 기준에서 얼마나 떨어져 있는가
        float xRatio = curPos / wordWidth;
        float minimapX = xRatio * uIWidth;
        Vector2 minmapPos = new Vector2 (minimapX, 0);
        return minmapPos;
    }

    //void AddToMinMapUnitList(BaseCharacter unit, bool isPlayer)
    void AddToMinMapUnitList(BaseUnit unit, bool isPlayer)
    {
        Dictionary<BaseCharacter, UIMiniMapIcon> unitPair = isPlayer ? playerUnitIconsPair : enemyUnitIconsPair;
        Color unitColor = isPlayer ? playerColor : enemyColor;
        bool isHeroOrBoss = false;
        // 영웅/보스는 색상 다르게
        switch (unit.UnitData.unitClass)
        {
            case UnitClass.Hero:
                unitColor = heroColor;
                isHeroOrBoss = true;
                break;
            case UnitClass.Boss:
                unitColor = bossColor;
                isHeroOrBoss = true;
                break;
        }
        GameObject obj = ObjectPoolManager.Instance.Get(PoolType.UIMinimapIcon);

        //부모 변경해야 캔버스에 표시됨. 반대로 반납할 때는 부모 원위치 필요
        //obj.transform.SetParent(unitsTransform, false); // 두번째 인자 false 하면 기존 월드좌표를 리셋하고 부모를 바꿈
        //obj.transform.SetParent(unitsTransform); // 두번째 인자 false 하면 기존 월드좌표를 리셋하고 부모를 바꿈
        UIMiniMapIcon icon = obj.GetComponent<UIMiniMapIcon>();
        // 보스 및 영웅 아이콘 크기 조정, 트랜스폼 부모도 다르게
        if (isHeroOrBoss)
        {
            obj.transform.SetParent(playerTransform); // 두번째 인자 false 하면 기존 월드좌표를 리셋하고 부모를 바꿈
            icon.ResetRectTransformForHeroAndBoss();
        }
        else
        {
            obj.transform.SetParent(unitsTransform); // 두번째 인자 false 하면 기존 월드좌표를 리셋하고 부모를 바꿈
            icon.ResetRectTransform();
        }
        icon.SetColor(unitColor);
        unitPair.Add(unit, icon);
    }

    void RemoveFromMiniMapUnitList(BaseCharacter unit, bool isPlayer)
    {
        Dictionary<BaseCharacter, UIMiniMapIcon> unitPiar = isPlayer ? playerUnitIconsPair : enemyUnitIconsPair;

        if (!unitPiar.ContainsKey(unit))
        {
            // 플레이어는 여기에 포함 안돼서 오류 뜹니다. 따라서 일단 이 부분에 플레이어 삭제할게요
            RemoveFromMinmapPlayer(unit);
            return;
        }
        unitPiar[unit].ReleaseSelf();
        unitPiar.Remove(unit);
    }

    //플레이어 아이콘 전용
    void AddToMinmapPlayer(BaseCharacter unit)
    {
        UIMiniMapIcon playerIcon = Instantiate<UIMiniMapIcon>(playerIconPrefab, playerTransform);
        playerPair.Add(unit, playerIcon);
    }
    void RemoveFromMinmapPlayer(BaseCharacter unit)
    {
        //playerPair[unit].ReleaseSelf(); 플레이어는 오브젝트 풀링 아님
        Destroy(playerPair[unit].gameObject);
        playerPair.Remove(unit);
    }
}
 