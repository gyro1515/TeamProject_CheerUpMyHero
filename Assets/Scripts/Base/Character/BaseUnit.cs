using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


public abstract class BaseUnit : BaseCharacter
{
    [field: Header("유닛 세팅")]
    //[field: SerializeField] public float AttackRate { get; protected set; }
    [field: SerializeField] public float AttackRange { get; protected set; }
    [field: SerializeField] public float CognizanceRange { get; protected set; } // 인식 범위
    //[field: SerializeField] public int FoodConsumption { get; protected set; }
    //[field: SerializeField] public float AttackDelayTime { get; protected set; } = 1f; // 선딜
    [field: SerializeField] public float StartAttackTime { get; private set; } = 0.09f; // 애니메이션 기준 공격 시작 시간
    [field: SerializeField] public float StartAttackNormalizedTime { get; private set; } = 0.36f; // 애니메이션 기준 정규화된 공격 시작 시간
    [field: SerializeField] protected int HitBackCount { get; set; } = 3; // 최대 몇 번 히트백될 수 되는지
    [field: SerializeField] public float SpawnCooldown { get; set; } = 5f;
    [field: SerializeField] public virtual BaseUnitData UnitData { get; protected set; }

    // 퍼센트
    [SerializeField] protected float attackPowerMultiplier = 1f; //
    [SerializeField] protected float attackRateMultiplier = 1f;  //
    [SerializeField] protected float moveSpeedMultiplier = 1f;   // 
    // 고정 수치
    [SerializeField] protected float attackPowerAdd = 0;
    [SerializeField] protected float moveSpeedAdd = 0;

    public BaseUnitController UnitController { get; protected set; }

    [field: SerializeField] public UnitType UnitType { get; private set; } // 유닛 타입 ******* 참조된거 다 데이터 테이블에서 가져오도록 수정 필요 *******
    [field: SerializeField] public Rarity UnitRarity { get; private set; } // 유닛 등급

    public IDamageable TargetUnit { get; set; }
    // 데이터 용 변수, 데이터 테이블 완성시 테이블에서 가져오기 -> 251016 테이블에서 가져오도록 수정
    /*public float TmpAttackRange { get; protected set; }
    public float TmpCognizanceRange { get; protected set; }
    public float TmpAttackRate { get; protected set; }*/


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

    protected ArtifactStatCalculator _artifactStatCalculateor;      // 유물 스탯 계산용 일반 C# 클래스

    // 공격 애니메이션 관련 변수
    public bool IsAttackAnimPlaying { get; set; } = false;
    protected override void Awake()
    {
        base.Awake();
        /*TmpAttackRange = AttackRange;
        TmpCognizanceRange = CognizanceRange;
        TmpAttackRate = AttackRate;*/
        knockbackHandler = GetComponent<KnockbackHandler>();
        UnitController = GetComponent<BaseUnitController>();
        // 바인드 해제는 람다식으로 안됨, 그리고 굳이 해제를...?
        // 넉백과 유닛은 생성주기가 같기 때문에
        knockbackHandler.OnHitBackActive += SetHitBackActive; // 무적 여부 바인드

        gameObject.name = gameObject.name.Replace("(Clone)", ""); // 프리팹 이름으로 바꾸기

        _artifactStatCalculateor = new ArtifactStatCalculator(PlayerDataManager.Instance);

        SetDataFromExcelData();
        SetStatMultiplier(1f);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        
        SetStatMultiplier(1f);
        TargetUnit = null;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        //SetStatMultiplier(1f); // 몬스터 비활성화시 초기화
        TargetUnit = null;
    }
    protected void InitFinStats()
    {
        // 수치는 다 버프 컨트롤러 쪽에서 다 원상복구하지만, 다시 활성화될 때마다 예방으로 초기화
        attackPowerMultiplier = 1f;
        attackRateMultiplier = 1f;
        moveSpeedMultiplier = 1f;
        attackPowerAdd = 0f;
        moveSpeedAdd = 0f;
        FinAttackPower = AtkPower * attackPowerMultiplier + attackPowerAdd;
        FinAttackRate = AttackRate * attackRateMultiplier;
        FinMoveSpeed = MoveSpeed * moveSpeedMultiplier + moveSpeedAdd;
    }
    public void SetBuffStatPercent(IntegratedBuffType buffType, float percentValue)
    {
        // 버프 타입에 따라 스탯
        switch (buffType)
        {
            case IntegratedBuffType.AttackPower:
                attackPowerMultiplier += percentValue;
                FinAttackPower = AtkPower * attackPowerMultiplier + attackPowerAdd;
                break;
            case IntegratedBuffType.AttackRate:
                attackRateMultiplier += percentValue;
                FinAttackRate = AttackRate * attackRateMultiplier;
                break;
            case IntegratedBuffType.MoveSpeed:
                moveSpeedMultiplier += percentValue;
                FinMoveSpeed = MoveSpeed * moveSpeedMultiplier + moveSpeedAdd;
                break;
        }
    }
    public void SetBuffStatAdd(IntegratedBuffType buffType, float addValue)
    {
        // 버프 타입에 따라 스탯
        switch (buffType)
        {
            case IntegratedBuffType.AttackPower:
                attackPowerAdd += addValue;
                FinAttackPower = AtkPower * attackPowerMultiplier + attackPowerAdd;
                break;
            case IntegratedBuffType.AttackRate:
                FinAttackRate = AttackRate * attackRateMultiplier;
                break;
            case IntegratedBuffType.MoveSpeed:
                moveSpeedAdd += addValue;
                FinMoveSpeed = MoveSpeed * moveSpeedMultiplier + moveSpeedAdd;
                break;
        }
    }
    /*public void SetMoveSpeed(float newSpeed)
    {
        MoveSpeed = newSpeed;
    }

    public void SetAttackPower(float newAtkPower)
    {
        AtkPower = newAtkPower;
    }

    public void SetAttackRate(float newAttackRate)
    {
        AttackRate = newAttackRate;
    }*/
    protected abstract void SetDataFromExcelData();
   
    protected virtual void SetHitBackActive(bool active)
    {
        IsInvincible = active;
    }
    public void StartHitBack()
    {
        if (IsInvincible) return; // 이미 히트백이라면 리턴
        OnHitBack?.Invoke();
    }
    public abstract void SetStatMultiplier(float statMultiplier, bool isSpawnHero = false);
    
    // 일단 기본값으로 베이스 유닛은 플레이어 유닛으로 취급하게 만들어둠.
    // 운명, 도전 등 기능 넣으려면 EffectTarget을 반환해야 해서 Unit이나 플레이어 등에 EffectTarget 반환하는 메서드 넣었어요
    protected virtual EffectTarget GetEffectTarget()
    {
        return EffectTarget.PlayerUnit;
    }
}
