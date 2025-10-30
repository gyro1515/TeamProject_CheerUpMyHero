using Cysharp.Threading.Tasks;
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
            PlayerDataManager.Instance.SaveDataToCloudAsync().Forget(); //여기선 저장 실패해도 되겠지..?
        }
    }
}
