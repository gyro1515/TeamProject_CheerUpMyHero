using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeModel
{
    private const float RewardBonusPerPoint = 3.0f;

    public StageChallengeData GetChallengeData(int id)
    {
        return DataManager.Instance.StageModifierData.GetData(id) as StageChallengeData;
    }

    public float CalculateRewardBonusPer(Dictionary<int, int> challenges)
    {
        float totalPoint = 0;
        foreach (var challenge in challenges)
        {
            StageChallengeData data = GetChallengeData(challenge.Key);
            if (data != null)
            {
                totalPoint += data.pointPerLevel * challenge.Value;
            }
        }
        return totalPoint * RewardBonusPerPoint;
    }

    public void ApplyChallenges(Dictionary<int, int> challenges)
    {
        var data = PlayerDataManager.Instance.activeChallenges;

        data.Clear();
        foreach (var challenge in challenges)
        {
            data[challenge.Key] = challenge.Value;
        }

        Debug.Log($"챌린지 {challenges.Count}개 저장됨");
    }
}
