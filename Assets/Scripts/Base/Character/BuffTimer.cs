using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class BuffTimer
{
    public bool IsActive { get; protected set; }   // 켜져 있는가?
    public float Duration { get; protected set; }     // 남은 시간
    public BuffTimer()
    {
        IsActive = false;
        Duration = -1f;
    }
    public virtual void Reset()
    {
        IsActive = false;
        Duration = -1f;
    }
    public void UpdateBuffTimer(float deltaTime)
    {
        if (IsActive == false) return;

        Duration -= deltaTime;
        if (Duration <= 0f)
        {
            Reset();
        }
    }
}
[System.Serializable]
public class ActiveBuff : BuffTimer
{
    public List<BuffEffect> BuffEffects { get; private set; }
    private BaseUnit _target;
    public BuffSource Source { get; set; }  // 어떤 스킬인지

    public ActiveBuff() : base() { }
    
    public void ApplyActiveBuff(BaseUnit target,BuffSource source, List<BuffEffect> buffEffects, float duration)
    {
        Source = source;
        BuffEffects = buffEffects;
        IsActive = true;
        Duration = duration;
        _target = target;
        for (int i = 0; i < BuffEffects.Count; i++)
        {
            BuffEffects[i].ApplyEffect(_target);
        }
    }
    public override void Reset()
    {
        if (BuffEffects == null || _target == null) return;
        base.Reset();
        for (int i = 0; i < BuffEffects.Count; i++)
        {
            BuffEffects[i].ResetEffect(_target);
        }
        BuffEffects = null;
        _target = null;
    }
    public void RefreshActiveBuff(float duration)
    {
        // 남은 시간과 상관없이 다시 새 시간으로 덮어씀
        this.Duration = duration;
    }
    public void ExtendActiveBuff(float duration)
    {
        // 기존 시간에 더하기
        this.Duration += duration;
    }
}
[System.Serializable]
public class BuffColor : BuffTimer
{
    public Color changedColor { get; private set; }
    public BuffColorType Type { get; private set; }
    public BuffColor() : base()
    {
        changedColor = Color.white;
    }
    public void ApplyBuffColor(BuffColorType type, Color color, float duration)
    {
        this.Type = type;
        this.changedColor = color;
        this.IsActive = true;
        this.Duration = duration;
    }
    public void RefreshActiveBuff(float duration)
    {
        // 남은 시간이 더 짧으면 다시 새 시간으로 덮어씀
        if (this.Duration >= duration) return;

        this.Duration = duration;
    }
}
