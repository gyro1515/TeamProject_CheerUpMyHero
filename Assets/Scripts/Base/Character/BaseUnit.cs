using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BaseUnit : BaseCharacter
{
    [field: Header("유닛 세팅")]
    [field: SerializeField] public float AttackRate { get; private set; }
    [field: SerializeField] public float AttackRange { get; private set; }
    [field: SerializeField] public int FoodConsumption { get; private set; }
    [field: SerializeField] public float AttackDelayTime { get; private set; } = 1f; // 선딜
    [field: SerializeField] public float StartAttackTime { get; private set; } = 0.09f; // 애니메이션 기준 공격 시작 시간
    [field: SerializeField] public float StartAttackNormalizedTime { get; private set; } = 0.36f; // 애니메이션 기준 정규화된 공격 시작 시간
    [field: SerializeField] protected int HitBackCount { get; set; } = 3; // 최대 몇 번 히트백될 수 되는지
    public BaseUnitController UnitController { get; private set; } 
    public IDamageable TargetUnit { get; set; }
    public bool IsInvincible { get; private set; } = false; // 무적 여부
    protected float hitbackHp = -1f; // 이 이상 데미지가 누적되면 히트백
    protected int hitbackTriggerCount = 0;
    public override float CurHp
    {
        get => base.CurHp;
        set
        {
            base.CurHp = value;
            if (curHp == MaxHp) return; // 최대 체력이라면 아래 실행x
            float curRatio = curHp / hitbackHp;
            // 현재 체력 비율이 히트백 트리거 지점보다 작을때만 히트백
            //Debug.Log($"{curHp} / {hitbackHp} = {curRatio}");
            if(curRatio <= hitbackTriggerCount)
            {
                // hitbackTriggerCount 갱신, 현재 체력 비율에서 소수점 버리기
                hitbackTriggerCount = (int)curRatio; // 0.1f- > 0, 1.1f -> 1
                OnHitBack?.Invoke();
            }
            else // 아니면 넉백
            {
                OnKnockBack?.Invoke(); 
            }
        }
    }
    public event Action OnHitBack;
    public event Action OnKnockBack;

    protected KnockbackHandler knockbackHandler;
    protected override void Awake()
    {
        base.Awake();
        knockbackHandler = GetComponent<KnockbackHandler>();
        UnitController = GetComponent<BaseUnitController>();
        // 바인드 해제는 람다식으로 안됨, 그리고 굳이 해제를...?
        // 넉백과 유닛은 생성주기가 같기 때문에
        knockbackHandler.OnHitBackActive += SetHitBackActive; // 무적 여부 바인드

    }
    protected override void OnEnable()
    {
        base.OnEnable();
        TargetUnit = null;
        
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        // 사이즈는 달라질 수 있으니 활성화 시마다 갱신
        knockbackHandler.Init(col.size.x);
        // ex: 최대 체력 = 300 / HitBackCount = 3 => 데미지 100이 누적될때마다 히트백
        hitbackHp = MaxHp / HitBackCount;
        // ex: curHp / hitbackHp  => 2 -> 1 -> 0에서만 히트백이 발생하도록
        hitbackTriggerCount = HitBackCount - 1;

    }
    protected override void OnDisable()
    {
        base.OnDisable();
        TargetUnit = null;
    }
    protected virtual void SetHitBackActive(bool active)
    {
        IsInvincible = active;
    }
}
