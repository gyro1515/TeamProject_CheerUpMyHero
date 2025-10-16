using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : BaseUI
{
    [SerializeField] Button clickToMove;

    private void OnEnable()
    {
        clickToMove.onClick.AddListener(onClickToMove);
    }

    private void OnDisable()
    {
        clickToMove.onClick.RemoveAllListeners();
    }


    void onClickToMove()
    {
        SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
    }
}
