using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageDestinyData : StageModifierData
{
    public DestinyType destinyType;
    public string description;
    public List<StageDestinyModifier> modifiers;
    public string iconSpritePath;
}
