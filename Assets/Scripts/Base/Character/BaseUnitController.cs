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
    protected override void OnEnable()
    {
        base.OnEnable();
        if (Animator)
        {
            //Debug.Log("애님 리셋");
            Animator.Rebind();
            Animator.Update(0f);
            Animator.ResetTrigger(baseCharacter.AnimationData.AttackParameterHash);
            Animator.Play(baseCharacter.AnimationData.BasicParameterHash, 0, 0f);
        }
    }
    public override void TakeDamage(float damage)
    {
        if (baseUnit.IsInvincible) return; // 무적이라면 리턴
        base.TakeDamage(damage);
    }
    protected abstract void HitBackActive(bool active);


}
