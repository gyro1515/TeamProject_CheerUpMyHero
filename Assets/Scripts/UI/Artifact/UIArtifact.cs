using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifact : BaseUI
{
    [Header("돌아가기 버튼")]
    [SerializeField] private Button _closeButton;

    CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _closeButton.onClick.AddListener(() => SceneLoader.Instance.StartLoadScene(SceneState.BattleScene));
    }

    private void OnCloseButtonClicked()
    {
        FadeManager.FadeOutUI(_canvasGroup);
    }
}
