public enum ArtifactType
{
    Active,
    Passive
}

public enum EffectTarget            // 어떤 유닛에게 영향 미치는가 -> 유물 슬롯에 영향 미침.
{
    Player,
    MeleeUnit,
    RangedUnit
}

public enum StatType                // 패시브 유물 스탯 타입
{
    PlayerAttackPower,
    PlayerHealth,
    PlayerMoveSpeed,
    PlayerAuraRange,
    MeleeUnitAttackPower,
    MeleeUnitHealth,
    RangedUnitAttackPower,
    RangedUnitHealth
}

public enum PassiveArtifactGrade    // 유물 등급을 들고 있으려면 필요함
{
    Common,
    Rare,
    Epic,
    Unique,
    Legendary
}