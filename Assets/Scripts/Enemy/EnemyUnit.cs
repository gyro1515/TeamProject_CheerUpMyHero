using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : BaseUnit
{
    float statMultiplier = 1f;
    protected override void Awake()
    {
        base.Awake();
        OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(this, false);
        };
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UnitManager.Instance.AddUnitList(this, false);
    }
    protected override void Start()
    {
        base.Start();

    }
    public void SetStatMultiplierByWave(float statMultiplier)
    {
        // 배율에 따른 체력 공격력 세팅
        MaxHp = TmpMaxHp * statMultiplier;
        curHp = MaxHp;
        AtkPower = TmpAtkPower * statMultiplier;
        gameObject.transform.localScale = TmpSize * statMultiplier;

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        // 사이즈는 달라질 수 있으니 활성화 시마다 갱신
        knockbackHandler.Init(col.size.x * statMultiplier);
        // ex: 최대 체력 = 300 / HitBackCount = 3 => 데미지 100이 누적될때마다 히트백
        hitbackHp = MaxHp / HitBackCount;
        // ex: curHp / hitbackHp  => 2 -> 1 -> 0에서만 히트백이 발생하도록
        hitbackTriggerCount = HitBackCount - 1;
    }


}
