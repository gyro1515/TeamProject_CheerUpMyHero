using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArtifactUIPresenter
{
    #region 아티팩트 UI 요소 
    private readonly ArtifactManager _model;
    private readonly UIArtifact _mainView;
    private readonly UIArtifactInventoryPanel _inventoryPanelView;
    private readonly UIArtifactEquipPanel _equipPanelView;
    private readonly UIArtifactStatPanel _statPanelView;
    #endregion

    #region 생성자 + 구독 해제 메서드
    public void InitialDisplay()    // 유물 선택 창 켜졌을 때 활성화해주는 함수
    {
        HandleEquippedArtifactChanged();
    }

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
        _mainView.OnRequestUnEquipAll += HandleUnEquipAllRequest;

        _inventoryPanelView.OnRequestEquip += HandleEquipRequest;
        _inventoryPanelView.OnRequestUnEquip += HandleUnEquipRequest;
        _inventoryPanelView.OnRequestSort += HandleSortRequest;
        _inventoryPanelView.OnRequestClose += HandleInventoryCloseRequest;
        _inventoryPanelView.OnRequestSelectArtifact += HandleSelectArtifactRequest;

        _equipPanelView.OnslotsInitialize += HandleEquipSlotsInitiaized;
    }

    public void Dispose()
    {
        _model.OnEquippedArtifactChanged -= HandleEquippedArtifactChanged;
        _model.OnOwnedArtifactsChanged -= HandleOwnedArtifactsChanged;

        _mainView.OnRequestAutoEquip -= HandleAutoEquipRequest;
        _mainView.OnRequestUnEquipAll -= HandleUnEquipAllRequest;

        _inventoryPanelView.OnRequestEquip -= HandleEquipRequest;
        _inventoryPanelView.OnRequestUnEquip -= HandleUnEquipRequest;
        _inventoryPanelView.OnRequestSort -= HandleSortRequest;
        _inventoryPanelView.OnRequestClose -= HandleInventoryCloseRequest;
        _inventoryPanelView.OnRequestSelectArtifact -= HandleSelectArtifactRequest;

        _equipPanelView.OnslotsInitialize -= HandleEquipSlotsInitiaized;

        foreach (var slot in _equipPanelView.GetSlots())
        {
            slot.OnRequestOpenInventory -= HandleInventoryOpenRequest;
        }
    }
    #endregion

    #region Handle 메서드 : 유물 데이터 변경
    public void HandleEquippedArtifactChanged()
    {
        if (_model == null || _model.EquippedArtifacts == null) return;

        // 각 스탯 종류별 뷰모델 만들어서 뷰모델로 이루어진 뷰모델 덩어리 만듬
        StatPanelViewModel statVM = new StatPanelViewModel
        {
            PlayerAtk = CreateStatBarViewModel(EffectTarget.Player, StatType.AtkPower),
            PlayerHp = CreateStatBarViewModel(EffectTarget.Player, StatType.MaxHp),
            PlayerSpd = CreateStatBarViewModel(EffectTarget.Player, StatType.MoveSpeed),
            PlayerAura = CreateStatBarViewModel(EffectTarget.Player, StatType.AuraRange),
            MeleeAtk = CreateStatBarViewModel(EffectTarget.MeleeUnit, StatType.AtkPower),
            MeleeHp = CreateStatBarViewModel(EffectTarget.MeleeUnit, StatType.MaxHp),
            RangedAtk = CreateStatBarViewModel(EffectTarget.RangedUnit, StatType.AtkPower),
            RangedHp = CreateStatBarViewModel(EffectTarget.RangedUnit, StatType.MaxHp)
        };

        _statPanelView.RefreshStatPanelUI(statVM);

        List<UIArtifactEquipSlot> slots = _equipPanelView.GetSlots();

        if (slots == null || slots.Count == 0) return;

        for (int i = 0; i < slots.Count; i++)
        {
            if (1 < _model.EquippedArtifacts.Count)
            {
                ArtifactData artifact = _model.EquippedArtifacts[i];
                EquipSlotViewModel vm = CreateEquipSlotViewModel(artifact);
                slots[i].RefreshArtifactEquipSlotDisplay(vm);
            }
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
        _inventoryPanelView.CloseUI();
    }

    private void HandleUnEquipRequest(ArtifactData artifact)
    {
        for (int i = 0; i < _model.EquippedArtifacts.Count; i++)
        {
            if (_model.EquippedArtifacts[i] == artifact)
            {
                _model.UnEquipArtifact(i);
                _inventoryPanelView.CloseUI();
                break;
            }
        }
    }
    #endregion

    #region Handle 메서드 : UIArtifactShow 관련
    private void HandleEquipSlotsInitiaized()
    {
        foreach (var slot in _equipPanelView.GetSlots())
        {
            slot.OnRequestOpenInventory += HandleInventoryOpenRequest;
        }
    }

    private void HandleAutoEquipRequest(ArtifactType type)
    {
        _model.AutoEquipArtifacts(type);
    }

    private void HandleUnEquipAllRequest()
    {
        _model.UnEquipAllArtifacts();
    }
    #endregion

    #region Handle 메서드 : UIArtifactInventoryPanel 관련
    private void HandleInventoryOpenRequest(int slotIndex)
    {
        List<ArtifactData> ownedList = _model.OwnedArtifacts;

        if (slotIndex < 0 || slotIndex >= _model.EquippedArtifacts.Count)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}. List count: {_model.EquippedArtifacts.Count}");
            return;
        }
        ArtifactData equippedInCurrentSlot = _model.EquippedArtifacts[slotIndex];

        List<InventorySlotViewModel> viewModels = ownedList.Select(artifact => 
                                                  CreateInventorySlotViewModel(artifact, equippedInCurrentSlot)).ToList();

        // ↑ 위의 모델에서 뷰모델 만들어서, ↓ 인벤토리 여는 메서드 호출함
        _inventoryPanelView.OpenInventory(slotIndex, viewModels);
    }

    private void HandleInventoryCloseRequest()
    {
        _inventoryPanelView.CloseUI();
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
    #endregion

    // 헬퍼 메서드 : 데이터를 UI에 주기 편한 상태로 가공하는 역할을 함
    #region ViewModel 헬퍼 메서드
    private EquipSlotViewModel CreateEquipSlotViewModel(ArtifactData artifact)
    {
        if (artifact == null) return new EquipSlotViewModel { Name = null };

        EquipSlotViewModel vm = new EquipSlotViewModel 
        { 
            Name = artifact.name,
            Icon = Resources.Load<Sprite>(artifact.iconSpritePath)
        };
        if (artifact is PassiveArtifactData p)
        {
            vm.StatType = p.statType.ToString();
            vm.StatValue = p.value.ToString();
            vm.BorderColor = GetGradeColor(p.grade);
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
            Icon = Resources.Load<Sprite>(artifact.iconSpritePath),

            IsEquippedInCurrentSlot = (artifact == equippedInCurrentSlot)
        };

        if (artifact is PassiveArtifactData p)
        {
            vm.StatType = p.statType.ToString();
            vm.StatValue = p.value.ToString();
            vm.BorderColor = GetGradeColor(p.grade);
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
            Icon = Resources.Load<Sprite>(artifact.iconSpritePath),

            IsEquipped = _model.EquippedArtifacts.Contains(artifact)
        };

        if (artifact is PassiveArtifactData p)
        {
            vm.GradeOrLevelText = $"등급 : {p.grade}";
            vm.StatTypeText = $"스탯 타입 : {p.statType}";
            vm.ValueOrCostText = $"효과 : + {p.value}%";
            vm.BorderColor = GetGradeColor(p.grade);
        }
        else if (artifact is ActiveArtifactData a)
        {
            vm.GradeOrLevelText = $"Lv. {a.levelData[a.curLevel].level}";
            vm.StatTypeText = $"유형 : {a.type}";
            vm.ValueOrCostText = $"Cost : {a.cost}";
            vm.BorderColor = GetGradeColor(PassiveArtifactGrade.Legendary);
        }
        return vm;
    }

    private StatBarViewModel CreateStatBarViewModel(EffectTarget target, StatType type)
    {
        List<PassiveArtifactData> artifacts = _model.EquippedArtifacts.OfType<PassiveArtifactData>()
                                                    .Where(p => p.effectTarget == target && p.statType == type)
                                                    .OrderByDescending(p => p.grade)
                                                    .ToList();

        StatBarViewModel barVm = new StatBarViewModel
        {
            Bonus = artifacts.Sum(p => p.value),
            SegmentColors = artifacts.Select(p => GetGradeColor(p.grade)).ToList()
        };
        return barVm;
    }

    private Color GetGradeColor(PassiveArtifactGrade grade)
    {
        switch (grade)
        {
            case PassiveArtifactGrade.Common:
                return Color.gray;

            case PassiveArtifactGrade.Rare:
                return Color.blue;

            case PassiveArtifactGrade.Epic:
                return Color.magenta;

            case PassiveArtifactGrade.Unique:
                return Color.yellow;

            case PassiveArtifactGrade.Legendary:
                return Color.green;

            default:
                return Color.black;
        }
    }
    #endregion
}
