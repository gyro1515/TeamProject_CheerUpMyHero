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
    private float offsetPlus = 3.5f;

    private Color32 playerColor = new Color32(0, 0, 255, 100);
    private Color32 enemyColor = new Color32(255, 0, 0, 135);

    private float wordWidth;
    private float uIWidth;
    private Dictionary<BaseCharacter, UIMiniMapIcon> playerUnitIconsPair = new();
    private Dictionary<BaseCharacter, UIMiniMapIcon> enemyUnitIconsPair = new();
    private Dictionary<BaseCharacter, UIMiniMapIcon> playerPair = new();


    private void OnEnable()
    {
        UnitManager.Instance.onUnitSpawn += AddToMinMapUnitList;
        UnitManager.Instance.onUnitDeSpawn += RemoveFromMiniMapUnitList;
    }
    private void OnDisable()
    {
        UnitManager.Instance.onUnitSpawn -= AddToMinMapUnitList;
        UnitManager.Instance.onUnitDeSpawn -= RemoveFromMiniMapUnitList;
    }


    private void Start()
    {
        AddToMinmapPlayer(GameManager.Instance.Player);

        playerHQPos = GameManager.Instance.PlayerHQ.transform.position.x;
        enemyHQPos = GameManager.Instance.enemyHQ.transform.position.x;

        wordWidth = (enemyHQPos + offsetPlus) - playerHQPos;
        uIWidth = rectTransform.rect.width;

        Debug.Log($"월드 크기: {wordWidth}");
        Debug.Log($"UI 크기: {uIWidth}");
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
        float xRatio = wordXPos / wordWidth;
        float minimapX = xRatio * uIWidth;
        Vector2 minmapPos = new Vector2 (minimapX, 0);
        return minmapPos;
    }

    void AddToMinMapUnitList(BaseCharacter unit, bool isPlayer)
    {
        Dictionary<BaseCharacter, UIMiniMapIcon> unitPiar = isPlayer ? playerUnitIconsPair : enemyUnitIconsPair;
        Color unitColor = isPlayer ? playerColor : enemyColor;

        GameObject obj = ObjectPoolManager.Instance.Get(PoolType.UIMinimapIcon);

        //부모 변경해야 캔버스에 표시됨. 반대로 반납할 때는 부모 원위치 필요
        obj.transform.SetParent(unitsTransform, false); // 두번째 인자 false 하면 기존 월드좌표를 리셋하고 부모를 바꿈
        UIMiniMapIcon icon = obj.GetComponent<UIMiniMapIcon>();
        icon.ResetRectTransform();
        icon.SetColor(unitColor);
        unitPiar.Add(unit, icon);
    }

    void RemoveFromMiniMapUnitList(BaseCharacter unit, bool isPlayer)
    {
        Dictionary<BaseCharacter, UIMiniMapIcon> unitPiar = isPlayer ? playerUnitIconsPair : enemyUnitIconsPair;

        unitPiar[unit].ReleaseSelf();
        unitPiar.Remove(unit);
    }

    //플레이어 아이콘 전용
    void AddToMinmapPlayer(BaseCharacter unit)
    {
        UIMiniMapIcon playerIcon = Instantiate<UIMiniMapIcon>(playerIconPrefab, playerTransform);
        playerPair.Add(unit, playerIcon);
    }
}
 