using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDestinyEffectPopup : BasePopUpUI
{
    [Header("UI 참조")]
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _description;

    protected override void Awake()
    {
        base.Awake();
    }

    public void OpenPanel(StageDestinyData destiny)
    {
        if (destiny == null) return;

        //_icon.sprite = destiny.icon;
        _title.text = destiny.name;
        _description.text = destiny.description;

        OpenUI();
    }
}
