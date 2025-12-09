using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactUpgradeMaterialSlot : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Outline _iconOutline;

    private Button _button;
    private Outline _slotOutline;
    private int _slotIndex;

    public event Action<int> OnSlotClicked;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnButtonClicked);
        _slotOutline = GetComponent<Outline>();
    }

    public void Init(int slotIndex)
    {
        _slotIndex = slotIndex;
    }

    public void RefreshMaterialSlot(PassiveMaterialSlotViewModel vm)
    {
        if (vm.IsFilled)
        {
            _icon.sprite = vm.Icon;
            _icon.color = Color.white;
            _iconOutline.effectColor = vm.BorderColor;
            _slotOutline.effectColor = vm.BorderColor;
        }
        else
        {
            ClearMaterialSlots();
        }
    }

    public void ClearMaterialSlots()
    {
        _icon.sprite = null;
        _icon.color = Color.clear;
        _iconOutline.effectColor = Color.gray;
        _slotOutline.effectColor = Color.gray;
    }

    private void OnButtonClicked()
    {
        OnSlotClicked?.Invoke(_slotIndex);
    }
}
