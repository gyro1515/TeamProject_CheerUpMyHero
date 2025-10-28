using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

public abstract class BaseUnitController : BaseController
{
    BaseUnit baseUnit;
    protected KnockbackHandler knockbackHandler;

    bool isPlayer;

    protected override void Awake()
    {
        base.Awake();
        baseUnit = GetComponent<BaseUnit>();
        baseUnit.OnHitBack += () => 
        { if (animator) animator.SetTrigger(baseUnit.AnimationData.HitBackParameterHash); };
        knockbackHandler = GetComponent<KnockbackHandler>();
        knockbackHandler.OnHitBackActive += HitBackActive; // 히트백 시, 컨트롤러에서 해야할 일 바인드

        if (baseUnit is PlayerUnit || baseUnit is Player)
        {
            isPlayer = true;
        }    
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

    public override void Attack()
    {
        base.Attack();

        // 공격할 때 효과음 유닛 종류별로 넣어줌
        PlayUnitAttackEffectSound();
    }

    public override void TakeDamage(float damage)
    {
        if (baseUnit.IsInvincible) return; // 무적이라면 리턴
        base.TakeDamage(damage);
    }
    protected abstract void HitBackActive(bool active);

    protected virtual void PlayUnitAttackEffectSound()
    {
        if (isPlayer)
        {
            if (baseUnit.CognizanceRange < 2f)
            {
                AudioManager.PlayRandomOneShot(DataManager.AudioData.meleeUnitAttackSE);
            }
            else
            {
                if ((baseUnit.UnitData.synergyType & UnitSynergyType.Archer) != 0)
                    AudioManager.PlayOneShot(DataManager.AudioData.archerUnitAttackSE);
                else if ((baseUnit.UnitData.synergyType & UnitSynergyType.Mage) != 0)
                    AudioManager.PlayOneShot(DataManager.AudioData.magicUnitAttackSE);
                else
                    AudioManager.PlayOneShot(DataManager.AudioData.archerUnitAttackSE);
            }

            if ((baseUnit.UnitData.synergyType & UnitSynergyType.Burn) != 0)
            {
                AudioManager.PlayOneShot(DataManager.AudioData.synergy_fireSE);
            }

            if ((baseUnit.UnitData.synergyType & UnitSynergyType.Frost) != 0)
            {
                AudioManager.PlayOneShot(DataManager.AudioData.synergy_iceSE);
            }

            if ((baseUnit.UnitData.synergyType & UnitSynergyType.Poison) != 0)
            {
                AudioManager.PlayOneShot(DataManager.AudioData.synergy_poisonSE);
            }
        }
    }
}
