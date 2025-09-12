using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{
    [field: Header("기본 캐릭터 세팅")]
    [field: SerializeField] public float MaxHp {  get; private set; }
    [field: SerializeField] public float AtkPower {  get; private set; }
    [field: SerializeField] public float MoveSpeed {  get; private set; }
    [field: SerializeField] public Vector3 HpBarPosByCharacter { get; private set; } // 보정용
    [field: SerializeField] public Vector2 HpBarSize { get; private set; } // 체력바 사이즈용
    public BaseController BaseController { get; private set; }
    public Vector3 MoveDir { get; set; }
    public IDamageable Damageable { get; private set; }
    [SerializeField] float curHp;
    public float CurHp { get { return curHp; }
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
    protected virtual void Awake()
    {
        BaseController = GetComponent<BaseController>();
        Damageable = GetComponent<IDamageable>();
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
