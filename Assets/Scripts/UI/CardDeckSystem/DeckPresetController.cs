using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using System.Collections;
using DG.Tweening;

public class DeckPresetController : BaseUI
{
    // 각 덱 탭의 UI 요소들을 묶어서 관리할 클래스
    [System.Serializable]
    public class DeckTabUI
    {
        public Button TabButton;
        public TextMeshProUGUI NameText;
        public GameObject EditIconObject; // 각 탭에 속한 수정 아이콘
    }

    [Header("--- UI 그룹 (Canvas Group 필요!) ---")]
    [SerializeField] private CanvasGroup viewModeCanvasGroup; // 평상시 UI 그룹
    [SerializeField] private CanvasGroup editNameCanvasGroup; // 이름 수정 UI 그룹

    [Header("--- 하위 컨트롤러 ---")]
    [SerializeField] private DeckTabController deckTabController;


    [Header("--- EditNameGroup UI 연결 ---")]
    [SerializeField] private TMP_InputField deckNameInputField;
    [SerializeField] private Button confirmNameButton;
    [SerializeField] private Button cancelNameButton;

    [Header("유닛 슬롯 설정")]
    [SerializeField] private GameObject unitSlotPrefab;
    [SerializeField] private Transform unitSlotParent;

    [Header("시너지 UI 설정")]
    [SerializeField] private GameObject synergyIconPrefab;
    [SerializeField] private Transform synergyIconParent;

    [Header("기능 버튼")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button adviserButton;
    [SerializeField] private Button relicButton;
    [SerializeField] private Button autoButton;

    [Header("외부 패널 연결")]
    [SerializeField] private ConfirmationPopup confirmationPopup;
    // [SerializeField] private UnitSelectPanelController unitSelectPanel; //임의로 지어 놓은 것

    // --- 내부 변수 ---
    private MainScreenUI _mainScreenUI;
    private UIStageSelect _stageSelectUI;
    private List<DeckUnitSlot> _unitSlots = new List<DeckUnitSlot>();
    private int _currentDeckIndex = 1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) //테스트 코드
        {
            OnUnitSelected(0, 100001);
            Debug.Log("테스트: 1번 슬롯에 100001번 유닛 강제 할당");
            OnUnitSelected(1, 100002);
            Debug.Log("테스트: 2번 슬롯에 100002번 유닛 강제 할당");
            OnUnitSelected(2, 100003);
            Debug.Log("테스트: 3번 슬롯에 100003번 유닛 강제 할당");

        }
    }
    private void Start()
    {
        InstantiateUnitSlots();
        _currentDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        _mainScreenUI = UIManager.Instance.GetUI<MainScreenUI>();
        _stageSelectUI = UIManager.Instance.GetUI<UIStageSelect>();

        deckTabController.Initialize();
        deckTabController.OnTabSelected += SelectDeck;
        deckTabController.OnEditIconClicked += EnterEditMode;
        // 나머지 기능 버튼들에 이벤트 연결
        resetButton.onClick.AddListener(OnResetClicked);
        completeButton.onClick.AddListener(OnCompleteClicked);
        adviserButton.onClick.AddListener(GoToMainScene);
        confirmNameButton.onClick.AddListener(OnConfirmNameChange);
        cancelNameButton.onClick.AddListener(ExitEditMode);

        // UI 초기 상태 설정
        editNameCanvasGroup.alpha = 0;
        editNameCanvasGroup.interactable = false;
        editNameCanvasGroup.blocksRaycasts = false;

        SelectDeck(_currentDeckIndex);
    }

    #region UI 생성 및 업데이트
    void InstantiateUnitSlots()
    {
        for (int i = 0; i < 9; i++)
        {
            GameObject slotGO = Instantiate(unitSlotPrefab, unitSlotParent);
            slotGO.name = $"UnitSlot_{i}";
            DeckUnitSlot slotScript = slotGO.GetComponent<DeckUnitSlot>();
            if (slotScript != null)
            {
                _unitSlots.Add(slotScript);
                int slotIndex = i;
                slotScript.GetComponent<Button>().onClick.AddListener(() => OnUnitSlotClicked(slotIndex));
            }
        }
    }

    public void SelectDeck(int deckIndex)
    {
        _currentDeckIndex = deckIndex;
        PlayerDataManager.Instance.ActiveDeckIndex = deckIndex;

        deckTabController.UpdateTabs(deckIndex);

        UpdateUnitSlotsUI();
    }

    private void UpdateUnitSlotsUI()
    {
        List<int> currentDeckUnits = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds;
        for (int i = 0; i < _unitSlots.Count; i++)
        {
            int unitId = currentDeckUnits[i];

            var unitData = (unitId == -1) ? null : PlayerDataManager.Instance.GetUnitData(unitId);
            _unitSlots[i].SetData(unitData, i);
        }
        UpdateCompleteButtonState();
        // UpdateSynergyUI();
    }

    private void UpdateSynergyUI()
    {
        foreach (Transform child in synergyIconParent) { Destroy(child.gameObject); }
    }

    private void UpdateCompleteButtonState()
    {
        List<int> currentDeck = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds;
        bool isDeckEmpty = !currentDeck.Exists(id => id != -1);
        completeButton.interactable = !isDeckEmpty;
    }
    #endregion

    #region 이름 수정 모드
    private void EnterEditMode()
    {
        viewModeCanvasGroup.DOFade(0.3f, 0.3f);
        viewModeCanvasGroup.interactable = false;
        FadeManager.Instance.FadeInUI(editNameCanvasGroup);

        string currentName = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].DeckName;
        deckNameInputField.text = currentName;
        deckNameInputField.ActivateInputField();
    }

    private void OnConfirmNameChange()
    {
        string newName = deckNameInputField.text;
        if (string.IsNullOrWhiteSpace(newName)) return;

        PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].DeckName = newName;
        PlayerDataManager.Instance.SaveDecks();

        ExitEditMode();
    }

    private void ExitEditMode()
    {
        FadeManager.Instance.FadeOutUI(editNameCanvasGroup);
        viewModeCanvasGroup.DOFade(1f, 0.3f);
        viewModeCanvasGroup.interactable = true;
        SelectDeck(_currentDeckIndex);
    }
    #endregion

    #region 버튼 클릭 이벤트 함수
    void OnUnitSlotClicked(int slotIndex)
    {
        Debug.Log($"{_currentDeckIndex}번 덱의 {slotIndex + 1}번 슬롯 클릭됨 -> 유닛 선택창 열기");
        // unitSelectPanel.Open(slotIndex, OnUnitSelected); //임의로 지어놓은 것 
    }

    private void OnUnitSelected(int slotIndex, int unitId)
    {
        PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds[slotIndex] = unitId;
        UpdateUnitSlotsUI();
    }

    private void OnResetClicked()
    {
        List<int> currentDeckUnitIds = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds;
        for (int i = 0; i < currentDeckUnitIds.Count; i++)
        {
            currentDeckUnitIds[i] = -1;
        }
        UpdateUnitSlotsUI();
    }

    private void OnCompleteClicked()
    {

        List<int> currentDeck = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds;
        bool hasEmptySlot = currentDeck.Contains(-1);
        bool dontAskAgain = PlayerPrefs.GetInt("DontAskAgain_EmptyDeck", 0) == 1;


        if (hasEmptySlot && !dontAskAgain)
        {
            confirmationPopup.Open(CompleteFormationDirect);
        }
        else
        {
            CompleteFormationDirect();
        }
    }

    private void CompleteFormationDirect()
    {
        Debug.Log("편성 완료. 모든 덱 정보를 저장하고 다음 화면으로 전환합니다.");
        PlayerDataManager.Instance.SaveDecks();

        if (_stageSelectUI != null)
        {
            FadeManager.Instance.SwitchGameObjects(gameObject, _stageSelectUI.gameObject);
        }
        else
        {
            Debug.LogError("UIManager에서 UIStageSelect를 찾을 수 없습니다!");
        }
    }


    private void OnAutoFormClicked() { Debug.Log("자동 편성 버튼 클릭됨"); }
    private void OnRelicButtonClicked() { Debug.Log("유물 전환 패널 열기 시도"); }

    public void GoToMainScene()
    {
        if (_mainScreenUI != null)
        {
            FadeManager.Instance.SwitchGameObjects(gameObject, _mainScreenUI.gameObject);
        }
        else
        {
            Debug.LogError("UIManager에서 MainScreenUI를 찾을 수 없습니다!");
        }
    }
    #endregion
}