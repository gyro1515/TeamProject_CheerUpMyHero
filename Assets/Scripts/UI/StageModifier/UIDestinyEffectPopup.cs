using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDestinyEffectPopup : BaseUI
{
    [Header("UI 참조")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _description;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OpenPanel(StageDestinyData destiny)
    {
        if (destiny == null) return;

        //_icon.sprite = destiny.icon;
        _title.text = destiny.name;
        _description.text = destiny.description;
        base.OpenUI();
    }
}
