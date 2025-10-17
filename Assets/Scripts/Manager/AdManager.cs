using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AdManager : SingletonMono<AdManager>
{
    public void ShowRewardedAd(Action onReward)
    {
        Debug.Log("보상형 광고를 재생합니다...");

        Debug.Log("광고 시청 완료! 보상을 지급합니다.");
        onReward?.Invoke();
    }
}
