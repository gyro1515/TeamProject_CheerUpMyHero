using System.Collections.Generic;
using UnityEngine;


public enum BuildingType
{

    None,
    Farm,
    LumberMill,
    Mine,
    Barracks,
}
public enum BuildingEffectType
{
    None,
    MaximumFood,
    IncreaseFoodGainSpeed,
    BaseWoodProduction,
    AdditionalWoodProduction,
    BaseIronProduction,
    AdditionalIronProduction,
    MagicStoneFindChance,
    MagicStoneProduction,
    UnitCoolDown,
    CanSummonRareUnits,
    CanSummonEpicUnits
}

[System.Serializable]
public class Cost
{
    public ResourceType resourceType;
    public int amount;
}

[System.Serializable]
public class BuildingEffect
{
    public BuildingEffectType effectType;
    public float effectValueMin;
    public float effectValueMax;
}

[System.Serializable]
public class BuildingUpgradeData : MonoData
{
    public string buildingName;
    public int level;
    public int nextLevel;
    public Sprite buildingSprite; //이미지

    public BuildingType buildingType = BuildingType.None;

    public List<Cost> costs = new List<Cost>();
    public List<BuildingEffect> effects = new List<BuildingEffect>();

    public string description;
}
