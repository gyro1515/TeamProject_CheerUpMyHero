using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIArtifactUpgradePresenter
{
    #region 의존성
    private readonly PlayerDataManager _data;

    private readonly ArtifactService _service;
    private readonly ArtifactUpgradeService _upgradeService;

    private readonly UIArtifactUpgrade _mainView;
    private readonly UIArtifactUpgradePassivePopup _passivePopup;
    private readonly UIArtifactUpgradePassivePreview _passivePreview;
    private readonly UIArtifactUpgradeActivePanel _activePanel;

    // 의존성은 아님 : 선택된 패시브 아티팩트 id값 저장하는 변수
    private int _selectedPassiveIdNumber = -1;
    #endregion

    #region 생성자 + 이벤트 구독
    public UIArtifactUpgradePresenter(PlayerDataManager data,
                                      ArtifactService service,
                                      ArtifactUpgradeService upgradeService,
                                      UIArtifactUpgrade mainView)
    {
        _data = data;
        _service = service;
        _upgradeService = upgradeService;
        _mainView = mainView;

        _passivePopup = mainView.GetPassivePopup();
        _passivePreview = mainView.GetPassivePreview();
        _activePanel = mainView.GetActivePanel();

        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        _mainView.OnMaterialSlotClicked += HandleMaterialSlotClicked;
        _mainView.OnRequestUpgradePassive += HandleUpgradePassiveRequest;
        _mainView.OnRequestAutoEquip += HandleAutoEquipRequest;
        _mainView.OnRequestUnequipAll += HandleUnequipAllRequest;
        _mainView.OnRequestClose += HandleCloseRequest;
        _mainView.OnActiveSlotClicked += HandleActiveSlotClicked;

        _passivePopup.OnArtifactSelected += HandlePassiveArtifactSelected;
        _passivePopup.OnRequestSort += HandleSortRequest;
        _passivePopup.OnRequestClose += HandlePassivePopupCloseRequest;

        _passivePreview.OnConfirm += HandlePassivePreviewConfirm;

        _activePanel.OnRequestUpgrade += HandleActiveUpgradeRequest;
        _activePanel.OnRequestClose += HandleActivePanelCloseRequest;

        _data.OnArtifactOwnedChanged += HandleOwnedArtifactsChanged;
    }

    public void Dispose()
    {
        _mainView.OnMaterialSlotClicked -= HandleMaterialSlotClicked;
        _mainView.OnRequestUpgradePassive -= HandleUpgradePassiveRequest;
        _mainView.OnRequestAutoEquip -= HandleAutoEquipRequest;
        _mainView.OnRequestUnequipAll -= HandleUnequipAllRequest;
        _mainView.OnRequestClose -= HandleCloseRequest;
        _mainView.OnActiveSlotClicked -= HandleActiveSlotClicked;

        _passivePopup.OnArtifactSelected -= HandlePassiveArtifactSelected;
        _passivePopup.OnRequestSort -= HandleSortRequest;
        _passivePopup.OnRequestClose -= HandlePassivePopupCloseRequest;

        _passivePreview.OnConfirm -= HandlePassivePreviewConfirm;

        _activePanel.OnRequestUpgrade -= HandleActiveUpgradeRequest;
        _activePanel.OnRequestClose -= HandleActivePanelCloseRequest;

        _data.OnArtifactOwnedChanged -= HandleOwnedArtifactsChanged;
    }

    public void InitialDisplay()
    {
        _selectedPassiveIdNumber = -1;
        _mainView.ClearAllPassiveMaterialSlots();
        RefreshActiveSlotList();
    }
    #endregion

    #region Handle 메서드 - 메인 뷰
    private void HandleMaterialSlotClicked(int slotIndex)
    {
        List<PassiveSlotViewModel> viewModels = CreatePassiveSlotViewModels();
        _passivePopup.OpenPassivePopup(viewModels);
    }

    private void HandleUpgradePassiveRequest()
    {
        if (_selectedPassiveIdNumber <= 0) return;
        if (!_upgradeService.CanUpgradePassive(_selectedPassiveIdNumber)) return;

        PassivePreviewViewModel previewVm = CreatePassivePreviewViewModel();
        _passivePreview.OpenPassivePreview(previewVm);
    }

    private void HandleAutoEquipRequest()
    {
        List<int> upgradeableIds = new List<int>();
        HashSet<int> checkedIds = new HashSet<int>(); // 중복 체크 방지용

        foreach (var artifact in _data.OwnedArtifacts)
        {
            if (artifact is PassiveArtifactData passive)
            {
                // 이미 확인한 ID면 패스
                if (checkedIds.Contains(passive.idNumber)) continue;
                checkedIds.Add(passive.idNumber);

                // 강화 조건(3개 이상 보유 + 다음 등급 존재)을 만족하는지 확인
                if (_upgradeService.CanUpgradePassive(passive.idNumber))
                {
                    upgradeableIds.Add(passive.idNumber);
                }
            }
        }

        // 2. 후보가 있다면 그 중 하나를 무작위(또는 첫 번째)로 선택해 슬롯에 올립니다.
        if (upgradeableIds.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, upgradeableIds.Count);
            int targetId = upgradeableIds[randomIndex];

            HandlePassiveArtifactSelected(targetId);

            Debug.Log($"자동 선택됨: ID {targetId}");
        }
        else
        {
            Debug.Log("강화 가능한 패시브 유물이 없습니다.");
        }
    }

    private void HandleUnequipAllRequest()
    {
        _selectedPassiveIdNumber = -1;
        _mainView.ClearAllPassiveMaterialSlots();
    }

    private void HandleCloseRequest()
    {
        _mainView.CloseUI();
    }

    private void HandleActiveSlotClicked(ActiveArtifactData artifact)
    {
        ActiveUpgradeViewModel vm = CreateActiveUpgradeViewModel(artifact);
        _activePanel.OpenActivePanel(vm);
    }
    #endregion

    #region Handle 메서드 - 패시브 팝업
    private void HandlePassiveArtifactSelected(int idNumber)
    {
        _selectedPassiveIdNumber = idNumber;
        _passivePopup.CloseUI();

        List<PassiveMaterialSlotViewModel> materialVms = CreatePassiveMaterialSlotViewModels(idNumber);
        _mainView.RefreshPassiveMaterialSlots(materialVms);

        bool canUpgrade = _upgradeService.CanUpgradePassive(idNumber);
        _mainView.SetUpgradeButtonInteractable(canUpgrade);
    }

    private void HandleSortRequest()
    {
        _service.SortOwnedArtifacts();
        List<PassiveSlotViewModel> viewModels = CreatePassiveSlotViewModels();
        _passivePopup.RefreshSlotList(viewModels);
    }

    private void HandlePassivePopupCloseRequest()
    {
        _passivePopup.CloseUI();
    }
    #endregion

    #region Handle 메서드 - 패시브 프리뷰
    private void HandlePassivePreviewConfirm()
    {
        if (_selectedPassiveIdNumber <= 0) return;

        UpgradePassiveAsync().Forget();
    }

    private async UniTaskVoid UpgradePassiveAsync()
    {
        bool success = await _upgradeService.UpgradePassive(_selectedPassiveIdNumber);

        if (success)
        {
            _selectedPassiveIdNumber = -1;
            _mainView.ClearAllPassiveMaterialSlots();
        }
    }
    #endregion

    #region Handle 메서드 - 액티브 패널
    private void HandleActiveUpgradeRequest(ActiveArtifactData artifact)
    {
        UpgradeActiveAsync(artifact).Forget();
    }

    private async UniTaskVoid UpgradeActiveAsync(ActiveArtifactData artifact)
    {
        bool success = await _upgradeService.UpgradeActive(artifact);

        if (success)
        {
            ActiveUpgradeViewModel vm = CreateActiveUpgradeViewModel(artifact);
            _activePanel.RefreshUI(vm);
        }
    }

    private void HandleActivePanelCloseRequest()
    {
        _activePanel.CloseUI();
    }
    #endregion 

    #region Handle 메서드 - 데이터 변경
    private void HandleOwnedArtifactsChanged()
    {
        RefreshActiveSlotList();
    }
    #endregion

    #region ViewModel 생성 메서드
    // 인벤토리 같은 패시브 팝업 내의 슬롯에 전달할 뷰모델
    private List<PassiveSlotViewModel> CreatePassiveSlotViewModels()
    {
        List<PassiveSlotViewModel> viewModels = new List<PassiveSlotViewModel>();

        // 1. 소지 개수 카운트 (강화 가능 여부 판단용)
        Dictionary<int, int> countByIdNumber = new Dictionary<int, int>();
        for (int i = 0; i < _data.OwnedArtifacts.Count; i++)
        {
            ArtifactData artifact = _data.OwnedArtifacts[i];
            if (artifact is PassiveArtifactData passive)
            {
                if (countByIdNumber.ContainsKey(passive.idNumber))
                    countByIdNumber[passive.idNumber]++;
                else
                    countByIdNumber[passive.idNumber] = 1;
            }
        }

        // 2. 뷰모델 생성 (중복 체크 제거 -> 모든 유물 표시)
        for (int i = 0; i < _data.OwnedArtifacts.Count; i++)
        {
            ArtifactData artifact = _data.OwnedArtifacts[i];
            if (artifact is PassiveArtifactData passive)
            {
                // [삭제됨] if (addedIdNumbers.Contains(passive.idNumber)) continue;
                // 이제 중복을 건너뛰지 않고 모두 리스트에 담습니다.

                int count = countByIdNumber[passive.idNumber];

                // 강화 조건: 3개 이상 보유 & 다음 등급 존재
                bool canUpgrade = _upgradeService.CanUpgradePassive(passive.idNumber);

                // [수정] 강화 불가능하거나 전설 등급이면 선택 불가(어둡게) 처리
                // 버튼의 interactable 속성이 이 값(IsSelectable)을 따라갑니다.
                bool isSelectable = canUpgrade && (passive.grade != PassiveArtifactGrade.Legendary);

                PassiveSlotViewModel vm = new PassiveSlotViewModel()
                {
                    Artifact = passive,
                    OwnedCount = count,
                    CanUpgrade = canUpgrade,
                    IsSelectable = isSelectable, // 수정된 조건 적용
                    Icon = Resources.Load<Sprite>(passive.iconSpritePath)
                };

                viewModels.Add(vm);
            }
        }

        return viewModels;
    }

    // 업그레이드 메인 패널 패시브 유물 강화용 슬롯에 전달할 뷰모델
    private List<PassiveMaterialSlotViewModel> CreatePassiveMaterialSlotViewModels(int idNumber)
    {
        List<PassiveMaterialSlotViewModel> viewModels = new List<PassiveMaterialSlotViewModel>();
        const int materialSlotCount = 3;
        
        if (!DataManager.ArtifactData.TryGetValue(idNumber, out ArtifactData artifactData))
            return viewModels;

        if (artifactData is not PassiveArtifactData passive)
            return viewModels;

        int ownedCount = _upgradeService.GetSameArtifactCount(idNumber);
        int displayCount = Mathf.Min(ownedCount, materialSlotCount);

        Sprite icon = Resources.Load<Sprite>(passive.iconSpritePath);
        Color borderColor = GetGradeColor(passive.grade);

        for (int i = 0; i < materialSlotCount; i++)
        {
            PassiveMaterialSlotViewModel vm = new PassiveMaterialSlotViewModel
            {
                IsFilled = i < displayCount,
                Icon = icon,
                BorderColor = borderColor,
            };

            viewModels.Add(vm);
        }

        return viewModels;
    }

    // 패시브 유물 업그레이드 확인 창 전달용 뷰모델
    private PassivePreviewViewModel CreatePassivePreviewViewModel()
    {
        PassivePreviewViewModel vm = new PassivePreviewViewModel();

        if (!DataManager.ArtifactData.TryGetValue(_selectedPassiveIdNumber, out ArtifactData artifactData))
            return vm;

        if (artifactData is not PassiveArtifactData source)
            return vm;

        PassiveArtifactData result = _upgradeService.GetNextPassiveArtifact(source);
        if (result == null) return vm;

        vm.SourceArtifact = source;
        vm.SourceIcon = Resources.Load<Sprite>(source.iconSpritePath);
        vm.SourceBorderColor = GetGradeColor(source.grade);
        vm.SourceEffectText = source.description;
        
        vm.ResultArtifact = result;
        vm.ResultIcon = Resources.Load<Sprite>(result.iconSpritePath);
        vm.ResultBorderColor = GetGradeColor(result.grade);
        vm.ResultEffectText = result.description;

        return vm;
    }

    // 액티브 유물 슬롯 전달용 뷰모델
    private List<ActiveSlotViewModel> CreateActiveSlotViewModels()
    {
        List<ActiveSlotViewModel> viewModels = new List<ActiveSlotViewModel>();

        for (int i = 0; i < _data.OwnedArtifacts.Count; i++)
        {
            ArtifactData artifact = _data.OwnedArtifacts[i];
            if (artifact is ActiveArtifactData active)
            {
                ActiveSlotViewModel vm = new ActiveSlotViewModel
                {
                    Artifact = active,
                    Icon = Resources.Load<Sprite>(active.iconSpritePath),
                    NameText = active.name,
                    LevelText = $"Lv. {active.curLevel + 1}"
                };

                viewModels.Add(vm);
            }
        }

        return viewModels;
    }

    private ActiveUpgradeViewModel CreateActiveUpgradeViewModel(ActiveArtifactData artifact)
    {
        ActiveUpgradeViewModel vm = new ActiveUpgradeViewModel();

        if (artifact == null) return vm;

        vm.Artifact = artifact;
        vm.Icon = Resources.Load<Sprite>(artifact.iconSpritePath);
        vm.CanUpgrade = _upgradeService.CanUpgradeActive(artifact);

        int maxLevel = artifact.levelData.Count - 1;
        vm.IsMaxLevel = artifact.curLevel >= maxLevel;

        vm.CurrentLevelText = $"Lv. {artifact.curLevel + 1}";
        vm.CurrentEffectText = CreateActiveEffectText(artifact, artifact.curLevel);

        if (!vm.IsMaxLevel)
        {
            vm.NextLevelText = $"Lv. {artifact.curLevel + 2}";
            vm.NextEffectText = CreateActiveEffectText(artifact, artifact.curLevel + 1);
        }
        else
        {
            vm.NextLevelText = "MAX LV";
            vm.NextEffectText = "";
        }

        Dictionary<ResourceType, int> cost = _upgradeService.GetActiveUpgradeCost(artifact);
        Dictionary<ResourceType, bool> sufficiency = _upgradeService.GetResourceSufficiency(artifact);

        int currentGold = _data.GetResourceAmount(ResourceType.Gold);
        int currentWood = _data.GetResourceAmount(ResourceType.Wood);
        int currentIron = _data.GetResourceAmount(ResourceType.Iron);
        int currentMagicStone = _data.GetResourceAmount(ResourceType.MagicStone);

        if (cost.ContainsKey(ResourceType.Gold))
        {
            vm.GoldCostText = $"{currentGold}/{cost[ResourceType.Gold]}";
            vm.HasEnoughGold = sufficiency[ResourceType.Gold];
        }

        if (cost.ContainsKey(ResourceType.Wood))
        {
            vm.WoodCostText = $"{currentWood}/{cost[ResourceType.Wood]}";
            vm.HasEnoughWood = sufficiency[ResourceType.Wood];
        }

        if (cost.ContainsKey(ResourceType.Iron))
        {
            vm.IronCostText = $"{currentIron}/{cost[ResourceType.Iron]}";
            vm.HasEnoughIron = sufficiency[ResourceType.Iron];
        }

        if (cost.ContainsKey(ResourceType.MagicStone))
        {
            vm.MagicStoneCostText = $"{currentMagicStone}/{cost[ResourceType.MagicStone]}";
            vm.HasEnoughMagicStone = sufficiency[ResourceType.MagicStone];
        }

        return vm;
    }

    private void RefreshActiveSlotList()
    {
        List<ActiveSlotViewModel> viewModels = CreateActiveSlotViewModels();
        _mainView.RefreshActiveSlotList(viewModels);
    }
    #endregion

    #region 헬퍼 메서드
    private string CreatePassiveEffectText(PassiveArtifactData artifact)
    {
        string targetText = GetEffectTargetText(artifact.effectTarget);
        string statTypeText = GetStatTypeText(artifact.statType);
        return $"{targetText}의 {statTypeText} {artifact.value}% 증가";
    }

    private string CreateActiveEffectText(ActiveArtifactData active, int level)
    {
        if (level < 0 || level >= active.levelData.Count) return "";

        ActiveArtifactLevelData levelData = active.levelData[level];
        string typeText = active.type;
        string bonusPercentText = "";
        string coolTimeText = levelData.coolTime.ToString();

        return $"주 효과 : {typeText}\n" +
               $"계수 : {bonusPercentText}\n" +
               $"재사용 대기시간 : {coolTimeText}";
    }

    private string GetEffectTargetText(EffectTarget target)
    {
        switch (target)
        {
            case EffectTarget.Player:
                return "플레이어";
            case EffectTarget.MeleeUnit:
                return "근거리 유닛";
            case EffectTarget.RangedUnit:
                return "원거리 유닛";
            default:
                return target.ToString();
        }
    }

    private string GetStatTypeText(StatType type)
    {
        switch (type)
        {
            case StatType.MaxHp:
                return "체력";
            case StatType.AtkPower:
                return "공격력";
            case StatType.MoveSpeed:
                return "이동 속도";
            case StatType.AuraRange:
                return "오라 범위";
            default:
                return type.ToString();
        }
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
