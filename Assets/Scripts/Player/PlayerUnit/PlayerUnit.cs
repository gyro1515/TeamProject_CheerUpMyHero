using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerUnit : BaseUnit
{
    private float _beforeAuraAtkBonus;
    private bool _hasBuff;

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

    // SetStatMultiplier 
    protected override EffectTarget GetEffectTarget()
    {
        return EffectTarget.PlayerUnit;
    }
    public override void SetStatMultiplier(float statMultiplier, bool isSpawnHero = false)
    {
        if (UnitData == null) { Debug.LogError("데이터 없음"); return; }
        if (gameObject.IsDestroyed() || gameObject == null) 
        {
            Debug.LogWarning($"왜 파괴됐을까?"); 
            return; // 비활성화/파괴된 상태라면 리턴
        } 

        float synergyHealthBonus = PlayerDataManager.Instance.SynergyAllUnitHealthBonus;
        float synergyAttackBonus = PlayerDataManager.Instance.SynergyAllUnitAttackBonus;
        float synergyAttackCooldownReduction = PlayerDataManager.Instance.SynergyUnitAttackCooldownReduction;

        // 운명 및 도전 배율 변수. 코스트 쿨타임 관련 로직은 UnitSlotUI 찾아보기
        EffectTarget target = GetEffectTarget();
        float hpModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.MaxHp, this.UnitData);
        float atkPowerModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.AtkPower, this.UnitData);
        float moveSpeedModifierBonus = Modifiercalculator.GetMultiplier(target, StatType.MoveSpeed, this.UnitData);

        // 아티팩트 스탯 보너스 관련 로직 -> cognizeRange 2 이상이면 Ranged, 아니면 Melee 반환함
        EffectTarget artifactTarget = (UnitData != null && UnitData.cognizanceRange >= 2f) ? EffectTarget.RangedUnit : EffectTarget.MeleeUnit;
        float hpArtifactBonusPercent = ArtifactManager.Instance.GetPassiveArtifactStatBonus(artifactTarget, StatType.MaxHp) / 100f;
        float atkArtifactBonusPercent = ArtifactManager.Instance.GetPassiveArtifactStatBonus(artifactTarget, StatType.AtkPower) / 100f;

        // 영웅 소환시, 소환될 유닛은 스탯 2배
        float spawnHeroBonus = isSpawnHero && UnitData.unitClass == UnitClass.Normal ? 2f : 1f;

        // 배율에 따른 체력 공격력 세팅 -> [원래 값 * (운명, 도전 배율 + statMultiplier) -> 합연산]
        MaxHp = UnitData.health * (hpModifierBonus + statMultiplier + hpArtifactBonusPercent) * (1.0f + synergyHealthBonus / 100.0f) * spawnHeroBonus;
        curHp = MaxHp;
        AtkPower = UnitData.atkPower * (atkPowerModifierBonus + statMultiplier + atkArtifactBonusPercent) * (1.0f + synergyAttackBonus / 100.0f) * spawnHeroBonus;
        MoveSpeed = UnitData.moveSpeed * (moveSpeedModifierBonus + 1f);

        // 이 시너지 체크 필요
        AttackRate = UnitData.attackRate / statMultiplier * (1.0f - synergyAttackCooldownReduction / 100.0f) / spawnHeroBonus; // 공격 속도는 시너지  배율에 비례
        // ********* 공격 속도는 스탯과 비례하지 않죠?? ************
        //AttackRate = UnitData.attackRate * (1.0f - synergyAttackCooldownReduction / 100.0f); // 공격 속도는 시너지  배율에 비례

        float tmpstatMultiplier = Math.Clamp(statMultiplier * spawnHeroBonus, 0.8f, 1.2f); // 크기는 너무 작아지거나 커지지 않도록 제한
        // 아래는 다 tmpstatMultiplier로 세팅, 크기에 따라 인식/공격 범위도 달라지도록
        gameObject.transform.localScale = TmpSize * tmpstatMultiplier;
        AttackRange = UnitData.attackRange * tmpstatMultiplier;
        CognizanceRange = UnitData.cognizanceRange * tmpstatMultiplier;

        // 현재 캡슐 사용 x
        /*CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        // 사이즈는 달라질 수 있으니 활성화 시마다 갱신
        knockbackHandler.Init(col.size.x * statMultiplier);*/
        // 그저 유닛 크기에 비례하도록
        knockbackHandler.Init((TmpSize * tmpstatMultiplier).x);
        // ex: 최대 체력 = 300 / HitBackCount = 3 => 데미지 100이 누적될때마다 히트백
        hitbackHp = MaxHp / UnitData.hitBack;
        // ex: curHp / hitbackHp  => 2 -> 1 -> 0에서만 히트백이 발생하도록
        hitbackTriggerCount = UnitData.hitBack - 1;

        _beforeAuraAtkBonus = AtkPower;
    }
    protected override void SetDataFromExcelData()
    {
        if (!Enum.TryParse(gameObject.name, out PoolType poolType))
        { Debug.LogError($"변환 실패: {gameObject.name} 은(는) PoolType에 없습니다."); return; }

        UnitData = DataManager.PlayerUnitData.GetData((int)poolType);
        // 컨트롤러 자동추가 테스트 -> 251017: 테스트 완료, 이제 유닛마다 컨트롤러 수동 추가 안해도 됨
        //if (UnitController == null) // 컨트롤러 없다면
        if (UnitData.unitType != UnitType.Healer) // 힐러는 따로
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
                case UnitAttackType.PierceArea:
                    UnitController = gameObject.AddComponent<PlayerHealerSplashController>();
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
   
    public void SetForRenderTexture()
    {
        // 렌더 텍스처용 세팅
        UnitManager.Instance.RemoveUnitFromList(this, true);
        UnitController.enabled = false;
        enabled = false;
        MoveDir = Vector3.zero;

    }

    public void ApplyAuraBuff(float bonusPer)
    {
        if (_hasBuff) return;

        _beforeAuraAtkBonus = AtkPower;

        AtkPower *= 1f + bonusPer / 100f;

        _hasBuff = true;
    }

    public void RemoveAuraBuff()
    {
        if (!_hasBuff) return;

        AtkPower = _beforeAuraAtkBonus;

        _hasBuff = false;
    }
}
