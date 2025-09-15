using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSceneMove : MonoBehaviour
{
    [SerializeField] Button sceneMoveButton;

    private void Awake()
    {
        sceneMoveButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        sceneMoveButton.onClick.AddListener(MoveToBattleScene);
    }

    private void OnDisable()
    {
        sceneMoveButton.onClick.RemoveAllListeners();
    }

    void MoveToBattleScene()
    {
        SceneLoadManager.Instance.LoadScene(SceneState.BattleScene);
    }
}
