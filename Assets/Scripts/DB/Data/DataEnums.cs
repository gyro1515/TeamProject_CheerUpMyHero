// 데이터 참조 태그 : 유물, 운명&도전
public enum EffectTarget            // 효과의 대상 개체 지정하는 enum
{
    None,
    Player,
    PlayerUnit,
    MeleeUnit,
    KnightUnit,
    RangedUnit,
    Hero,
    EnemyUnit,
    SameNation,
    DifferentNation,
    System
}

// 데이터 참조 태그 : 유물, 운명&도전
public enum StatType                // 효과의 대상 스탯 지정하는 enum
{
    None,
    MaxHp,
    AtkPower,
    MoveSpeed,
    AuraRange,
    MaxSpawnCount,
    SpawnCost,
    SpawnCooldown,
    Timer
}

// 데이터 참조 태그 : 운명&도전
public enum ValueModificationType   // 효과의 값 계산 방식 지정하는 enum
{
    None,
    Percentage,
    Set,
    Absolute
}

// 데이터 참조 태그 : 운명&도전
public enum ValueConditionOperater  // 효과에 특수 조건이 붙어있거나 할 때 비교 연산자 형태로 조건 지정하는 enum
{
    None,
    Equals,             // ==
    NotEquals,          // !=
    Greater,            // >
    Less,               // <
    GreaterThanOrEqual,  // >=
    LessThanOrEqual     // <=
}

#region 유물 enum
// 데이터 참조 태그 : 유물
public enum ArtifactType
{
    None,
    Active,
    Passive
}

// 데이터 참조 태그 : 유물
public enum PassiveArtifactGrade
{
    Common,
    Rare,
    Epic,
    Unique,
    Legendary
}
#endregion

#region 운명&도전 enum
// 데이터 참조 태그 : 운명&도전
public enum DestinyType
{
    None,
    Fortune,
    Misfortune
}

public enum ConditionType
{
    None,
    SameNationCount,
    IsDifferentNation
}

public enum ModifierSpecialEffect
{
    None,
    DisableDeckSlot,
    DisableTerritory
}
#endregion