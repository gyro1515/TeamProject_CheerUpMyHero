using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestButton_ToBattleScene : MonoBehaviour
{
    Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(ToBattleScene);
    }
    void ToBattleScene()
    {
        SceneLoader.Instance.StartLoadScene(SceneState.BattleScene);
    }
}
