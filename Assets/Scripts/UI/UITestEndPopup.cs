using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITestEndPopup : BaseUI
{
    [SerializeField] private Button _backToMainButton;

    private void Awake()
    {
        _backToMainButton.onClick.AddListener(OnBackToMainButtonClicked);
    }

    private void OnBackToMainButtonClicked()
    {
        GameManager.IsStageAndDestinySelected = false;
        SceneLoader.Instance.StartLoadScene(SceneState.MainScene);
        CloseUI();
    }
}
