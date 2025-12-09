using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactUpgradePassivePopup : BasePopUpUI
{
    [Header("UI 참조")]
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private GameObject _slotPrefab;

    [Header("버튼")]
    [SerializeField] private Button _sortButton;
    [SerializeField] private Button _closeButton;

    private List<UIArtifactUpgradePassiveSlot> _slotList = new List<UIArtifactUpgradePassiveSlot> ();

    public event Action<int> OnArtifactSelected;
    public event Action OnRequestSort;
    public event Action OnRequestClose;

    protected override void Awake()
    {
        base.Awake();
        _sortButton.onClick.AddListener(OnSortButtonClicked);
        _closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    public void RefreshSlotList(List<PassiveSlotViewModel> viewModels)
    {
        for (int i = 0; i < viewModels.Count; i++)
        {
            UIArtifactUpgradePassiveSlot slot;

            if ( i >= _slotList.Count )
            {
                GameObject createdSlot = Instantiate( _slotPrefab, _slotContainer);
                slot = createdSlot.GetComponent<UIArtifactUpgradePassiveSlot>();
                slot.OnPassiveSlotClicked += SlotClicked;
                _slotList.Add(slot);
            }
            else
            {
                slot = _slotList[i];
            }

            slot.Init(viewModels[i]);
            slot.gameObject.SetActive(true);
        }

        for (int i = viewModels.Count; i < _slotList.Count; i++)
        {
            _slotList[i].gameObject.SetActive(false);
        }
    }

    public void OpenPassivePopup(List<PassiveSlotViewModel> viewModels)
    {
        RefreshSlotList(viewModels);
        OpenUI();
    }

    private void SlotClicked(int idNumber)
    {
        OnArtifactSelected?.Invoke(idNumber);
    }

    private void OnSortButtonClicked()
    {
        OnRequestSort?.Invoke();
    }

    private void OnCloseButtonClicked()
    {
        OnRequestClose?.Invoke();
    }
}
