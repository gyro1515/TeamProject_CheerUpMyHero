using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifactUpgrade : BaseUI, IBackButtonHandler
{
    #region UI 참조 + 변수
    private const int DefaultPassiveMaterialSlotCount = 3;

    [Header("패시브 합성 재료 슬롯 개수")]
    [SerializeField] private int _materialSlotCount = DefaultPassiveMaterialSlotCount;

    [Header("패시브 합성 재료 슬롯")]
    [SerializeField] private UIArtifactUpgradeMaterialSlot[] _materialSlots;

    [Header("액티브 유물 목록")]
    [SerializeField] private Transform _activeSlotContainer;
    [SerializeField] private GameObject _activeSlotPrefab;

    [Header("패널")]
    [SerializeField] private UIArtifactUpgradePassivePopup _passivePopup;
    [SerializeField] private UIArtifactUpgradePassivePreview _passivePreview;
    [SerializeField] private UIArtifactUpgradeActivePanel _activePanel;

    [Header("버튼")]
    [SerializeField] private Button _passiveUpgradeButton;
    [SerializeField] private Button _autoEquipButton;
    [SerializeField] private Button _unequipAllButton;
    [SerializeField] private Button _closeButton;

    private List<UIArtifactUpgradeActiveSlot> _activeSlotList = new List<UIArtifactUpgradeActiveSlot>();

    private ArtifactService _service;
    private ArtifactUpgradeService _upgradeService;
    private UIArtifactUpgradePresenter _presenter;
    #endregion

    #region 이벤트 시스템
    public event Action<int> OnMaterialSlotClicked;
    public event Action OnRequestUpgradePassive;
    public event Action OnRequestAutoEquip;
    public event Action OnRequestUnequipAll;
    public event Action OnRequestClose;
    public event Action<ActiveArtifactData> OnActiveSlotClicked;
    #endregion

    #region 생명주기

    private void Start()
    {
        _service = new ArtifactService(PlayerDataManager.Instance);
        _upgradeService = new ArtifactUpgradeService(PlayerDataManager.Instance, _service);
        _presenter = new UIArtifactUpgradePresenter(PlayerDataManager.Instance,
                                                    _service,
                                                    _upgradeService,
                                                    this);

        _passiveUpgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        _autoEquipButton.onClick.AddListener(OnAutoEquipButtonClicked);
        _unequipAllButton.onClick.AddListener(OnUnequipAllButtonClicked);
        _closeButton.onClick.AddListener(OnCloseButtonClicked);

        InitializePassiveMaterialSlots();

        _presenter.InitialDisplay();
    }

    private void OnEnable()
    {
        UIManager.PubishAddUIStackEvent(this);

        _presenter?.InitialDisplay();
    }

    private void OnDisable()
    {
        UIManager.PublishRemoveUIStackEvent();
    }

    private void OnDestroy()
    {
        _presenter?.Dispose();
    }
    #endregion

    #region 패시브 합성 메서드
    private void InitializePassiveMaterialSlots()
    {
        for (int i = 0; i < _materialSlotCount; i++)
        {
            _materialSlots[i].Init(i);
            _materialSlots[i].OnSlotClicked += OnMaterialSlotButtonClicked;
        }
    }

    public void RefreshPassiveMaterialSlots(List<PassiveMaterialSlotViewModel> vm)
    {
        for (int i = 0; i < _materialSlotCount; i++)
        {
            if (i < vm.Count)
            {
                _materialSlots[i].RefreshMaterialSlot(vm[i]);
            }
            else
            {
                _materialSlots[i].ClearMaterialSlots();
            }
        }
    }

    public void ClearAllPassiveMaterialSlots()
    {
        for (int i = 0; i < _materialSlotCount; i++)
        {
            _materialSlots[i].ClearMaterialSlots();
        }

        _passiveUpgradeButton.interactable = false;
    }

    public void SetUpgradeButtonInteractable(bool interactable)
    {
        _passiveUpgradeButton.interactable = interactable;
    }
    #endregion

    #region 액티브 슬롯 메서드
    public void RefreshActiveSlotList(List<ActiveSlotViewModel> vm)
    {
        for (int i = 0; i < vm.Count; i++)
        {
            UIArtifactUpgradeActiveSlot slot;

            if (i >= _activeSlotList.Count)
            {
                GameObject createdSlot = Instantiate(_activeSlotPrefab, _activeSlotContainer);
                slot = createdSlot.GetComponent<UIArtifactUpgradeActiveSlot>();
                slot.OnActiveSlotClicked += OnActiveSlotButtonClicked;
                _activeSlotList.Add(slot);
            }
            else
            {
                slot = _activeSlotList[i];
            }

            slot.Init(vm[i]);
            slot.gameObject.SetActive(true);
        }

        for (int i = vm.Count; i < _activeSlotList.Count; i++)
        {
            _activeSlotList[i].gameObject.SetActive(false);
        }
    }
    #endregion

    #region 패널 반환 메서드 
    // presenter에서 쓰는 패널 반환하는 메서드

    public UIArtifactUpgradePassivePopup GetPassivePopup()
    {
        return _passivePopup;
    }

    public UIArtifactUpgradePassivePreview GetPassivePreview()
    {
        return _passivePreview;
    }

    public UIArtifactUpgradeActivePanel GetActivePanel()
    {
        return _activePanel;
    }
    #endregion

    #region 이벤트 메서드
    private void OnMaterialSlotButtonClicked(int SlotIndex)
    {
        OnMaterialSlotClicked?.Invoke(SlotIndex);
    }

    private void OnUpgradeButtonClicked()
    {
        OnRequestUpgradePassive?.Invoke();
    }

    private void OnAutoEquipButtonClicked()
    {
        OnRequestAutoEquip?.Invoke();
    }

    private void OnUnequipAllButtonClicked()
    {
        OnRequestUnequipAll?.Invoke();
    }

    private void OnCloseButtonClicked()
    {
        var mainScreen = UIManager.Instance.GetUI<MainScreenUI>();

        if (mainScreen != null && UIManager.Instance.fromUI == FromUI.MainScreen)
        {
            FadeManager.Instance.SwitchGameObjects(gameObject, mainScreen.gameObject);
            UIManager.Instance.fromUI = FromUI.MainScreen;
        }
    }

    private void OnActiveSlotButtonClicked(ActiveArtifactData artifact)
    {
        OnActiveSlotClicked?.Invoke(artifact);
    }
    #endregion

    #region 뒤로가기 로직
    public void OnBackPressed()
    {
        Debug.Log($"{gameObject.name} 뒤로가기 버튼 눌림");
        OnCloseButtonClicked();
    }
    #endregion
}
