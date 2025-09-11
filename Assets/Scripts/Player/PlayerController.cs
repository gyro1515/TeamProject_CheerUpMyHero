using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseController
{
    Player player;
    protected override void Awake()
    {
        player = GetComponent<Player>();
        player.OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(player, true);
        };
        base.Awake();

    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!player) return;
        gameObject.transform.position += player.MoveDir * player.MoveSpeed * Time.fixedDeltaTime;
    }
}
