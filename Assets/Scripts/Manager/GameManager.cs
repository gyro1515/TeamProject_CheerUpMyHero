using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using Unity.Services.Analytics;
using UnityEngine;

public enum LoadMain
{
    None,
    DeckPresetController,
    UIDestinyRoullette
}
public class GameManager : SingletonMono<GameManager>
{

    [Header("테스트용 스테이지 ID")]
    public int currentStageID = 1001;

    public RewardPanelUI RewardPanelUI { get; set; }
    public UIStageClearArtifactSelect UIStageClearArtifactSelect { get; set; }

    public EnemyHQ enemyHQ { get; set; }
    public PlayerHQ PlayerHQ { get; set; }

    public Player Player { get; set; }
    public bool IsBattleStarted { get; private set; } = false;

    public float StartTime { get; private set; }
    //public bool NeedsTileVisualUpdate { get; set; } = false;

    public LoadMain LoadMain { get; set; } = LoadMain.None;

    bool isStageAndDestinySelected = false;
    public static bool IsStageAndDestinySelected { get => Instance.isStageAndDestinySelected; set => Instance.isStageAndDestinySelected = value; }

    bool isTutorialCompleted = false;
    public static bool IsTutorialCompleted { get => Instance.isTutorialCompleted; set => Instance.isTutorialCompleted = value; }

    bool isClearedButTryAgain = false;

    protected override void Awake()
    {
        base.Awake();
        // 배틀 씬에만 존재해야 합니다
        /*RewardPanelUI = UIManager.Instance.GetUI<RewardPanelUI>();
        UIStageClearArtifactSelect = UIManager.Instance.GetUI<UIStageClearArtifactSelect>();*/

        // 튜토리얼 완료 여부 설정
        // TODO: 서버에서 불러오기
        isTutorialCompleted = false;
    }
    private void Update()
    {
        // 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale += 0.5f;
            Time.timeScale = Mathf.Clamp(Time.timeScale,0, 5f);

            //Debug.Log("AddTimeScale");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 1.0f;
            //Debug.Log("ResetTimeScale");
        }
        // ***현재는 밸런스 때문에 키를 눌러 클리어/실패 결과를 출력, 추후 각 HQ, Player에 옮겨야 할 내용**********
        //C키 눌러서 적 HQ 파괴
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("'C'키 입력!");

            // enemyHQ가 존재하고, 아직 파괴되지 않았다면
            if (enemyHQ != null && enemyHQ.gameObject.activeInHierarchy)
            {
                Debug.Log("'C'키 입력! 적 HQ를 강제 파괴합니다.");

                enemyHQ.CurHp = 0;
            }
        }

        // 플레이어 HQ 바로 죽이는 치트키 V키
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (PlayerHQ != null && PlayerHQ.gameObject.activeInHierarchy)
            {
                Debug.Log("V키 눌려서 아군 HQ 터뜨림");
                PlayerHQ.CurHp = 0;
            }
        }

        // 플레이어 바로 죽이는 치트키 B키
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (Player != null && !Player.IsDead)
            {
                Debug.Log("B키 눌려서 플레이어 개체 즉시 죽임");
                Player.CurHp = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (enemyHQ != null && enemyHQ.gameObject.activeInHierarchy)
            {
                Debug.Log("H키 눌려서 적 HQ 피 반 깎음");
                enemyHQ.CurHp = enemyHQ.MaxHp * 0.5f;
            }
        }
        if (IsBattleStarted)
        {
            PlayerDataManager.Instance.AddFoodOverTime(Time.deltaTime);
        }
    }

    public void StartBattle()
    {
        PlayerDataManager.Instance.ResetFood();

        IsBattleStarted = true;

        StartTime = Time.time;

        //배틀 시작때 이미 클리어한 스테이지인지 확인
        (int mainIdx, int subIdx) = PlayerDataManager.Instance.SelectedStageIdx;
        isClearedButTryAgain = PlayerDataManager.Instance.IsStageCleared(mainIdx +1, subIdx + 1);
        Debug.Log($"{mainIdx}, {subIdx}");
        Debug.Log($"<color=green> 이미 클리어한 스테이지 입니까?{isClearedButTryAgain}</color>");

        Debug.Log($"Battle Started! MaxFood: {PlayerDataManager.Instance.MaxFood}, CurrentFood: {PlayerDataManager.Instance.CurrentFood}");    
    }

    public void OpenSelectArtifactUI()
    {
        if (!IsBattleStarted) return;

        IsBattleStarted = false;
        Time.timeScale = 0f;

        if (UIStageClearArtifactSelect == null)
        {
            UIStageClearArtifactSelect = UIManager.Instance.GetUI<UIStageClearArtifactSelect>();
            //UIStageClearArtifactSelect.CloseUI();
        }

        (int mainIdx, int subIdx) = PlayerDataManager.Instance.SelectedStageIdx;
        if (subIdx + 1 == 9)
        {
            UIStageClearArtifactSelect.OpenSelectUI(ArtifactType.Passive, true);
        }
        else
        {
            UIStageClearArtifactSelect.OpenSelectUI(ArtifactType.Passive);
        }
    }

    public async UniTaskVoid ShowResultUI(bool isVictory)
    {
        EventManager.GetPublisher<BattleEndedEvent>().Publish(new BattleEndedEvent { IsVictory = isVictory });

        Modifiercalculator.EndBattle();

        CollectStageResultAnalyticsEvent(isVictory);

        //NeedsTileVisualUpdate = true;

        Time.timeScale = 0f;

        if (RewardPanelUI == null)
        {
            RewardPanelUI = UIManager.Instance.GetUI<RewardPanelUI>();
            RewardPanelUI.gameObject.SetActive(false);
        }

        int finalGold = 0;
        int finalWood = 0;
        int finalIron = 0;
        int finalMagicStone = 0;

        if (isVictory) // =============== 승리했을 경우 ===============
        {
            StageRewardData rewardData = DataManager.Instance.RewardData.GetData(currentStageID);
            if (rewardData == null)
            {
                Debug.LogError($"ID: {currentStageID}에 해당하는 보상 데이터를 찾을 수 없습니다!");
                return; // 보상 데이터가 없으면 함수 종료
            }

            // 건물 효과를 합산하여 추가 보너스를 계산
            int totalBaseWood = 0;
            float totalBonusWoodPercent = 0f;
            int totalBaseIron = 0;
            float totalBonusIronPercent = 0f;
            float totalMagicStoneChance = 0f;
            int totalMagicStoneMin = 0;
            int totalMagicStoneMax = 0;

            for (int y = 0; y < 4; y++) // 범위를 4x4 일반 타일로 수정
            {
                for (int x = 0; x < 4; x++) // 범위를 4x4 일반 타일로 수정
                {
                    float efficiencyMultiplier = 1.0f;
                    float additiveBonusPercent = 0f;


                    if (PlayerDataManager.Instance.TileEfficiencyBonuses.TryGetValue((x, y), out float bonusPercent))
                    {
                        efficiencyMultiplier += bonusPercent / 100.0f;
                        additiveBonusPercent = bonusPercent;
                    }

                    BuildingUpgradeData building = PlayerDataManager.Instance._TileDataHandler.BuildingGridData[x, y];
                    if (building != null)
                    {
                        foreach (BuildingEffect effect in building.effects)
                        {
                            switch (effect.effectType)
                            {
                                case BuildingEffectType.BaseWoodProduction:
                                    totalBaseWood += Mathf.CeilToInt(effect.effectValueMin * efficiencyMultiplier);
                                    break;
                                case BuildingEffectType.AdditionalWoodProduction:
                                    totalBonusWoodPercent += effect.effectValueMin + additiveBonusPercent;
                                    break;
                                case BuildingEffectType.BaseIronProduction:
                                    totalBaseIron += Mathf.CeilToInt(effect.effectValueMin * efficiencyMultiplier);
                                    break;
                                case BuildingEffectType.AdditionalIronProduction:
                                    totalBonusIronPercent += effect.effectValueMin * additiveBonusPercent;
                                    break;
                                case BuildingEffectType.MagicStoneFindChance:
                                    totalMagicStoneChance += effect.effectValueMin;
                                    break;
                                case BuildingEffectType.MagicStoneProduction:
                                    totalMagicStoneMin += Mathf.CeilToInt(effect.effectValueMin * efficiencyMultiplier);
                                    totalMagicStoneMax += Mathf.CeilToInt(effect.effectValueMax * efficiencyMultiplier);
                                    break;
                            }
                        }
                    }
                }
            }

            //최종 보상을 계산
            finalGold = rewardData.rewardGold;
            finalWood = rewardData.rewardWood + Mathf.CeilToInt(totalBaseWood * (1 + totalBonusWoodPercent / 100f));
            finalIron = rewardData.rewardIron + Mathf.CeilToInt(totalBaseIron * (1 + totalBonusIronPercent / 100f));
            finalMagicStone = rewardData.rewardMagicStone;

            if (Random.Range(0, 100) < totalMagicStoneChance)
            {
                finalMagicStone += Random.Range(totalMagicStoneMin, totalMagicStoneMax + 1);
            }
            // 스테이지 클리어 효과음 출력함.
            AudioManager.PlayOneShot(DataManager.AudioData.StageClearSE, 0.7f);

            RewardPanelUI?.OpenUI(finalGold, finalWood, finalIron, finalMagicStone, true);
            try
            {
                //계산된 보상을 PlayerDataManager에 추가
                var rewardTasks = new List<UniTask>()
                {
                    PlayerDataManager.Instance.AddResource(ResourceType.Gold, finalGold),
                    PlayerDataManager.Instance.AddResource(ResourceType.Wood, finalWood),
                    PlayerDataManager.Instance.AddResource(ResourceType.Iron, finalIron),
                    PlayerDataManager.Instance.AddResource(ResourceType.MagicStone, finalMagicStone),
                };

                await UniTask.WhenAll(rewardTasks);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogWarning("에러 팝업: 에러가 나서 보상을 주지 못했습니다.");
            }
        }
        else // =============== 패배했을 경우 ===============
        {
            (int gold, int wood, int iron, int magicStone) penalties = (0, 0, 0, 0);
            if (GameManager.IsTutorialCompleted)
            {
                Debug.Log("패배 페널티를 적용합니다.");

                penalties = await PlayerDataManager.Instance.ApplyResourcePenalty();
            }
            else
            {
                Debug.Log("튜토리얼에서는 패널티 없습니다.");
            }

            // 스테이지 패배 효과음
            AudioManager.PlayOneShot(DataManager.AudioData.StageFailSE, 0.7f);

            // 결과창 UI 열기 (차감된 값이므로 음수로 전달)
            RewardPanelUI?.OpenUI(-penalties.gold, -penalties.wood, -penalties.iron, -penalties.magicStone, false);
        }

        await PlayerDataManager.Instance.SaveDataToCloudAsync();
    }
    public async UniTaskVoid ClearStage()
    {
        try
        {

            // 플레이어 선택 스테이지 데이터 기반으로 세팅
            (int mainIdx, int subIdx) = PlayerDataManager.Instance.SelectedStageIdx;
            Debug.Log($"스테이지{mainIdx + 1}-{subIdx + 1} 클리어");
            // 스테이지 데이터 가져와서 해금하기
            // 최대 서브 스테이지를 클리어 했다면 다음 메인 스테이지 해금, 서브 인덱스 1으로

            bool isFirstClear = !PlayerDataManager.Instance.IsStageCleared(mainIdx + 1, subIdx + 1);
            if (isFirstClear)
            {
                Debug.Log($"스테이지 {mainIdx + 1}-{subIdx + 1} 최초 클리어!");
                await PlayerDataManager.Instance.MarkLocalStageClear(mainIdx + 1, subIdx + 1);
            }

            else
            {
                Debug.Log($"스테이지 {mainIdx + 1}-{subIdx + 1}은(는) 이미 클리어한 스테이지입니다.");
            }

            int maxSubIdx = SettingDataManager.Instance.MainStageData[mainIdx].subStages.Count;
            if (++subIdx >= maxSubIdx)
            {
                subIdx = 0;
                SettingDataManager.Instance.MainStageData[++mainIdx].isUnlocked = true;
            }
            // 서브 스테이지 해금
            SettingDataManager.Instance.MainStageData[mainIdx].subStages[subIdx].isUnlocked = true;

            await PlayerDataManager.Instance.SaveDataToCloudAsync();

            GameManager.IsStageAndDestinySelected = false; // 스테이지 선택부터 다시 하도록 설정

        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogWarning("에러 팝업: 에러가 나서 보상을 주지 못했습니다.");
        }
        
        //finally는 없다. 스테이지 클리어하고 인터넷 없으면 그냥 결과 날라가는거다!

    }

    private void CollectStageResultAnalyticsEvent(bool isVictory)
    {
        PlayerDataManager playerDataManager = PlayerDataManager.Instance;

        //스테이지 id (1-1 => 1001)
        (int mainIdx, int subIdx) = playerDataManager.SelectedStageIdx;
        mainIdx++;
        subIdx++;
        Debug.Log($"{mainIdx}, {subIdx}");
        StringBuilder sb = new StringBuilder();
        sb.Append(mainIdx.ToString());
        sb.Append(0.ToString());
        sb.Append(0.ToString());
        sb.Append(subIdx.ToString());
        bool boool = int.TryParse(sb.ToString(), out int stageId);

        //도전 관련
        bool hasChallenge = false;
        if (playerDataManager.activeChallenges.Count > 0)
        {
            hasChallenge = true;
        }

        StageResultEvent stageResultEvent = new StageResultEvent()
        {
            isHeroArriveStage_Bool = this.PlayerHQ.IsSpawnHero,
            isStageChallenge_Bool = hasChallenge,
            isStageCleard_Bool = isVictory,
            isStageClearedButTryAgain_Bool = isClearedButTryAgain,

            stageChallengeData_String = ConvertToJson<Dictionary<int, int>>(playerDataManager.activeChallenges),
            stageConstruction_String = ConvertToJson<TileDataSnapshot>(playerDataManager._TileDataHandler.GetSnapshot()),
            stageDestinyId_Int = playerDataManager.currentDastiny.idNumber,
            stageId_Int = stageId,
            stageSupplyLevel_Int = playerDataManager.SupplyLevel,
            stageTimeTaken_Float = Time.time - StartTime,
            stageUsedArtifat_String = ArtifactManager.Instance.SaveArtifactData(ArtifactManager.Instance.EquippedArtifacts), //기존 로직 재사용
            stageUsedUnit_String = ConvertToJson<Dictionary<PoolType, int>>(this.PlayerHQ.UnitSpawnCnt), //일단 풀타입으로 내보내고, 나중에 통계 정리할때 유닛 붙이기로 어짜피 내가 해야하니..
        };

        if (AnalyticsService.Instance != null)
        {
            AnalyticsService.Instance.RecordEvent(stageResultEvent);
            Debug.Log("<color=cyan>스테이지 통계가 기록되었습니다</color>");
        }
        else
        {
            Debug.LogWarning("AnalyticsService 인스턴스가 없습니다. 스테이지 통계를 기록할 수 없습니다.");
        }
        
        Debug.Log($"사용된 유닛 통계: {ConvertToJson<Dictionary<PoolType, int>>(this.PlayerHQ.UnitSpawnCnt)}");
    }

    private string ConvertToJson<T>(T obj)
    {
        if (obj == null) { return null; }
        
        string result = JsonConvert.SerializeObject(obj, Formatting.Indented);

        return result;
    }
}
