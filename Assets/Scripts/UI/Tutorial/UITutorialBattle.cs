using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITutorialBattle : UITutorialBase
{
    IEventPublisher<TutorialSkipEvent> tutorialSkipEventPub;
    protected override void Awake()
    {
        base.Awake();
        Time.timeScale = 0.0f;
        tutorialSkipEventPub = EventManager.GetPublisher<TutorialSkipEvent>();
    }

    protected override void OnSkipButtonClicked()
    {
        base.OnSkipButtonClicked();
        if (!GameManager.IsPaused) tutorialSkipEventPub?.Publish();
    }
}
struct TutorialSkipEvent{ }
