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

    private void Start()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClicked);
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
        Grey(!canSelect);
    }
    
    void Grey(bool isGrey)
    {
        if (isGrey)
            GreyBlocker.SetActive(true);
        else
            GreyBlocker.SetActive(false);
    }

    private void OnClicked()
    {
        if (_controller != null && _curUnitData != null)
        {
            _controller.OnCardSlotClicked(_curUnitData, _canSelect);
        }
    }
}
