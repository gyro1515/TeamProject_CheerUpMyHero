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

        //C키 눌러서 적 HQ 파괴
        if (Input.GetKeyDown(KeyCode.C))
        {
            // enemyHQ가 존재하고, 아직 파괴되지 않았다면
            if (enemyHQ != null && enemyHQ.gameObject.activeInHierarchy)
            {
                Debug.Log("'C'키 입력! 적 HQ를 강제 파괴합니다.");

                enemyHQ.Dead();

                ShowResultUI(true);
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

        ResourceManager.Instance.AddResource(ResourceType.Gold, goldReward);
        ResourceManager.Instance.AddResource(ResourceType.Wood, woodReward);
        ResourceManager.Instance.AddResource(ResourceType.Iron, ironReward);
        ResourceManager.Instance.AddResource(ResourceType.MagicStone, magicStoneReward);

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
}
