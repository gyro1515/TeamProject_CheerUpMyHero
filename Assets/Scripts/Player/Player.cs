using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseCharacter
{
    [field: Header("플레이어 세팅")]
    [field: SerializeField] public float MagicPower { get; private set; }
    [field: SerializeField] public float MaxMana { get; private set; }

    public Vector3 MoveDir { get; set; }

    protected override void Awake()
    {
        base.Awake();
        UIManager.Instance.GetUI<UIHpBarContainer>().AddHpBar(this, EUIHpBarType.Player);
        GameManager.Instance.Player = this;
    }
    protected override void FixedUpdate()
    {
        base.Update();
        // 테스트로 플레이어는 계속 정렬해주기
        InitCharacter();
    }
}
