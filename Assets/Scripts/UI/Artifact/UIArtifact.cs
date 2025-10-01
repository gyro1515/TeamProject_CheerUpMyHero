using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifact : BaseUI
{
    [Header("UI간 이동 버튼")]
    [SerializeField] private Button _closeButton; //지금 비활성화 되어있음
    [SerializeField] private Button _gotoCardDeckButton;

    CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        _gotoCardDeckButton.onClick.AddListener(OnCardDeckClicked);
    }

    private void OnCloseButtonClicked()
    {
        FadeManager.Instance.FadeOutUI(_canvasGroup);
    }

    private void OnCardDeckClicked()
    {
        FadeManager.Instance.SwitchGameObjects(gameObject, UIManager.Instance.GetUI<DeckPresetController>().gameObject);
    }
}
