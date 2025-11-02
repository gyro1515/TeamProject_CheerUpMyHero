using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseUnitData : MonoData
{
    public Rarity rarity;
    public string unitName;
    public UnitType unitType;
    public PoolType poolType;
    public float health;
    public int hitBack;
    public float atkPower;
    public float cognizanceRange;
    public float attackRange;
    public float moveSpeed;
    public float attackDelayTime;
    public float attackRate;
    public bool isRangedAttack;
    public UnitAttackType attackType;
    public float spawnCooldown;
    public UnitSynergyType synergyType; // 비트마스크 방식 도입
    public int cost;
    public string description = "설명 추가해야 합니다!!";
    public UnitClass unitClass = UnitClass.Normal;
    public Sprite unitBGSprite; // 유닛 카드 배경 스프라이트
    public Sprite unitIconSprite; // 유닛 카드 아이콘 스프라이트
    public Sprite gachaHeroSprite;// 가챠 이미지
    public float healAmount; // 치유량 (힐러 유닛 전용)
    public int maxTargetCount; // 최대 타겟 수 (범위 공격 유닛 전용)
}
public enum Rarity
{
    common,
    rare,
    epic,
    Legendary
}

public enum UnitType
{
    Tanker,
    Dealer,
    Healer
}
public enum UnitAttackType
{
    Target,         // 단일 대상 타격
    Area,           // 범위 공격
    PierceArea      // 관통형 범위 공격
}
[System.Flags]
public enum UnitSynergyType
{
    None = 0,
    // 세력 계열
    Kingdom     = 1 << 0,  // 왕국   
    Empire      = 1 << 1,  // 황국
    // 직업 계열
    Mage        = 1 << 2,  // 마법사
    Cleric      = 1 << 3,  // 성직자
    Berserker   = 1 << 4,  // 버서커
    Archer      = 1 << 5,  // 궁수
    Hero        = 1 << 6,  // 영웅
    //  속성 계열
    Frost       = 1 << 7,  // 냉기
    Burn        = 1 << 8,  // 화상
    Poison      = 1 << 9,  // 중독
}
public enum UnitClass
{
    Normal,
    Hero,
    Boss
}
