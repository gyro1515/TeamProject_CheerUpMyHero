using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHQ : BaseHQ
{
    [Header("아군 본부 세팅")]
    [SerializeField] List<PoolType> playerUnits = new List<PoolType>();

    protected override void Awake()
    {
        base.Awake();

        //UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.PlayerUnit, new Vector2(300f, 16.5f));
        //UnitManager.Instance.AddUnitList(this, true);
        // 위와 다르게 아래는 바로 매니저를 호출한 이유는 다른 클래스의 start에서 GameManager.Instance.PlayerHQ를 사용하기 때문
        // 만약 이것도 이벤트로 바꾸면 start 실행 순서에 따라 null 참조가 발생할 수 있음
        GameManager.Instance.PlayerHQ = this;
    }
    protected override void Start()
    {
        EventManager.Publish(new SpawnHQEvent { baseHQ = this, type = EUIHpBarType.PlayerUnit, hpBarSize = new Vector2(300f, 16.5f), isPlayer = true });
        base.Start();
    }
    public override void Dead()
    {
        base.Dead();

        GameManager.Instance.ShowResultUI(false);
        Debug.Log("아군 HQ 파괴! 패배!");
    }
    /*protected override void SpawnUnit() // 현재 사용 안함
    {
        if (playerUnits.Count == 0) return;
        
        // 여기서 오브젝트 풀에서 가져오기
        GameObject playerUnitGO = ObjectPoolManager.Instance.Get(playerUnits[0]);
        playerUnitGO.transform.position = GetRandomSpawnPos();
        //playerUnitGO.transform.SetParent(gameObject.transform);
        //PlayerUnit playerUnit = playerUnitGO.GetComponent<PlayerUnit>();
    }*/
    public void SpawnUnit(PoolType poolType)
    {
        GameObject playerUnitGO = ObjectPoolManager.Instance.Get(poolType);
        playerUnitGO.transform.position = GetRandomSpawnPos();
    }
}
