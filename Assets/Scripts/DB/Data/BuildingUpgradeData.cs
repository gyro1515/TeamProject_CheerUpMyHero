using System.Collections.Generic;

public enum BuildingEffectType
{
    None,
    IncreaseMaxFood,
    IncreaseFoodGainSpeed,
    BaseWoodProduction,
    AdditionalWoodProduction,
    BaseIronProduction,
    AdditionalIronProduction,
    MagicStoneFindChance,
    MagicStoneProduction,
    MaxPopulation,
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

    public List<Cost> costs = new List<Cost>();
    public List<BuildingEffect> effects = new List<BuildingEffect>();

    public string description;
}