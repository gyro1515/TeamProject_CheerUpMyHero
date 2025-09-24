using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : SceneBase
{
    public override void OnSceneEnter()
    {
        UIManager.Instance.GetUI<MainScreenUI>();
        UIManager.Instance.GetUI<DeckPresetController>().CloseUI();
        UIManager.Instance.GetUI<UIStageSelect>().CloseUI();
    }

    public override void OnSceneExit()
    {
    }

    public override void SceneLoading()
    {
    }
}
