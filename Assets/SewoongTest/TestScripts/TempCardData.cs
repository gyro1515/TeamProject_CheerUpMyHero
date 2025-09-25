using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


//유닛 데이터 테이블로 갈아끼우기 전까지
[Serializable]
public class TempCardData
{
    public Rarity rarity;
    public UnitType unitType;
    public string unitName = "";
    public string description = "";
    public float health;
    public int cost;
    public float atkPower;
    public float coolTime;
    public PoolType poolType;

    public TempCardData(string name, PoolType pooltype)
    {
        this.poolType = pooltype;
        rarity = (Rarity)UnityEngine.Random.Range(1, 4);
        unitType = (UnitType)UnityEngine.Random.Range(1, 4);
        unitName = name;
        description = "설명";
        health = UnityEngine.Random.Range(100f, 5000f);
        cost = UnityEngine.Random.Range(50, 3000);
        atkPower = UnityEngine.Random.Range(100f, 5000f);
        coolTime = UnityEngine.Random.Range(5f, 60f);
    }
}

public enum Rarity 
{ 
    common,
    rare,
    epic
}

public enum UnitType 
{ 
    Tanker,
    Dealer,
    Healer
}

