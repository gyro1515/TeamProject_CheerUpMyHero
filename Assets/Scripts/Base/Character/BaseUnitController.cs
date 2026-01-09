using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

public abstract class BaseUnitController : BaseController
{
    BaseUnit baseUnit;
    protected KnockbackHandler knockbackHandler;

    bool isPlayer;
    IEventPublisher<HeroUnitDeadEvent> heroUnitDeadEventPub;
    protected float attackTimer = 0f;

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
        heroUnitDeadEventPub = EventManager.GetPublisher<HeroUnitDeadEvent>();

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
    protected override void Update()
    {
        base.Update();
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
    public override void SetDead()
    {
        base.SetDead();
        //251127: 영웅 유닛 사망시 스폰 쿨타임 75% 반환하기
        if((baseUnit.UnitData.synergyType & UnitSynergyType.Hero) == UnitSynergyType.Hero)
        {
            heroUnitDeadEventPub.Publish(new HeroUnitDeadEvent(baseUnit.UnitData.poolType));
        }
    }
    protected abstract void HitBackActive(bool active);

    protected virtual void PlayUnitAttackEffectSound()
    {
        if (isPlayer)
        {
            if (baseUnit.CognizanceRange < 2f)
            {
                AudioManager.PlayRandomOneShotByCameraDistance(DataManager.AudioData.meleeUnitAttackSE, gameObject.transform, 0.3f);
            }
            else
            {
                if ((baseUnit.UnitData.synergyType & UnitSynergyType.Archer) != 0)
                    AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.archerUnitAttackSE, gameObject.transform, 0.5f);
                else if ((baseUnit.UnitData.synergyType & UnitSynergyType.Mage) != 0)
                    AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.magicUnitAttackSE, gameObject.transform, 0.5f);
                else
                    AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.archerUnitAttackSE, gameObject.transform, 0.5f);
            }

            if ((baseUnit.UnitData.synergyType & UnitSynergyType.Burn) != 0)
            {
                AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.synergy_fireSE, gameObject.transform);
            }

            if ((baseUnit.UnitData.synergyType & UnitSynergyType.Frost) != 0)
            {
                AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.synergy_iceSE, gameObject.transform);
            }

            if ((baseUnit.UnitData.synergyType & UnitSynergyType.Poison) != 0)
            {
                AudioManager.PlayOneShotByCameraDistance(DataManager.AudioData.synergy_poisonSE, gameObject.transform);
            }
        }
    }
}
// 영웅 유닛 사망 이벤트
public struct HeroUnitDeadEvent
{
    public PoolType poolType;
    public HeroUnitDeadEvent(PoolType poolType)
    {
        this.poolType = poolType;
    }
}
