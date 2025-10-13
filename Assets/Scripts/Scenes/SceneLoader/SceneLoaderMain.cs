using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderMain : MonoBehaviour
{
    private void Awake()
    {
        switch (GameManager.Instance.LoadMain)
        {
            case LoadMain.None:
                UIManager.Instance.GetUI<MainScreenUI>();
                UIManager.Instance.GetUI<DeckPresetController>().CloseUI();
                break;
            case LoadMain.DeckPresetController:
                var mainUI = UIManager.Instance.GetUI<MainScreenUI>();
                mainUI.CloseUI();
                /*// 덱프리셋 컨트롤러UI를 메인 UI 위에 쌓기
                EventManager.Publish(new AddUIStackEvent { ui = mainUI });*/
                UIManager.Instance.GetUI<DeckPresetController>();
                GameManager.Instance.LoadMain = LoadMain.None;
                break;
        }
        UIManager.Instance.GetUI<UISelectActiveArtifact>().CloseUI();
        UIManager.Instance.GetUI<UIStageSelect>().CloseUI();
        //UIManager.Instance.GetUI<UISelectCard>().CloseUI();
    }
}
