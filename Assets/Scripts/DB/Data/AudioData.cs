using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Sound Data", order = 0)]
public class AudioData : ScriptableObject
{
    [Header("배경음")]
    public AudioClip mainBGM;
    public AudioClip startBGM;
    
    [Header("전투 배경음")]
    public AudioClip chapter1BGM;
    public AudioClip chapter2BGM;
    public AudioClip chapter3BGM;
    public AudioClip chapter4BGM;
    public AudioClip chapter5BGM;

    [Header("보스 배경음")]
    public AudioClip BossStageBGM;

    [Header("스테이지 효과음")]
    public AudioClip StageClearSE;
    public AudioClip StageFailSE;
    public AudioClip ClearArtifactSE;
    public AudioClip BattleStartSE;

    [Header("UI 효과음")]
    public AudioClip levelUpSE;
    public AudioClip buildingSE;
    public AudioClip cardEquipSE;
    public AudioClip artifactEquipSE;
    public AudioClip stageModifierselectedSE;
    public AudioClip buttonTouchSE;
    public AudioClip optionOpenSE;
    public AudioClip optionQuitSE;
    public AudioClip waveWarningSE;

    [Header("전투 효과음")]
    public AudioClip[] monsterWaveSE_oak;
    public AudioClip magicUnitAttackSE;
    public AudioClip[] meleeUnitAttackSE;
    public AudioClip archerUnitAttackSE;
    public AudioClip hqDestroySE;
    public AudioClip playerWarningHpSE;
    public AudioClip unitHealSE;
    public AudioClip useHQSkill;
    public AudioClip hqSkillSound;

    [Header("유물 효과음")]
    public AudioClip AF_IceBreath;
    public AudioClip AF_ThunderGod;
    public AudioClip AF_golem;
    public AudioClip AF_goddess;
    public AudioClip AF_KingdomMarch;

    [Header("시너지 효과음")]
    public AudioClip synergy_fireSE;
    public AudioClip synergy_iceSE;
    public AudioClip synergy_poisonSE;

    [Header("용사 효과음")]
    public AudioClip heroAppearSE;
    public AudioClip heroBuffSE;
}
