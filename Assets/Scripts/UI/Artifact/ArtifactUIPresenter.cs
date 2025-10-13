using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArtifactUIPresenter
{
    private readonly ArtifactManager _model;
    private readonly UIArtifact _mainView;
    private readonly UIArtifactInventoryPanel _inventoryPanelView;
    private readonly UIArtifactEquipPanel _equipPanelView;
    private readonly UIArtifactStatPanel _statPanelView;

    public ArtifactUIPresenter(ArtifactManager model,
                               UIArtifact mainView,
                               UIArtifactInventoryPanel inventoryPanelView,
                               UIArtifactEquipPanel equipPanelView,
                               UIArtifactStatPanel statPanelView)
    {
        _model = model;
        _mainView = mainView;
        _inventoryPanelView = inventoryPanelView;
        _equipPanelView = equipPanelView;
        _statPanelView = statPanelView;

        _model.OnEquippedArtifactChanged += HandleEquippedArtifactChanged;
        _model.OnOwnedArtifactsChanged += HandleOwnedArtifactsChanged;

        _mainView.OnRequestAutoEquip += HandleAutoEquipRequest;

        _inventoryPanelView.OnRequestEquip += HandleEquipRequest;
        _inventoryPanelView.OnRequestUnEquip += HandleUnEquipRequest;
        _inventoryPanelView.OnRequestSort += HandleSortRequest;
        _inventoryPanelView.OnRequestClose += () => _inventoryPanelView.gameObject.SetActive(false);
        _inventoryPanelView.OnRequestSelectArtifact += HandleSelectArtifactRequest;

        _equipPanelView.OnslotsInitialize += HandleEquipSlotsInitiaized;
    }

    public void Dispose()
    {
        _model.OnEquippedArtifactChanged -= HandleEquippedArtifactChanged;
        _model.OnOwnedArtifactsChanged -= HandleOwnedArtifactsChanged;

        _mainView.OnRequestAutoEquip -= HandleAutoEquipRequest;

        _inventoryPanelView.OnRequestEquip -= HandleEquipRequest;
        _inventoryPanelView.OnRequestUnEquip -= HandleUnEquipRequest;
        _inventoryPanelView.OnRequestSort -= HandleSortRequest;
        _inventoryPanelView.OnRequestClose -= () => _inventoryPanelView.gameObject.SetActive(false);
        _inventoryPanelView.OnRequestSelectArtifact -= HandleSelectArtifactRequest;

        _equipPanelView.OnslotsInitialize -= HandleEquipSlotsInitiaized;

        foreach (var slot in _equipPanelView.GetSlots())
        {
            slot.OnRequestOpenInventory -= HandleInventoryOpenRequest;
        }
    }

    public void InitialDisplay()
    {
        HandleEquippedArtifactChanged();
    }

    #region Handle 메서드
    public void HandleEquippedArtifactChanged()
    {
        _statPanelView.RefreshArtifactStatUI();

        List<UIArtifactEquipSlot> slots = _equipPanelView.GetSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            ArtifactData artifact = _model.EquippedArtifacts[i];
            EquipSlotViewModel vm = CreateEquipSlotViewModel(artifact);
            slots[i].RefreshArtifactEquipSlotDisplay(vm);
        }
    }

    private void HandleOwnedArtifactsChanged()
    {
        List<ArtifactData> sortedList = _model.OwnedArtifacts;

        List<InventorySlotViewModel> viewModels = sortedList.Select(artifact =>
                                                  CreateInventorySlotViewModel(artifact)).ToList();

        _inventoryPanelView.RefreshArtifactInventoryUI(viewModels);
    }

    private void HandleEquipRequest(ArtifactData artifact, int slotIndex)
    {
        _model.EquipArtifact(artifact, slotIndex);
        _inventoryPanelView.gameObject.SetActive(false);
    }

    private void HandleUnEquipRequest(ArtifactData artifact)
    {
        for (int i = 0; i < _model.EquippedArtifacts.Count; i++)
        {
            if (_model.EquippedArtifacts[i] == artifact)
            {
                _model.UnEquipArtifact(i);
                break;
            }
        }
    }

    private void HandleEquipSlotsInitiaized()
    {
        foreach (var slot in _equipPanelView.GetSlots())
        {
            slot.OnRequestOpenInventory += HandleInventoryOpenRequest;
        }
    }

    private void HandleInventoryOpenRequest(int slotIndex)
    {
        List<ArtifactData> ownedList = _model.OwnedArtifacts;
        ArtifactData equippedInCurrentSlot = _model.EquippedArtifacts[slotIndex];

        List<InventorySlotViewModel> viewModels = ownedList.Select(artifact => 
                                                  CreateInventorySlotViewModel(artifact, equippedInCurrentSlot)).ToList();

        _inventoryPanelView.OpenInventory(slotIndex, viewModels);
    }

    private void HandleSelectArtifactRequest(ArtifactData selectedArtifact)
    {
        DescriptionViewModel vm = CreateDescriptionViewModel(selectedArtifact);
        _inventoryPanelView.UpdateDescriptionPanel(vm);
    }

    private void HandleSortRequest()
    {
        _model.SortOwnedArtifacts();
    }

    private void HandleAutoEquipRequest(ArtifactType type)
    {
        _model.AutoEquipArtifacts(type);
    }
    #endregion

    // 헬퍼 메서드 : 데이터를 UI에 주기 편한 상태로 가공하는 역할을 함
    #region ViewModel 헬퍼 메서드
    private EquipSlotViewModel CreateEquipSlotViewModel(ArtifactData artifact)
    {
        if (artifact == null) return new EquipSlotViewModel { Name = null };

        EquipSlotViewModel vm = new EquipSlotViewModel { Name = artifact.name };
        if (artifact is PassiveArtifactData p)
        {
            vm.StatType = p.statType.ToString();
            vm.StatValue = p.value.ToString();

            switch (p.grade)
            {
                case PassiveArtifactGrade.Common:
                    vm.BorderColor = Color.gray;
                    break;
                case PassiveArtifactGrade.Rare:
                    vm.BorderColor = Color.blue;
                    break;
                case PassiveArtifactGrade.Epic:
                    vm.BorderColor = Color.magenta;
                    break;
                case PassiveArtifactGrade.Unique:
                    vm.BorderColor = Color.yellow;
                    break;
                case PassiveArtifactGrade.Legendary:
                    vm.BorderColor = Color.green;
                    break;
                default:
                    vm.BorderColor = Color.black;
                    break;
            }
        }
        else if (artifact is ActiveArtifactData a)
        {
            vm.StatType = $"Lv. {a.levelData[a.curLevel].level}";
            vm.StatValue = $"Cost : {a.cost}";
            vm.BorderColor = Color.green;
        }

        return vm;
    }

    private InventorySlotViewModel CreateInventorySlotViewModel(ArtifactData artifact, ArtifactData equippedInCurrentSlot = null)
    {
        InventorySlotViewModel vm = new InventorySlotViewModel
        {
            Artifact = artifact,
            Name = artifact.name,
            IsEquippedInCurrentSlot = (artifact == equippedInCurrentSlot)
        };

        if (artifact is PassiveArtifactData p)
        {
            vm.StatType = p.statType.ToString();
            vm.StatValue = p.value.ToString();

            switch (p.grade)
            {
                case PassiveArtifactGrade.Common:
                    vm.BorderColor = Color.gray;
                    break;
                case PassiveArtifactGrade.Rare:
                    vm.BorderColor = Color.blue;
                    break;
                case PassiveArtifactGrade.Epic:
                    vm.BorderColor = Color.magenta;
                    break;
                case PassiveArtifactGrade.Unique:
                    vm.BorderColor = Color.yellow;
                    break;
                case PassiveArtifactGrade.Legendary:
                    vm.BorderColor = Color.green;
                    break;
                default:
                    vm.BorderColor = Color.black;
                    break;
            }
        }
        else if (artifact is ActiveArtifactData a)
        {
            vm.StatType = $"Lv. {a.levelData[a.curLevel].level}";
            vm.StatValue = $"Cost : {a.cost}";
            vm.BorderColor = Color.green;
        }

        return vm;
    }

    private DescriptionViewModel CreateDescriptionViewModel(ArtifactData artifact)
    {
        if (artifact == null) return new DescriptionViewModel { IsPanelActive = false };

        DescriptionViewModel vm = new DescriptionViewModel
        {
            IsPanelActive = true,
            ArtifactData = artifact,
            IsEquipped = _model.EquippedArtifacts.Contains(artifact)
        };

        if (artifact is PassiveArtifactData p)
        {
            vm.GradeOrLevelText = $"등급 : {p.grade}";
            vm.StatTypeText = $"스탯 타입 : {p.statType}";
            vm.ValueOrCostText = $"효과 : + {p.value}%";
        }
        else if (artifact is ActiveArtifactData a)
        {
            vm.GradeOrLevelText = $"Lv. {a.levelData[a.curLevel].level}";
            vm.StatTypeText = $"유형 : {a.type}";
            vm.ValueOrCostText = $"Cost : {a.cost}";
        }

        return vm;
    }
    #endregion
}
