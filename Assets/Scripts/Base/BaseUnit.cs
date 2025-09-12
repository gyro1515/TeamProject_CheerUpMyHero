using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUnit : BaseCharacter
{
    [field: Header("유닛 세팅")]
    [field: SerializeField] public float AttackRate { get; private set; }
    [field: SerializeField] public float AttackRange { get; private set; }
    [field: SerializeField] public int foodConsumption { get; private set; }
    //public BaseCharacter TargetUnit { get; set; }
    public IDamageable TargetUnit { get; set; }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        TargetUnit = null;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        TargetUnit = null;
    }
}
