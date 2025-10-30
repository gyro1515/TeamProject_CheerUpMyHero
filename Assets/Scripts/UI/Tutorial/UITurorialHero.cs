using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITutorialHero : UITutorialBase
{
    protected override void Awake()
    {
        base.Awake();
        Time.timeScale = 0f; // 시간 멈추기
    }
    protected override void OnSkipButtonClicked()
    {
        base.OnSkipButtonClicked();
        Time.timeScale = 1f; // 시간 정상화
        UIManager.Instance.GetUI<UIHeroCinematic>().OpenHeroCinematic(HeroCinematicType.CutSceneForFirstWave);
    }
}
