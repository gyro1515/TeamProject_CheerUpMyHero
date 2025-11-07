using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    // --- 네트워크 (BackendManger 관련) ---

    // 캐시 유효 시간 (초)
    public const float NETWORK_CACHE_DURATION = 5.0f;
    // 인터넷 확인용 주소
    public const string NETWORK_CHECK_URL = "https://connectivitycheck.gstatic.com/generate_204";

    //재화 ID값
    public const string GOLD_ID = "GOLD";
    public const string WOOD_ID = "WOOD";
    public const string IRON_ID = "IRON";
    public const string TICKET_ID = "TICKET";
    public const string MAGICSTONE_ID = "MAGICSTONE";
    public const string BM_ID = "BM";

    //세이브 데이터 Key값
    public static string PLAYER_DATA_KEY = "PLAYER_DATA";
    public static string PITY_COUNT_KEY_PREFIX = "PITYCOUNT_";
    public static string NORMAL_GACHA_KEY = "NORMAL_BANNER";
    public static string PICKUP_GACHA_KEY = "PICKUP_BANNER";

    //통계 전송 이벤트 파라미터 Key값
    public const string IS_HERO_ARRIVE = "isHeroArriveStage";
    public const string IS_STAGE_CHALLENGE = "isStageChallenge";
    public const string IS_STAGE_CLEARED = "isStageCleared";
    public const string IS_STAGE_CLEARED_BUT_TRY = "isStageClearedButTryAgain";
    public const string STAGE_CHALLENGE_DATA = "stageChallengeData";
    public const string STAGE_CONSTRUCTION = "stageConstruction";
    public const string STAGE_DESTNIY_ID = "stageDestinyId";
    public const string STAGE_ID = "stageId";
    public const string STAGE_SUPPLY_LEVEL = "stageSupplyLevel";
    public const string STAGE_TIME_TAKEN = "stageTimeTaken";
    public const string STAGE_USED_ARTIFACT = "stageUsedArtifat";
    public const string STAGE_USED_UNIT = "stageUsedUnit";



}
