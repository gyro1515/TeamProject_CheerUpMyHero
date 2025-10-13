using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIArtifactEquipPanel : MonoBehaviour
{
    [Header("슬롯 프리펩")]
    [SerializeField] private GameObject _equipSlotPrefab;
    [SerializeField] private Transform _createPosition;

    private List<UIArtifactEquipSlot> _slots = new List<UIArtifactEquipSlot>();

    public event Action OnslotsInitialize;

    private void Start()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        foreach (UIArtifactEquipSlot slot in _slots)
        {
            Destroy(slot.gameObject);
        }
        _slots.Clear();

        for (int i = 0; i < 8; i++)
        {
            GameObject slot = Instantiate(_equipSlotPrefab, _createPosition);
            UIArtifactEquipSlot newSlot = slot.GetComponent<UIArtifactEquipSlot>();

            newSlot.Init(i);
            _slots.Add(newSlot);
        }

        OnslotsInitialize?.Invoke();
    }

    public List<UIArtifactEquipSlot> GetSlots()
    {
        return _slots;
    }
}
