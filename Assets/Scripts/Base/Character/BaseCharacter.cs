using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    [Header("*데이터 idNumber설정*")]
    [SerializeField] public int idNumber; 

    [field: Header("기본 캐릭터 세팅")]
    [field: SerializeField] public float MaxHp {  get; protected set; }
    [field: SerializeField] public float AtkPower {  get; protected set; }
    [field: SerializeField] public float MoveSpeed {  get; protected set; }
    [field: SerializeField] public float AttackRate { get; protected set; }
    [field: SerializeField] public Vector3 HpBarPosByCharacter { get; private set; } // 보정용
    [field: SerializeField] public Vector2 HpBarSize { get; private set; } // 체력바 사이즈용
    public BaseController BaseController { get; protected set; }
    
    // 데이터 용 변수, 데이터 테이블 완성시 테이블에서 가져오기  -> 251016 테이블에서 가져오도록 수정
    /*public float TmpMaxHp { get; protected set; }
    public float TmpAtkPower { get; protected set; }*/
    public Vector3 TmpSize { get; protected set; }

    protected Vector3 _moveDir;

    public void SetMoveSpeed(float newSpeed)
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
    }
    public virtual Vector3 MoveDir
    {
        get { return _moveDir; }
        set { _moveDir = value; }
    }
    public IDamageable Damageable { get; protected set; }
    [SerializeField] protected float curHp;
    public virtual float CurHp { get { return curHp; }
        set
        {
            curHp = value;
            curHp = Mathf.Clamp(curHp, 0, MaxHp);
            OnCurHpChane?.Invoke(curHp, MaxHp);
            if(curHp <=0) OnDead?.Invoke();
        } }
    public bool IsDead { get; set; }
    public event Action<float, float> OnCurHpChane;
    public event Action OnDead;
    [field: SerializeField] public int ListIndex { get; set; } = -1; // 자기 리스트 내 인덱스, UnitManager판별용

    public AnimationData AnimationData { get; private set; }
    protected virtual void Awake()
    {
        /*TmpMaxHp = MaxHp;
        TmpAtkPower = AtkPower;*/
        TmpSize = gameObject.transform.localScale;

        BaseController = GetComponent<BaseController>();
        Damageable = GetComponent<IDamageable>();
        AnimationData = AnimationData.Instance;
    }
    protected virtual void OnEnable()
    {
        // 다시 활성화 됐을때
        curHp = MaxHp;
        IsDead = false;
    }
    protected virtual void Start()
    {
        // UI 체력바 초기화용
        CurHp = curHp;
    }
    protected virtual void Update()
    {
        
    }
    protected virtual void FixedUpdate()
    {

    }
    protected virtual void OnDisable()
    {
        
    }
}
