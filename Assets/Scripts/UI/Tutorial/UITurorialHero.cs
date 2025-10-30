using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITutorialHero : UITutorialBase
{
    IEventPublisher<TutorialSkipEvent> tutorialSkipEventPub;

    protected override void Awake()
    {
        base.Awake();
        Time.timeScale = 0f; // 시간 멈추기
        tutorialSkipEventPub = EventManager.GetPublisher<TutorialSkipEvent>();

    }
    protected override void OnSkipButtonClicked()
    {
        base.OnSkipButtonClicked();
        tutorialSkipEventPub?.Publish(); // 시간 정상화
        UIManager.Instance.GetUI<UIHeroCinematic>().OpenHeroCinematic(HeroCinematicType.CutSceneForFirstWave);
    }
}
