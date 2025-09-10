using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnitController : BaseController
{
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        gameObject.transform.position += Vector3.left * baseCharacter.MoveSpeed * Time.fixedDeltaTime;
    }
}
