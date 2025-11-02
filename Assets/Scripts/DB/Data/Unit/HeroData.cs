using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroData : BaseUnitData
{
    public string heroTitle;           // 영웅 칭호
    public string firstWaveSpeech;      // 1웨이브 시작 시 대사
    public string firstWaveSpritePath;      // 1웨이브 시작 시 대사 경로 주소
    public Sprite firstWaveSprite;      // 1웨이브 시작 시 대사  스프라이트
    public string preSpawnSpeech;       // 영웅 소환 전 대사
    public string spawnSpritePath;          // 영웅 소환 이미지 경로
    public Sprite spawnSprite;          // 영웅 소환 이미지 스프라이트
}
