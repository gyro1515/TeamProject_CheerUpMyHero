using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class BuffEffect
{
    public IntegratedBuffType BuffType { get; protected set; }

    public BuffEffect(IntegratedBuffType buffType)
    {
        this.BuffType = buffType;
    }
    public abstract void ApplyEffect(BaseUnit unit);

    public abstract void ResetEffect(BaseUnit unit);
}
[System.Serializable]
public class BuffStatPercent : BuffEffect
{
    public float PercentValue { get; private set; }

    public BuffStatPercent(IntegratedBuffType buffType, float percentValue) : base(buffType)
    {
        this.PercentValue = percentValue;
    }
    public void ChagePercentValue(float newValue)
    {
        this.PercentValue = newValue;
    }
    public override void ApplyEffect(BaseUnit unit)
    {
        unit.SetBuffStatPercent(BuffType, PercentValue);
    }

    public override void ResetEffect(BaseUnit unit)
    {
        unit.SetBuffStatPercent(BuffType, -PercentValue);
    }
}
[System.Serializable]
public class BuffStat : BuffEffect
{
    public float StatValue { get; set; }

    public BuffStat(IntegratedBuffType type, float StatValue) : base(type)
    {
        this.StatValue = StatValue;
    }
    public void ChangeStatValue(float newValue)
    {
        this.StatValue = newValue;
    }
    public override void ApplyEffect(BaseUnit unit)
    {
        unit.SetBuffStatAdd(BuffType, StatValue);
    }
    public override void ResetEffect(BaseUnit unit)
    {
        unit.SetBuffStatAdd(BuffType, -StatValue);
    }
}
