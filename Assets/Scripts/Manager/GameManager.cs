using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public enum LoadMain
{
    None,
    DeckPresetController
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

    public LoadMain LoadMain { get; set; } = LoadMain.None;

    protected override void Awake()
    {
        base.Awake();
        RewardPanelUI = UIManager.Instance.GetUI<RewardPanelUI>();
        UIStageClearArtifactSelect = UIManager.Instance.GetUI<UIStageClearArtifactSelect>();
    }
    private void Update()
    {
        // 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale += 0.5f;
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

        Debug.Log($"Battle Started! MaxFood: {PlayerDataManager.Instance.MaxFood}, CurrentFood: {PlayerDataManager.Instance.CurrentFood}");    
    }

    public void OpenSelectArtifactUI()
    {
        if (!IsBattleStarted) return;

        IsBattleStarted = false;
        Time.timeScale = 0f;

        if (UIStageClearArtifactSelect == null)
            UIStageClearArtifactSelect = UIManager.Instance.GetUI<UIStageClearArtifactSelect>();

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

    public void ShowResultUI(bool isVictory)
    {
        Time.timeScale = 0f;

        if (RewardPanelUI == null)
        {
            RewardPanelUI = UIManager.Instance.GetUI<RewardPanelUI>();
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

            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    BuildingUpgradeData building = PlayerDataManager.Instance.BuildingGridData[x, y];
                    if (building != null)
                    {
                        foreach (BuildingEffect effect in building.effects)
                        {
                            switch (effect.effectType)
                            {
                                case BuildingEffectType.BaseWoodProduction: totalBaseWood += (int)effect.effectValueMin; break;
                                case BuildingEffectType.AdditionalWoodProduction: totalBonusWoodPercent += effect.effectValueMin; break;
                                case BuildingEffectType.BaseIronProduction: totalBaseIron += (int)effect.effectValueMin; break;
                                case BuildingEffectType.AdditionalIronProduction: totalBonusIronPercent += effect.effectValueMin; break;
                                case BuildingEffectType.MagicStoneFindChance: totalMagicStoneChance += effect.effectValueMin; break;
                                case BuildingEffectType.MagicStoneProduction:
                                    totalMagicStoneMin += (int)effect.effectValueMin;
                                    totalMagicStoneMax += (int)effect.effectValueMax;
                                    break;
                            }
                        }
                    }
                }
            }

            //최종 보상을 계산
            finalGold = rewardData.rewardGold;
            finalWood = rewardData.rewardWood + (int)(totalBaseWood * (1 + totalBonusWoodPercent / 100f));
            finalIron = rewardData.rewardIron + (int)(totalBaseIron * (1 + totalBonusIronPercent / 100f));
            finalMagicStone = rewardData.rewardMagicStone;

            if (Random.Range(0, 100) < totalMagicStoneChance)
            {
                finalMagicStone += Random.Range(totalMagicStoneMin, totalMagicStoneMax + 1);
            }

            //계산된 보상을 PlayerDataManager에 추가
            PlayerDataManager.Instance.AddResource(ResourceType.Gold, finalGold);
            PlayerDataManager.Instance.AddResource(ResourceType.Wood, finalWood);
            PlayerDataManager.Instance.AddResource(ResourceType.Iron, finalIron);
            PlayerDataManager.Instance.AddResource(ResourceType.MagicStone, finalMagicStone);

            RewardPanelUI?.OpenUI(finalGold, finalWood, finalIron, finalMagicStone, true);

        }
        else // =============== 패배했을 경우 ===============
        {
            Debug.Log("패배 페널티를 적용합니다.");

            var penalties = PlayerDataManager.Instance.ApplyResourcePenalty();

            // 결과창 UI 열기 (차감된 값이므로 음수로 전달)
            RewardPanelUI?.OpenUI(-penalties.gold, -penalties.wood, -penalties.iron, -penalties.magicStone, false);
        }
    }
    public void ClearStage()
    {
        // 플레이어 선택 스테이지 데이터 기반으로 세팅
        (int mainIdx, int subIdx) = PlayerDataManager.Instance.SelectedStageIdx;
        Debug.Log($"스테이지{mainIdx + 1}-{subIdx + 1} 클리어");
        // 스테이지 데이터 가져와서 해금하기
        // 최대 서브 스테이지를 클리어 했다면 다음 메인 스테이지 해금, 서브 인덱스 1으로
        int maxSubIdx = SettingDataManager.Instance.MainStageData[mainIdx].subStages.Count;
        if (++subIdx >= maxSubIdx)
        {
            subIdx = 0;
            SettingDataManager.Instance.MainStageData[++mainIdx].isUnlocked = true;
        }
        // 서브 스테이지 해금
        SettingDataManager.Instance.MainStageData[mainIdx].subStages[subIdx].isUnlocked = true;
    }
}
