using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{
    public RewardPanelUI RewardPanelUI { get; set; }
    public EnemyHQ enemyHQ { get; set; }

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

                StageClear();
            }
        }
    }
    public void StageClear()
    {
        Debug.Log("스테이지 클리어!");
        Time.timeScale = 0f;

        int goldReward = 100;
        int woodReward = 100;
        int ironReward = 100;
        int magicStoneReward = 100;

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
            Debug.LogError("RewardPanel이 GameManager에 연결되지 않았습니다!");
        }
    }
}
