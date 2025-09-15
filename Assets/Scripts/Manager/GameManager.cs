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

        int goldReward;
        int woodReward;
        int ironReward;
        int magicStoneReward;

        if (isVictory)
        {
            //가져온 보상 데이터(rewardData)의 값을 사용
            goldReward = rewardData.rewardGold;
            woodReward = rewardData.rewardWood;
            ironReward = rewardData.rewardIron;
            magicStoneReward = rewardData.rewardMagicStone;
        }
        else
        {
            //패배 시에도 가져온 데이터를 기준으로 20%를 계산
            goldReward = (int)(rewardData.rewardGold * 0.2f);
            woodReward = (int)(rewardData.rewardWood * 0.2f);
            ironReward = (int)(rewardData.rewardIron * 0.2f);
            magicStoneReward = (int)(rewardData.rewardMagicStone * 0.2f);
        }

        ResourceManager.Instance.AddResource(ResourceType.Gold, goldReward);
        ResourceManager.Instance.AddResource(ResourceType.Wood, woodReward);
        ResourceManager.Instance.AddResource(ResourceType.Iron, ironReward);
        ResourceManager.Instance.AddResource(ResourceType.MagicStone, magicStoneReward);

        if (RewardPanelUI != null)
        {
            RewardPanelUI.OpenUI(goldReward, woodReward, ironReward, magicStoneReward);
        }
        else
        {
            Debug.LogError("RewardPanel이 GameManager에 등록되지 않았습니다!");
        }
    }
}
