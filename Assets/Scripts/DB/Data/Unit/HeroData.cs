using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroData : BaseUnitData
{
    public string firstWaveSpeech;      // 1웨이브 시작 시 대사
    public Sprite firstWaveSprite;      // 1웨이브 시작 시 대사 이미지
    public string preSpawnSpeech;       // 영웅 소환 전 대사
    public Sprite spawnSprite;          // 영웅 소환 이미지
}
