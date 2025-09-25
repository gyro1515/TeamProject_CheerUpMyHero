using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactEquipSlot : MonoBehaviour
{
    [Header("유물 슬롯 타입 : 플 / 원 / 근")]
    [SerializeField] private EffectTarget _target;

    [Header("인벤토리 UI")]
    [SerializeField] private UIPassiveArtifactInventory _inventory;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnSlotClicked);
    }

    private void OnSlotClicked()
    {
        if (_inventory != null)
        {
            _inventory.OpenInventory(_target);
        }
    }
}
