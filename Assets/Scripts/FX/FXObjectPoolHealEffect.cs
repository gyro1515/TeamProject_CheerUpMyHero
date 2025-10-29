using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXObjectPoolHealEffect : FXObjectPool
{
    public override void ReleaseSelf()
    {
        transform.SetParent(ObjectPoolManager.Instance.poolTransformsDic[PoolType.FXHealEffect]);
        base.ReleaseSelf();
    }
}
