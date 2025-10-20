using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageChallengeData : StageModifierData
{
    public EffectTarget effectTarget;
    public StatType statType;
    public ValueModificationType valueModificationType;
    public int maxLevel;
    public float valuePerLevel;
    public int pointPerLevel;
    public ModifierSpecialEffect modifierSpecialEffect;
}
