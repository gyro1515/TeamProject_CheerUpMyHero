using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseCharacter
{

    [field: Header("플레이어 세팅")]
    [field: SerializeField] public float MagicPower { get; private set; }
    [field: SerializeField] public float MaxMana { get; private set; }
    public float CurMana { get; set; }
    //프로퍼티도 버추얼 오버라이드가 되네요??
    public override Vector3 MoveDir
    {
        get { return base.MoveDir; } 
        set
        {
            if (base.MoveDir == value) return;

            base.MoveDir = value;
            OnMoveDirChanged?.Invoke(base.MoveDir);
        }
    }

    public event Action<Vector3> OnMoveDirChanged;
    public PlayerController PlayerController { get; private set; }
    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.Player = this;
        UnitManager.Instance.AddUnitList(this, true);
        OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(this, true);
        };
        //GameManager에게 전투 시작 준비를 명령
        GameManager.Instance.StartBattle(); //배틀씬으로 갔을 때부터 식량 획득 증가 함수
        CurMana = MaxMana;
        PlayerController = GetComponent<PlayerController>();
    }
    protected override void Start()
    {
        base.Start();
    }
    protected override void FixedUpdate()
    {
        base.Update();
        // 테스트로 플레이어는 계속 정렬해주기
        //InitCharacter();
    }
    
}
