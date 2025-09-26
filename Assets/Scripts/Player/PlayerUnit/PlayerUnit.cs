using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : BaseUnit
{
    [field: Header("플레이어 유닛 세팅")]
    [field: SerializeField] public float SpawnCooldown { get; set; } = 5f; 
    protected override void Awake()
    {
        base.Awake();
        OnDead += () =>
        {
            UnitManager.Instance.RemoveUnitFromList(this, true);
        };

    }
    protected override void Start()
    {
        base.Start();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        UnitManager.Instance.AddUnitList(this, true);
        
    }

    protected override void ApplyArtifactStat()
    {
        EffectTarget type = EffectTarget.MeleeUnit;

        Dictionary<StatType, float> unitBonus = PlayerDataManager.Instance.CalculateArtifactTotalBonusStat(type);
        UpdateBonusStat(unitBonus);
    }
}
