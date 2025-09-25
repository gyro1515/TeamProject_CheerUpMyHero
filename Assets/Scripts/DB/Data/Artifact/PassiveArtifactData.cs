using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PassiveArtifactData : ArtifactData
{
    public EffectTarget effectTarget;
    public StatType statType;
    public PassiveArtifactGrade grade;
    public float value;

    /*[NonSerialized]
    public float[] effectValuesByGrade = new float[5];

    public void ArtifactGradeProcess()
    {
        effectValuesByGrade[0] = commonValue;
        effectValuesByGrade[1] = rareValue;
        effectValuesByGrade[2] = epicValue;
        effectValuesByGrade[3] = uniqueValue;
        effectValuesByGrade[4] = legendaryValue;
    }*/
}
