using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

// 용사 타이머, 용사 관련 다 여기서 로직 처리
public class UITimer : MonoBehaviour 
{
    [SerializeField] TMP_Text timerText;

    //시작 절대 시간
    //private float startTime;

    //7분 30초
    private float totalTime = 450f;

    private float remainTime;
    bool willSpawnHero = false; // 용사 소환 예정 대사 출력

    IEventSubscriber<TimeSyncEvent> timeSyncEventSub;
    IEventSubscriber<StartWaveEvent> startWaveEventSub;
    IEventPublisher<HeroSpawnEvent> heroSpawnEventPub;
    UIHeroCinematic uIHeroCinematic; // 용사 컷씬 UI
    HeroData selectedHeroData; // 소환할 용사 데이터
    bool checkUnknownHero = false; // 용사 알 수 없음 체크용

    private void Awake()
    {
        // HQ정보 가져와서 시간 세팅하기
        timeSyncEventSub = EventManager.GetSubscriber<TimeSyncEvent>();
        timeSyncEventSub.Subscribe(SetTimer);
        startWaveEventSub = EventManager.GetSubscriber<StartWaveEvent>();
        startWaveEventSub.Subscribe(CheckWaveForFirstHeroSpeach);
        uIHeroCinematic = UIManager.Instance.GetUI<UIHeroCinematic>();
        heroSpawnEventPub = EventManager.GetPublisher<HeroSpawnEvent>();
        // 랜덤으로 소환될 용사 선택
        List<HeroData> hero_unit = DataManager.PlayerUnitData.SO.hero_unit;
        int randomIdx = Random.Range(0, hero_unit.Count);
        selectedHeroData = hero_unit[randomIdx];
        Debug.Log($"선택된 영웅: {selectedHeroData.poolType} / {selectedHeroData.unitName}");

        // 만약 랜덤요소로 용사 알수 없음이 뜬다면 여기서 처리
        //checkUnknownHero = 용사 알 수 없음 체크는 여기서 // 데이터 가져와서 체크
        checkUnknownHero = true;
        // 용사 컷씬 초기화
        uIHeroCinematic.InitHeroCinematic(selectedHeroData);
    }
    private void OnDisable()
    {
        timeSyncEventSub.Unsubscribe(SetTimer);
        startWaveEventSub.Unsubscribe(CheckWaveForFirstHeroSpeach);
    }
    IEnumerator SpendTimeRoutine()
    {
        while (remainTime > 0)
        {
            //흐른 시간
            //float spendTime = Time.time - startTime;
            remainTime -= Time.deltaTime; 
            UpdateTimer();
            // 타이머 3초 전에 소환 예정 대사 출력
            // 용사 스폰 예정 대사 출력
            if (remainTime <= 3f)
            {
                // 대사 출력
                //Debug.Log("3초 후 용사가 스폰됩니다!");
                uIHeroCinematic.OpenHeroCinematic(HeroCinematicType.HeroSpeachForPreSpawn);
                //if(!checkUnknownHero) uIHeroCinematic.OpenHeroCinematic(HeroCinematicType.HeroSpeachForPreSpawn);
            }
            yield return null;
        }
        // 용사 타이머 종료 시 용사 소환
        //Debug.Log("용사를 스폰합니다!");
        uIHeroCinematic.OpenHeroCinematic(HeroCinematicType.CutSceneForHeroSpawn);
        heroSpawnEventPub.Publish( new HeroSpawnEvent { selectedHero = selectedHeroData });
    }
    void UpdateTimer()
    {
        remainTime = Mathf.Max(0, remainTime);
        int tmpRemainTime = Mathf.CeilToInt(remainTime); // 올림? 내림? 일단 올림 처리
        int min = tmpRemainTime / 60;
        int sec = tmpRemainTime % 60;
        if (sec >= 10)
            timerText.text = $"0{min}:{sec}";
        else
            timerText.text = $"0{min}:0{sec}";
    }
    void SetTimer(TimeSyncEvent timeSyncEvent)
    {
        // 용사는 서브 스테이지 9에만 등장
        (int selectedMainStageIdx, int selectedSubStageIdx) = PlayerDataManager.Instance.SelectedStageIdx;
        // 테스트를 위해 일단 주석처리
        /*if (selectedSubStageIdx != 8)
        {
            Debug.Log("용사타이머 활성화x");
            gameObject.SetActive(false);
            return;
        }*/

        this.totalTime = timeSyncEvent.waveTime * timeSyncEvent.maxWaveCount; // 용사 타이머는 웨이브 타임 * 최대 웨이브 수
        // TODO: totalTime은 게임 환경에 따라 여기서 길이를 조절할 수 있음 ********
        Debug.Log($"용사 타이머 세팅: {totalTime}초");
        // 타이머 시작
        //startTime = GameManager.Instance.StartTime;
        //Debug.Log($"용사 타이머 시작 시간: {startTime}초");
        
        remainTime = totalTime;
        StartCoroutine(SpendTimeRoutine());
    }
    void CheckWaveForFirstHeroSpeach(StartWaveEvent startWaveEvent)
    {
        // 용사는 1웨이브 시작시에만 대사 출력
        if (startWaveEvent.waveIdx != 1) return;
        // 용사 컷씬 대사 출력
        //Debug.Log("첫 웨이브에 어떤 용사인지 출력");
        if (!checkUnknownHero) uIHeroCinematic.OpenHeroCinematic(HeroCinematicType.CutSceneForFirstWave);

    }
}
#region 용사 소환 이벤트
public struct HeroSpawnEvent 
{
    public BaseUnitData selectedHero;
}
#endregion
