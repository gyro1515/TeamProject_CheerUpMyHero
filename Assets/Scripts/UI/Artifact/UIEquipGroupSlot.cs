using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEquipGroupSlot : MonoBehaviour
{
    [Header("슬롯 정보")]
    [SerializeField] private EffectTarget _targerType;

    [Header("슬롯 리스트")]
    [SerializeField] private List<UIArtifactEquipSlot> _slots;

    [Header("인벤토리")]
    [SerializeField] private UIPassiveArtifactInventory _inventory;

    private void Awake()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].Init(_targerType, i, _inventory);
        }
    }
}
