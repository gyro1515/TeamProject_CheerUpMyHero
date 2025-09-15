using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{

    [Header("테스트용 스테이지 ID")]
    public int currentStageID = 1001;

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

                ShowResultUI(true);
            }
        }

        // ========== 플레이어 바로 죽이는 치트키 B키 =============
        if (Input.GetKeyDown(KeyCode.B))
        {
            if(Player != null && !Player.IsDead)
            {
                Debug.Log("B키 눌려서 플레이어 개체 즉시 죽임");
                Player.CurHp = 0;                                   // 근데 이거 ondead 직업 호출하는 게 더 나은지? 모르겠음
                StageDefeat();
            }
        }
        // ========================================================
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

    // ================================================================================================
    public void StageDefeat()
    {
        Debug.Log("스테이지 클리어 실패");
        Time.timeScale = 0f;

        // 차후에 데이터 테이블로 스테이지별 보상 만들어야 함
        // 일단 해당 스테이지의 원래 보상은 모든 자원 100씩인 것으로 설정하고 코드 짬.
        int goldReward = 100;
        int woodReward = 100;
        int ironReward = 100;
        int magicStoneReward = 100;

        // 우리 자원 체계가 int형인데, 20% 없애면 float형이 발생하는 경우가 반드시 생김.
        // 이런 경우 어떻게 할지 : 버릴지, 올릴지, 내릴지, 반올림할 지 정해야 함.
        ResourceManager.Instance.AddResource(ResourceType.Gold, (int)(goldReward * 0.2f));
        ResourceManager.Instance.AddResource(ResourceType.Wood, (int)(woodReward * 0.2f));
        ResourceManager.Instance.AddResource(ResourceType.Iron, (int)(ironReward * 0.2f));
        ResourceManager.Instance.AddResource(ResourceType.MagicStone, (int)(magicStoneReward * 0.2f));

        Debug.Log("게임 실패 UI 출력");
        // 실패 시 전용 UI 따로 만들 지 한 번 생각해보기
    }
    // ================================================================================================
}
