using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitCardSlot : MonoBehaviour
{
    [SerializeField] TMP_Text cardNameText;
    [SerializeField] TMP_Text costText;
    [SerializeField] Image bgImg;
    [SerializeField] Image unitIconImg;
    [SerializeField] UIRarityIconArea rarityIconArea;
    [SerializeField] GameObject GreyBlocker;

    private BaseUnitData _curUnitData;
    private UIUnitCardSelect _controller;
    private bool _canSelect;

    private Button _button;
    private UIAdvancedButton _advancedButton;

    private void Start()
    {
        _advancedButton = GetComponent<UIAdvancedButton>();

        _advancedButton.onShortClick += OnShortClicked;
        _advancedButton.onHoldStart += OnHoldStarted;
        //_advancedButton.onHoldRelease += OnHoldReleased;
    }

    public void Initialize(BaseUnitData data, UIUnitCardSelect controller, bool canSelect)
    {
        _curUnitData = data;
        _controller = controller;
        _canSelect = canSelect;

        if (data == null) return;

        cardNameText.text = $"{data.unitName}";
        rarityIconArea.SetIconCnt((int)data.rarity);
        costText.text = $"식량\n{data.cost.ToString("F0")}";
        bgImg.sprite = data.unitBGSprite;
        unitIconImg.sprite = data.unitIconSprite;
        /*if (_advancedButton == null) _advancedButton = GetComponent<UIAdvancedButton>();
        _advancedButton.onShortClick -= OnShortClicked;
        _advancedButton.onHoldStart -= OnHoldStarted;
        _advancedButton.onShortClick += OnShortClicked;
        _advancedButton.onHoldStart += OnHoldStarted;*/
        Grey(!canSelect);
    }
    
    void Grey(bool isGrey)
    {
        if (isGrey)
            GreyBlocker.SetActive(true);
        else
            GreyBlocker.SetActive(false);
    }

    private void OnShortClicked()
    {
        Debug.Log("OnShortClicked Fired!");
        if (_controller != null && _curUnitData != null)
        {
            _controller.OnCardSlotShortClick(_curUnitData, _canSelect);
        }
    }

    private void OnHoldStarted()
    {
        Debug.Log("OnHoldStarted Fired!");

        if (_controller != null && _curUnitData != null)
        {
            _controller.OnCardSlotHold(_curUnitData, _canSelect);
        }
    }

    private void OnHoldReleased()
    {
        if (_controller != null)
        {
            _controller.OnCardSlotHoldRelease();
        }
    }
}
