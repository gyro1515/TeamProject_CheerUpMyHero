using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : BaseUnit
{
    protected override void Awake()
    {
        base.Awake();
        OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(this, true);
        };

    }
    protected override void Start()
    {
        base.Start();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UnitManager.Instance.AddUnitList(this, true);
        
    }
    public override void SetStatMultiplier(float statMultiplier)
    {
        if (UnitData == null) { Debug.LogError("데이터 없음"); return; }

        float synergyHealthBonus = PlayerDataManager.Instance.SynergyAllUnitHealthBonus;
        float synergyAttackBonus = PlayerDataManager.Instance.SynergyAllUnitAttackBonus;
        float synergyAttackCooldownReduction = PlayerDataManager.Instance.SynergyUnitAttackCooldownReduction;

        // 배율에 따른 체력 공격력 세팅
        MaxHp = UnitData.health * statMultiplier * (1.0f + synergyHealthBonus / 100.0f);
        curHp = MaxHp;
        AtkPower = UnitData.atkPower * statMultiplier * (1.0f + synergyAttackBonus / 100.0f);
        AttackRate = UnitData.attackRate * statMultiplier * (1.0f - synergyAttackCooldownReduction / 100.0f); // 공격 속도는 크기와 상관없이 배율에 비례
        float tmpstatMultiplier = Math.Clamp(statMultiplier, 0.8f, 1.2f); // 크기는 너무 작아지거나 커지지 않도록 제한
        // 아래는 다 tmpstatMultiplier로 세팅, 크기에 따라 인식/공격 범위도 달라지도록
        gameObject.transform.localScale = TmpSize * tmpstatMultiplier;
        AttackRange = UnitData.attackRange * tmpstatMultiplier;
        CognizanceRange = UnitData.cognizanceRange * tmpstatMultiplier;

        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        // 사이즈는 달라질 수 있으니 활성화 시마다 갱신
        knockbackHandler.Init(col.size.x * statMultiplier);
        // ex: 최대 체력 = 300 / HitBackCount = 3 => 데미지 100이 누적될때마다 히트백
        hitbackHp = MaxHp / UnitData.hitBack;
        // ex: curHp / hitbackHp  => 2 -> 1 -> 0에서만 히트백이 발생하도록
        hitbackTriggerCount = UnitData.hitBack - 1;
    }
    protected override void SetDataFromExcelData()
    {
        if (!Enum.TryParse(gameObject.name, out PoolType poolType))
        { Debug.LogError($"변환 실패: {gameObject.name} 은(는) PoolType에 없습니다."); return; }

        UnitData = DataManager.PlayerUnitData.GetData((int)poolType);
        // 컨트롤러 자동추가 테스트
        if(UnitController == null) // 컨트롤러 없다면
        {
            if(UnitData.unitType != UnitType.Healer) // 힐러는 따로
            {
                switch (UnitData.attackType)
                {
                    case UnitAttackType.Target:
                        UnitController = gameObject.AddComponent<PlayerUnitController>();
                        break;
                    case UnitAttackType.Area:
                        UnitController = gameObject.AddComponent<PlayerRangedSplashController>();
                        break;
                    case UnitAttackType.PierceArea:
                        UnitController = gameObject.AddComponent<PlayerMeleeSplashController>();
                        break;
                }
            }
            else
            {
                switch (UnitData.attackType)
                {
                    case UnitAttackType.Target:
                        UnitController = gameObject.AddComponent<PlayerHealerUnitController>();
                        break;
                    case UnitAttackType.Area:
                        UnitController = gameObject.AddComponent<PlayerHealerSplashController>();
                        break;
                }
                
            }
            
        }
    }
    protected override float GetStatBonus(StatType type)
    {
        return ArtifactManager.Instance.GetPassiveArtifactStatBonus(EffectTarget.MeleeUnit, type);
        // 이거 일단 임시로 Melee 유닛으로 만들어두긴 했는데 유닛을 어떻게 구분할 지에 대한 것도 생각해봐야 함
    }
}
