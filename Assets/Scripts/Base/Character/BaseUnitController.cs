using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUnitController : BaseController
{
    BaseUnit baseUnit;
    protected KnockbackHandler knockbackHandler;

    protected override void Awake()
    {
        base.Awake();
        baseUnit = GetComponent<BaseUnit>();
        baseUnit.OnHitBack += () => 
        { if (animator) animator.SetTrigger(baseUnit.AnimationData.HitBackParameterHash); };
        knockbackHandler = GetComponent<KnockbackHandler>();
        knockbackHandler.OnHitBackActive += HitBackActive; // 히트백 시, 컨트롤러에서 해야할 일 바인드
    }
    public override void TakeDamage(float damage)
    {
        if (baseUnit.IsInvincible) return; // 무적이라면 리턴
        base.TakeDamage(damage);
    }
    protected abstract void HitBackActive(bool active);
    
}
