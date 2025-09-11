using System;
using System.Collections;
using System.Collections.Generic;
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
    float curHp;
    public float CurHp { get { return curHp; }
        set
        {
            curHp = value;
            curHp = Mathf.Clamp(curHp, 0, MaxHp);
            OnCurHpChane?.Invoke(curHp, MaxHp);
            if(curHp <=0) OnDead?.Invoke();
            
        } }
    public event Action<float, float> OnCurHpChane;
    public event Action OnDead;

    protected virtual void Awake()
    {
        BaseController = GetComponent<BaseController>();
        curHp = MaxHp;
        Damageable = GetComponent<IDamageable>();
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
    /*public virtual void InitCharacter()
    {
        // 소환하면 위 아래로 움직일 일이 없으니까, 소환할때 sortingOrder 설정하기
        if (!characterSpriteRenderer) return;
        // y값이 낮을수록 sortingOrder가 커짐 → 앞에 그려짐
        characterSpriteRenderer.sortingOrder = 350 - (int)(gameObject.transform.position.y * 100);
        // UI초기화용
        //CurHp = curHp;
    }*/
}
