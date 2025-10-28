using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SynergyData : MonoData
{
    public UnitSynergyType synergyType;
    public SynergyGrade synergyGrade;
    public string effectDescription;
    public int requiredUnitCount;
    public string synergyTypeText;
    public string synergyGradeText;
}
public enum SynergyGrade
{
    Bronze,
    Gold,
    Prism
}
