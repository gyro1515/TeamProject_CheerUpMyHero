using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIArtifactEquipPanel : MonoBehaviour
{
    [Header("슬롯 리스트")]
    [SerializeField] private List<UIArtifactEquipSlot> _slots;

    [Header("인벤토리")]
    [SerializeField] private UIArtifactInventoryPanel _inventory;

    private void Awake()
    {
        InitializeSlots();
    }

    private void OnDisable()
    {
        
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].Init(i, _inventory);
        }
    }

    public void RefreshAllArtifactEquipSlotDisplay()
    {
        foreach (var slot in _slots)
        {
            slot.RefreshArtifactEquipSlotDisplay();
        }
    }
}
