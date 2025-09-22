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
    public void SetStatMultiplierByWave(int waveIdx)
    {
        // 현재는 웨이브에 따라 배율이 달라지도록 구현했지만
        // 추후 웨이브 데이터에 배율이 있다면
        // 웨이브 데이터의 배율에 따라 스탯 설정하도록 변경해야 함
        switch (waveIdx)
        {
            case 0:
                statMultiplier = 0.5f;
                break;
            case 1:
                statMultiplier = 0.75f;
                break;
            case 2:
                statMultiplier = 1.0f;
                break;
            case 3:
                statMultiplier = 1.25f;
                break;
            case 4:
                statMultiplier = 1.5f;
                break;
        }
        // 배율에 따른 체력 공격력 세팅
        MaxHp = TmpMaxHp * statMultiplier;
        curHp = MaxHp;
        AtkPower = TmpAtkPower * statMultiplier;
        gameObject.transform.localScale = TmpSize * statMultiplier;
    }


}
