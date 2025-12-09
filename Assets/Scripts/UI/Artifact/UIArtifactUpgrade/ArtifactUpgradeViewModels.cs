using UnityEngine;

public struct PassiveSlotViewModel
{
    public PassiveArtifactData Artifact;
    public int OwnedCount;
    public bool CanUpgrade;
    public bool IsSelectable;
    public Sprite Icon;
    public Color BorderColor;
}

public struct PassiveMaterialSlotViewModel
{
    public bool IsFilled;
    public Sprite Icon;
    public Color BorderColor;
}

public struct PassivePreviewViewModel
{
    public PassiveArtifactData SourceArtifact;
    public PassiveArtifactData ResultArtifact;
    public Sprite SourceIcon;
    public Sprite ResultIcon;
    public Color SourceBorderColor;
    public Color ResultBorderColor;
    public string SourceEffectText;
    public string ResultEffectText;
}

public struct ActiveSlotViewModel
{
    public ActiveArtifactData Artifact;
    public Sprite Icon;
    public string NameText;
    public string LevelText;
}

public struct ActiveUpgradeViewModel
{
    public ActiveArtifactData Artifact;
    public bool CanUpgrade;
    public bool IsMaxLevel;
    public Sprite Icon;
    public string CurrentLevelText;
    public string NextLevelText;
    public string CurrentEffectText;
    public string NextEffectText;
    public string GoldCostText;          
    public string WoodCostText;          
    public string IronCostText;          
    public string MagicStoneCostText;
    public bool HasEnoughGold;
    public bool HasEnoughWood;
    public bool HasEnoughIron;
    public bool HasEnoughMagicStone;
}