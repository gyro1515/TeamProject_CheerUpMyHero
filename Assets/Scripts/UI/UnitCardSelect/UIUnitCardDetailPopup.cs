using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitCardDetailPopup : BasePopUpUI
{
    [Header("UI 요소")]
    [SerializeField] private UIUnitCardInScroll _display;

    private UIUnitCardSelect _controller;

    public void Initialize(UIUnitCardSelect controller)
    {
        _controller = controller;
    }

    public void Show(BaseUnitData data)
    {
        if (data == null || _display) { return; }

        base.OpenUI();

        _display.UpdateCardDataByData(data);
    }

    public override void OnBackPressed()
    {
        CloseUI();
    }
}
