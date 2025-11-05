using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : BaseUnit
{
    //float statMultiplier = 1f;
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
        //EventManager.Instance.Publish(new SpawnUnitEvent { baseUnit = this, isPlayer = false });
    }
    protected override EffectTarget GetEffectTarget()
    {
        return EffectTarget.EnemyUnit;
    }
    public override void SetStatMultiplier(float statMultiplier, bool isSpawnHero = false)
    {
        if (UnitData == null) { Debug.LogError("데이터 없음"); return; }

        EffectTarget target = GetEffectTarget();
        float hpModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.MaxHp, this.UnitData);
        float atkModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.AtkPower, this.UnitData);
        float moveSpeedModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.MoveSpeed, this.UnitData);
        // float spawnCooldownModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.SpawnCooldown, this.UnitData);

        // 배율에 따른 체력 공격력 세팅
        MaxHp = UnitData.health * (hpModifierBonus + statMultiplier);
        curHp = MaxHp;
        AtkPower = UnitData.atkPower * (atkModifierBonus + statMultiplier);
        MoveSpeed = UnitData.moveSpeed * (1f + moveSpeedModifierBonus);
        // SpawnCooldown = UnitData.spawnCooldown * spawnCooldownModifierBonus;
        AttackRate = UnitData.attackRate * statMultiplier; // 공격 속도는 크기와 상관없이 배율에 비례
        float tmpstatMultiplier = Math.Clamp(statMultiplier, 0.8f, 1.2f); // 크기는 너무 작아지거나 커지지 않도록 제한
        // 아래는 다 tmpstatMultiplier로 세팅, 크기에 따라 인식/공격 범위도 달라지도록
        if (UnitData.unitClass == UnitClass.Boss) tmpstatMultiplier = 2.0f;
        gameObject.transform.localScale = TmpSize * tmpstatMultiplier;
        AttackRange = UnitData.attackRange * tmpstatMultiplier;
        CognizanceRange = UnitData.cognizanceRange * tmpstatMultiplier;

        /*CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        // 사이즈는 달라질 수 있으니 활성화 시마다 갱신
        knockbackHandler.Init(col.size.x * statMultiplier);*/
        knockbackHandler.Init((TmpSize * tmpstatMultiplier).x);
        // ex: 최대 체력 = 300 / HitBackCount = 3 => 데미지 100이 누적될때마다 히트백
        hitbackHp = MaxHp / UnitData.hitBack;
        // ex: curHp / hitbackHp  => 2 -> 1 -> 0에서만 히트백이 발생하도록
        hitbackTriggerCount = UnitData.hitBack - 1;
    }
    protected override void SetDataFromExcelData()
    {
        if (!Enum.TryParse(gameObject.name, out PoolType poolType))
        { Debug.LogError($"변환 실패: {gameObject.name} 은(는) PoolType에 없습니다."); return; }

        UnitData = DataManager.EnemyUnitData.GetData((int)poolType);
        if (UnitData.unitType != UnitType.Healer) // 힐러는 따로
        {
            switch (UnitData.attackType)
            {
                case UnitAttackType.Target:
                    UnitController = gameObject.AddComponent<EnemyUnitController>();
                    break;
                case UnitAttackType.Area:
                    UnitController = gameObject.AddComponent<EnemyRangedSplashController>();
                    break;
                case UnitAttackType.PierceArea:
                    UnitController = gameObject.AddComponent<EnemyMeleeSplashController>();
                    break;
            }
        }
        else
        {
            switch (UnitData.attackType)
            {
                case UnitAttackType.Target:
                    UnitController = gameObject.AddComponent<EnemyHealerUnitController>();
                    break;
                case UnitAttackType.Area:
                case UnitAttackType.PierceArea:
                    UnitController = gameObject.AddComponent<EnemyHealerSplashController>();
                    break;
                default:
                    Debug.LogError("유닛 데이터 테이블 오류, 일단 단일 타겟 힐러 컨트롤러 부착");
                    UnitController = gameObject.AddComponent<PlayerHealerUnitController>();
                    break;
            }

        }
        BaseController = UnitController;
        Damageable = GetComponent<IDamageable>();
    }

    protected override void Start()
    {
        base.Start();
    }
}
// 스폰 유닛 이벤트
public struct SpawnUnitEvent
{
    public BaseUnit baseUnit;
    public bool isPlayer;
}
