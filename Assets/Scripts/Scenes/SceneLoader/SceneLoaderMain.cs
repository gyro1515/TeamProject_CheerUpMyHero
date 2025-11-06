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
                UIManager.Instance.GetUI<UIMenu>();
                UIManager.Instance.GetUI<MainScreenUI>().CloseUI();
                UIManager.Instance.GetUI<DeckPresetController>().CloseUI();
                UIManager.Instance.GetUI<UIDestinyRoullette>().CloseUI();
                break;
            case LoadMain.DeckPresetController: // 패배 후, 덱 재배치 선택시
                UIManager.Instance.GetUI<UIMenu>().CloseUI();
                var mainUI = UIManager.Instance.GetUI<MainScreenUI>();
                mainUI.CloseUI();
                /*// 덱프리셋 컨트롤러UI를 메인 UI 위에 쌓기
                EventManager.Publish(new AddUIStackEvent { ui = mainUI });*/
                UIManager.Instance.GetUI<DeckPresetController>();
                UIManager.Instance.GetUI<UIDestinyRoullette>().CloseUI();
                GameManager.Instance.LoadMain = LoadMain.None;
                break;
            case LoadMain.UIDestinyRoullette: // 승리 후, 다음 스테이지 선택시
                UIManager.Instance.GetUI<UIMenu>().CloseUI();
                UIManager.Instance.GetUI<MainScreenUI>().CloseUI();
                UIManager.Instance.GetUI<DeckPresetController>().CloseUI();
                UIManager.Instance.GetUI<UIDestinyRoullette>();
                GameManager.Instance.LoadMain = LoadMain.None;
                break;
            case LoadMain.TutorialInWisdom:
                UIManager.Instance.GetUI<UIMenu>().CloseUI();
                UIManager.Instance.GetUI<MainScreenUI>();
                UIManager.Instance.GetUI<DeckPresetController>().CloseUI();
                UIManager.Instance.GetUI<UIDestinyRoullette>().CloseUI();
                GameManager.Instance.LoadMain = LoadMain.None;
                break;
        }
        UIManager.Instance.GetUI<GachaUIPanel>().CloseUI();
        //UIManager.Instance.GetUI<UISelectActiveArtifact>().CloseUI();
        UIManager.Instance.GetUI<UIStageSelect>().CloseUI();
        //UIManager.Instance.GetUI<UISelectCard>().CloseUI();
    }
}
