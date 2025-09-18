using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{

    [Header("테스트용 스테이지 ID")]
    public int currentStageID = 1001;

    public RewardPanelUI RewardPanelUI { get; set; }
    public EnemyHQ enemyHQ { get; set; }
    public PlayerHQ PlayerHQ { get; set; }

    public Player Player { get; set; }

    protected override void Awake()
    {
        base.Awake();
        //RewardPanelUI = UIManager.Instance.GetUI<RewardPanelUI>();
    }
    private void Update()
    {
        // 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale += 0.5f;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 1.0f;
        }
        // ***현재는 밸런스 때문에 키를 눌러 클리어/실패 결과를 출력, 추후 각 HQ, Player에 옮겨야 할 내용**********
        //C키 눌러서 적 HQ 파괴
        if (Input.GetKeyDown(KeyCode.C))
        {
            // enemyHQ가 존재하고, 아직 파괴되지 않았다면
            if (enemyHQ != null && enemyHQ.gameObject.activeInHierarchy)
            {
                Debug.Log("'C'키 입력! 적 HQ를 강제 파괴합니다.");

                enemyHQ.Dead();

                ShowResultUI(true);
                ClearStage();
            }
        }

        // 플레이어 HQ 바로 죽이는 치트키 V키
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (PlayerHQ != null && PlayerHQ.gameObject.activeInHierarchy)
            {
                Debug.Log("V키 눌려서 아군 HQ 터뜨림");
                PlayerHQ.CurHp = 0;
                ShowResultUI(false);
            }
        }

        // 플레이어 바로 죽이는 치트키 B키
        if (Input.GetKeyDown(KeyCode.B))
        {
            if(Player != null && !Player.IsDead)
            {
                Debug.Log("B키 눌려서 플레이어 개체 즉시 죽임");
                Player.CurHp = 0;
                ShowResultUI(false);
            }
        }
    }

    public void ShowResultUI(bool isVictory)
    {
        Time.timeScale = 0f;

        StageRewardData rewardData = DataManager.Instance.RewardData.GetData(currentStageID);
        if (rewardData == null)
        {
            Debug.LogError($"ID: {currentStageID}에 해당하는 보상 데이터를 DataManager에서 찾을 수 없습니다!");
            return;
        }

        float rewardMultiplier = isVictory ? 1.0f : 0.2f;
        int goldReward = Mathf.CeilToInt(rewardData.rewardGold * rewardMultiplier);
        int woodReward = Mathf.CeilToInt(rewardData.rewardWood * rewardMultiplier);
        int ironReward = Mathf.CeilToInt(rewardData.rewardIron * rewardMultiplier);
        int magicStoneReward = Mathf.CeilToInt(rewardData.rewardMagicStone * rewardMultiplier);

        PlayerDataManager.Instance.AddResource(ResourceType.Gold, goldReward);
        PlayerDataManager.Instance.AddResource(ResourceType.Wood, woodReward);
        PlayerDataManager.Instance.AddResource(ResourceType.Iron, ironReward);
        PlayerDataManager.Instance.AddResource(ResourceType.MagicStone, magicStoneReward);

        // 실패 UI 따로 만들 거면 여기서 조건문 걸어주기

        if (RewardPanelUI != null && isVictory)
        {
            RewardPanelUI.OpenUI(goldReward, woodReward, ironReward, magicStoneReward, true);
        }
        else if (RewardPanelUI != null && !isVictory)
        {
            RewardPanelUI.OpenUI(goldReward, woodReward, ironReward, magicStoneReward, false);
        }
        else
        {
            Debug.LogError("RewardPanel이 GameManager에 등록되지 않았습니다!");
        }
    }
    void ClearStage()
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
