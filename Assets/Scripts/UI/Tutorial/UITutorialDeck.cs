using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITutorialDeck : UITutorialBase
{
    protected override void OnSkipButtonClicked()
    {
        base.OnSkipButtonClicked();

        if(!GameManager.IsTutorialCompleted)
        {
            GameManager.IsTutorialCompleted = true;
            //TODO : 서버도 변경해줘야 함.
        }
    }
}
