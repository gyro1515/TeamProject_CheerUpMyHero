using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class UIArtifact : BaseUI, IBackButtonHandler
{
    #region UI요소 참조 변수
    [Header("UI 요소 참조")]
    [SerializeField] private UIArtifactEquipPanel _equipPanel;
    [SerializeField] private UIArtifactInventoryPanel _inventoryPanel;
    [SerializeField] private UIArtifactStatPanel _statPanel;

    [Header("UI간 이동 버튼")]
    [SerializeField] private Button _gotoCardDeckButton;

    private CanvasGroup _canvasGroup;

    private ArtifactUIPresenter _presenter;

    private bool _isInitialized = false;
    #endregion

    #region 이벤트 시스템
    public event Action<ArtifactType> OnRequestAutoEquip;
    public event Action OnRequestUnEquipAll;
    #endregion
    
    #region 생명주기
    private void Awake()
    {
        _presenter = new ArtifactUIPresenter(ArtifactManager.Instance,
                                             this,
                                             _inventoryPanel,
                                             _equipPanel,
                                             _statPanel);

        _canvasGroup = GetComponent<CanvasGroup>();

        _passiveEquipButton.onClick.AddListener(() =>
        {
            OnRequestAutoEquip?.Invoke(ArtifactType.Passive);
            UpdateSelectionUI(ArtifactType.Passive);
        });
        _activeEquipButton.onClick.AddListener(() =>
        {
            OnRequestAutoEquip?.Invoke(ArtifactType.Active);
            UpdateSelectionUI(ArtifactType.Active);
        });
        _UnEquipAllButton.onClick.AddListener(() =>
        {
            OnRequestUnEquipAll?.Invoke();
            UpdateSelectionUI(null);
        });

        UpdateSelectionUI(null);

        if (_passiveOutline != null) _passiveOutline.enabled = false;
        if (_activeOutline != null) _activeOutline.enabled = false;
        
    }
    private void OnEnable()
    {
        UIManager.PubishAddUIStackEvent(this);

        if (!_isInitialized)
        {
            StartCoroutine(InitializeUIAfterFrame());
        }
        else
        {
            RefreshUI();
        }
    }
    private void Start()
    {
        
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

    private IEnumerator InitializeUIAfterFrame()
    {
        yield return null;

        _gotoCardDeckButton.onClick.AddListener(OnCardDeckClicked);
        _presenter.InitialDisplay();

        _isInitialized = true;
    }

    private void RefreshUI()
    {
        _presenter?.InitialDisplay();
    }

    #region 버튼
    private void OnCardDeckClicked()
    {
        FadeManager.Instance.SwitchGameObjects(gameObject, UIManager.Instance.GetUI<DeckPresetController>().gameObject);
    }

    public void OnBackPressed()
    {
        Debug.Log($"{gameObject.name} 뒤로가기: ");
        OnCardDeckClicked();
    }

    #endregion

    #region 자동 장착 로직 관련
    [Header("자동 장착 버튼")]
    [SerializeField] private Button _passiveEquipButton;
    [SerializeField] private Button _activeEquipButton;
    [SerializeField] private Button _UnEquipAllButton;

    [Header("선택 아웃라인")]
    [SerializeField] private Outline _passiveOutline;
    [SerializeField] private Outline _activeOutline;
    //private ArtifactType _selectedType;

    //private void OnPassiveEquipButtonClicked()
    //{
    //    _selectedType = ArtifactType.Passive;
    //    UpdateSelectionUI();
    //}

    //private void OnActiveEquipButtonClicked()
    //{
    //    _selectedType = ArtifactType.Active;
    //    UpdateSelectionUI();
    //}

    private void UpdateSelectionUI(ArtifactType? selectedType)
    {
        if (_passiveOutline != null)
            _passiveOutline.enabled = (selectedType == ArtifactType.Passive);

        if (_activeOutline != null)
            _activeOutline.enabled = (selectedType == ArtifactType.Active);
    }
    #endregion
}
