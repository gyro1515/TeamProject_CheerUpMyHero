using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITutorialBattle : UITutorialBase
{
    protected override void Awake()
    {
        base.Awake();
        Time.timeScale = 0.0f;
    }

    protected override void OnSkipButtonClicked()
    {
        base.OnSkipButtonClicked();
        if(!GameManager.IsPaused) Time.timeScale = 1.0f;
    }
}
