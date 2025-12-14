using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerUnitArtifactBonus
{
    public float HpBonusPercent;
    public float AtkBonusPercent;
}

public struct PlayerArtifactBonus
{
    public float HpBonusPercent;
    public float AtkBonusPercent;
    public float MoveSpeedBonusPercent;
    public float AuraRangeBonusPercent;
}

public class ArtifactStatCalculator
{
    private readonly PlayerDataManager _data;

    private const float RangedUnitStandardCognizeRange = 2f;    // 원거리 유닛 판별 기준

    public ArtifactStatCalculator(PlayerDataManager data)       // 생성자 함수
    {
        _data = data;
    }

    #region 스탯 계산용 메서드
    // 패시브 유물 스탯 보너스 추출하는 메서드
    public float GetPassiveStatBonus(EffectTarget target, StatType statType)
    {
        if (_data.EquippedArtifacts == null) return 0f;

        float totalBonus = 0f;

        foreach (ArtifactData artifact in _data.EquippedArtifacts)
        {
            if (artifact is PassiveArtifactData passiveAf)
            {
                if (passiveAf.effectTarget == target && passiveAf.statType == statType)
                {
                    totalBonus += passiveAf.value;
                }
            }
        }
        return totalBonus;
    }

    // 패시브 유물 스탯 보너스 적용하기 좋은 형태로 가공함. 확률 형태로.
    public float GetPassiveStatBonusPercent(EffectTarget target, StatType statType)
    {
        return GetPassiveStatBonus(target, statType) / 100f;
    }

    // 패시브 유물 스탯 보너스 추출함. (특정 idNumber 사용하는 버전) 아직 잘 안 씀.
    public float GetPassiveArtifactValue(int idNumber)
    {
        if (DataManager.ArtifactData.TryGetValue(idNumber, out ArtifactData data))
        {
            if (data is PassiveArtifactData passiveArtifactData)
            {
                return passiveArtifactData.value;
            }
        }
        return 0f;
    }
    #endregion

    #region 타입 판별 + 스탯 보너스 구조체 헬퍼
    // 유닛 데이터 받아서 유닛 유형(원거리 or 근거리) 판별함
    public EffectTarget DetermineUnitEffectTarget(BaseUnitData unitData)
    {
        if (unitData == null) return EffectTarget.MeleeUnit;

        return unitData.cognizanceRange >= RangedUnitStandardCognizeRange
            ? EffectTarget.RangedUnit
            : EffectTarget.MeleeUnit;
    }

    // 구조체 활용해서 유물로 인한 플레이어 유닛 스탯별 보너스 계산함. 
    public PlayerUnitArtifactBonus GetUnitArtifactBonus(BaseUnitData unitData)
    {
        EffectTarget target = DetermineUnitEffectTarget(unitData);

        return new PlayerUnitArtifactBonus
        {
            HpBonusPercent = GetPassiveStatBonusPercent(target, StatType.MaxHp),
            AtkBonusPercent = GetPassiveStatBonusPercent(target, StatType.AtkPower)
        };
    }

    // 구조체 활용해서 유물로 인한 플레이어 스탯별 보너스 계산함. 
    public PlayerArtifactBonus GetPlayerArtifactBonus()
    {
        EffectTarget target = EffectTarget.Player;

        return new PlayerArtifactBonus
        {
            HpBonusPercent = GetPassiveStatBonusPercent(target, StatType.MaxHp),
            AtkBonusPercent = GetPassiveStatBonusPercent(target, StatType.AtkPower),
            MoveSpeedBonusPercent = GetPassiveStatBonusPercent(target, StatType.MoveSpeed),
            AuraRangeBonusPercent = GetPassiveStatBonusPercent(target, StatType.AuraRange)
        };
    }
    #endregion
}
