using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageModifierManager : SingletonMono<StageModifierManager>
{
    #region 데이터
    // 지금 선택된 운명 기능
    public StageDestinyData CurrentDastiny { get; private set; }

    // 지금 선택된 도전 기능
    public Dictionary<StageChallengeData, int> ActiveChallenge { get; private set; }
    #endregion

    // 운명 설정 메서드 : 운명은 하나만 설정됨
    public void SetDestiny(StageDestinyData destiny)
    {
        CurrentDastiny = destiny;
    }

    // 운명 비우는 기능 -> 매 스테이지마다 
    public void ClearDestiny()
    {
        CurrentDastiny = null;
    }

    // 도전 기능 설정 메서드 : 도전은 여러개 + 레벨도 있음. 딕셔너리 형태에 value 값을 int로 레벨로 받아줌.
    public void SetChallenges(Dictionary<StageChallengeData, int> challenges)
    {
        ActiveChallenge = challenges;
        foreach (var challenge in ActiveChallenge)
        {
            Debug.Log($"도전 기능 {challenge.Key.name}, 선택한 레벨 : {challenge.Value}");
        }
    }

    // 도전 기능 비우는 기능
    public void ClearChallenge()
    {
        ActiveChallenge.Clear();
    }
}
