using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveArtifactLevelData : ArtifactData
{
    public int level;
    public float coolTime;

    public float damageBonusPercent;        // 얼숨, 천둥
    
    public float attackBonusPercent;        // 왕군가
    public float attackSpeedBonusPercent;   // 왕군가
    
    public float healPercent;               // 여축
    
    public float summonHealth;              // 거석
    public float summonDuration;            // 거석
    public PoolType summonPoolType;         // 거석

    public int woodCost;
    public int goldCost;
    public int ironCost;
    public int magicStoneCost;
}
