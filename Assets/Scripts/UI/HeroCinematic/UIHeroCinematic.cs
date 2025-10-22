using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHeroCinematic : BaseUI
{
    [Header("영웅 소환 시네마틱 설정")]
    [SerializeField] CutSceneForFirstWave cutSceneForFirstWave;
    [SerializeField] HeroSpeachForPreSpawn heroSpeachForPreSpawn;
    [SerializeField] CutSceneForHeroSpawn cutSceneForHeroSpawn;

    public void InitHeroCinematic(HeroData heroData)
    {
        // 사용될 용사 정보 받아와서 초기화
        cutSceneForFirstWave.InitCutSceneForFirstWave(heroData);
        heroSpeachForPreSpawn.InitHeroSpeachForPreSpawn(heroData);
        cutSceneForHeroSpawn.InitCutSceneForHeroSpawn(heroData);
    }
    public void OpenHeroCinematic(HeroCinematicType openType)
    {
        switch (openType)
        {
            case HeroCinematicType.CutSceneForFirstWave:
                cutSceneForFirstWave.OpenUI();
                break;
            case HeroCinematicType.HeroSpeachForPreSpawn:
                heroSpeachForPreSpawn.OpenPanel();
                break;
            case HeroCinematicType.CutSceneForHeroSpawn:
                cutSceneForHeroSpawn.OpenPanel();
                break;
        }
    }
}
public enum HeroCinematicType
{
    CutSceneForFirstWave,
    HeroSpeachForPreSpawn,
    CutSceneForHeroSpawn
}
