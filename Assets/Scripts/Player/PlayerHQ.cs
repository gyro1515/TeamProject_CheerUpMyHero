using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHQ : BaseHQ
{
    [Header("아군 본부 세팅")]
    [SerializeField] List<GameObject> playerUnitPrefabs = new List<GameObject>();
    protected override void Awake()
    {
        base.Awake();
        UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.PlayerUnit, new Vector2(300f, 16.5f));
        UnitManager.Instance.PlayerUnitList.Add(this);
    }
    protected override void SpawnUnit()
    {
        if (playerUnitPrefabs[0] == null) return;
        Vector3 spawnPos = gameObject.transform.position;
        spawnPos.y += UnityEngine.Random.Range(tmpMinY, tmpMaxY) / 100f;
        GameObject playerUnitGO = Instantiate(playerUnitPrefabs[0], spawnPos, Quaternion.identity);
        playerUnitGO.transform.SetParent(gameObject.transform);
        PlayerUnit playerUnit = playerUnitGO.GetComponent<PlayerUnit>();
        playerUnit?.InitCharacter();
        UnitManager.Instance.PlayerUnitList.Add(playerUnit);
    }
}
