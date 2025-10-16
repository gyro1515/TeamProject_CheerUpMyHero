using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageDestinyModifier : StageModifierData
{
    public EffectTarget effectTarget;
    public StatType statType;
    public ValueModificationType valueModificationType;
    public float value;
    public ConditionType conditionType;
    public ValueConditionOperater valueConditionOperater;
    public float conditionValue;
}
