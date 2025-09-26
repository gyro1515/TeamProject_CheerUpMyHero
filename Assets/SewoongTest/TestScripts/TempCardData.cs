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
    public int potential;
    public PoolType poolType;

    public TempCardData(string name, PoolType pooltype)
    {
        this.poolType = pooltype;
        rarity = (Rarity)UnityEngine.Random.Range(1, 4);
        unitType = (UnitType)UnityEngine.Random.Range(1, 4);
        unitName = name;
        description = "설명";
        health = UnityEngine.Random.Range(100f, 5000f);
        cost = UnityEngine.Random.Range(50, 100);
        atkPower = UnityEngine.Random.Range(100f, 5000f);
        coolTime = UnityEngine.Random.Range(5f, 10f);
        //중복 카드 획득에 따른 잠재력 증가, 추후 구현 예정
        potential = 0;
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

