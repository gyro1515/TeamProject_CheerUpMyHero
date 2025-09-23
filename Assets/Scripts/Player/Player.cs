using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseCharacter
{

    [field: Header("플레이어 세팅")]
    [field: SerializeField] public float MagicPower { get; private set; }
    [field: SerializeField] public float MaxMana { get; private set; }

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

    protected override void Awake()
    {
        base.Awake();
        GameManager.Instance.Player = this;
        UnitManager.Instance.AddUnitList(this, true);
        OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(this, true);
        };
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
