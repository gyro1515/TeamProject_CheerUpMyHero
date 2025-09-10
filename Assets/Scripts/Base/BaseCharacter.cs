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
    // 랜더 순서용
    [field: SerializeField] public SpriteRenderer characterSpriteRenderer { get; private set; }
    [field: SerializeField] public Vector3 HpBarPosByCharacter { get; private set; } // 보정용

    float curHp;

    public float CurHp { get { return curHp; }
        set
        {
            curHp = value;
            OnCurHpChane?.Invoke(curHp, MaxHp);
            if(curHp <=0) OnDead?.Invoke();
            
        } }
    public event Action<float, float> OnCurHpChane;
    public event Action OnDead;

    protected virtual void Awake()
    {
        curHp = MaxHp;
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
    public void InitCharacter()
    {
        // 소환하면 위 아래로 움직일 일이 없으니까, 소환할때 sortingOrder 설정하기
        if (!characterSpriteRenderer) return;
        // y값이 낮을수록 sortingOrder가 커짐 → 앞에 그려짐
        characterSpriteRenderer.sortingOrder = 350 - (int)(gameObject.transform.position.y * 100);
        // UI초기화용
        //CurHp = curHp;
    }
}
