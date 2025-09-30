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
                UIManager.Instance.GetUI<MainScreenUI>().CloseUI();
                UIManager.Instance.GetUI<DeckPresetController>();
                GameManager.Instance.LoadMain = LoadMain.None;
                break;
        }
        UIManager.Instance.GetUI<UISelectActiveArtifact>().CloseUI();
        UIManager.Instance.GetUI<UIStageSelect>().CloseUI();
        //UIManager.Instance.GetUI<UISelectCard>().CloseUI();
    }
}
