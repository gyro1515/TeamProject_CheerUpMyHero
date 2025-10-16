using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


//유닛 데이터 테이블로 갈아끼우기 전까지
[Serializable]
public class TempCardData
{
    public int id;
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

    public TempCardData(int id, string name, UnitType unitType, float health, int cost, float atkPower, 
        float coolTime, PoolType pooltype, Rarity rarity = Rarity.common)
    {
        this.id = id;
        //rarity = (Rarity)UnityEngine.Random.Range(0, 3);
        this.rarity = rarity;
        this.unitType = unitType;
        unitName = name;
        description = "설명";
        this.health  = health;
        this.cost = cost;
        this.atkPower = atkPower;
        this.coolTime = coolTime;
        this.poolType = pooltype;
        //중복 카드 획득에 따른 잠재력 증가, 추후 구현 예정
        potential = 0;
    }
}



