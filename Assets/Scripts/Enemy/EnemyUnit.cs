using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : BaseUnit
{
    public struct SpawnUnitEvent
    {
        public BaseUnit baseUnit;
        public bool isPlayer;
    }
    //float statMultiplier = 1f;
    protected override void Awake()
    {
        base.Awake();
        OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(this, false);
        };
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UnitManager.Instance.AddUnitList(this, false);
        //EventManager.Instance.Publish(new SpawnUnitEvent { baseUnit = this, isPlayer = false });
    }
    protected override void Start()
    {
        base.Start();

    }
    


}
