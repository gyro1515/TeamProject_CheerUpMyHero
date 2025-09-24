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

    [Header("--- ViewModeGroup UI 연결 ---")]
    [SerializeField] private List<DeckTabUI> deckTabs;

    [Header("--- EditNameGroup UI 연결 ---")]
    [SerializeField] private TMP_InputField deckNameInputField;
    [SerializeField] private Button confirmNameButton;
    [SerializeField] private Button cancelNameButton;

    // (기타 유닛 슬롯, 기능 버튼, 외부 패널 연결은 그대로)
    [Header("유닛 슬롯 설정")]
    [SerializeField] private GameObject unitSlotPrefab;
    [SerializeField] private Transform unitSlotParent;

    [Header("기능 버튼")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button adviserButton;
    [SerializeField] private Button relicButton;
    [SerializeField] private Button autoButton; 

    [Header("외부 패널 연결")]
    [SerializeField] private ConfirmationPopup confirmationPopup;

    // --- 내부 변수 ---
    private MainScreenUI _mainScreenUI;
    private UIStageSelect _stageSelectUI;
    private List<DeckUnitSlot> _unitSlots = new List<DeckUnitSlot>();
    private int _currentDeckIndex = 1;

    void Start()
    {
        InstantiateUnitSlots();
        _currentDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        _mainScreenUI = UIManager.Instance.GetUI<MainScreenUI>();
        _stageSelectUI = UIManager.Instance.GetUI<UIStageSelect>();

        for (int i = 0; i < deckTabs.Count; i++)
        {
            int deckIndex = i + 1;
            deckTabs[i].TabButton.onClick.AddListener(() => SelectDeck(deckIndex));
            deckTabs[i].EditIconObject.GetComponent<Button>().onClick.AddListener(EnterEditMode);
        }

        resetButton.onClick.AddListener(OnResetClicked);
        completeButton.onClick.AddListener(OnCompleteClicked);
        adviserButton.onClick.AddListener(GoToMainScene);
        confirmNameButton.onClick.AddListener(OnConfirmNameChange);
        cancelNameButton.onClick.AddListener(ExitEditMode);
        relicButton.onClick.AddListener(OnRelicButtonClicked);
        autoButton.onClick.AddListener(OnAutoFormClicked);

        editNameCanvasGroup.alpha = 0;
        editNameCanvasGroup.interactable = false;
        editNameCanvasGroup.blocksRaycasts = false;

        SelectDeck(_currentDeckIndex);
    }

    public void SelectDeck(int deckIndex)
    {
        _currentDeckIndex = deckIndex;
        PlayerDataManager.Instance.ActiveDeckIndex = deckIndex;

        for (int i = 0; i < deckTabs.Count; i++)
        {
            bool isActive = (i + 1 == deckIndex);
            deckTabs[i].NameText.text = PlayerDataManager.Instance.DeckPresets[i + 1].DeckName;
            deckTabs[i].TabButton.image.color = isActive ? Color.yellow : Color.white;
            deckTabs[i].EditIconObject.SetActive(isActive);
        }

        UpdateUnitSlotsUI();
    }

    #region 이름 수정 모드
    private void EnterEditMode()
    {
        viewModeCanvasGroup.DOFade(0.3f, 0.3f);
        viewModeCanvasGroup.interactable = false;

        FadeEffectManager.Instance.FadeInUI(editNameCanvasGroup);

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
        FadeEffectManager.Instance.FadeOutUI(editNameCanvasGroup);

        viewModeCanvasGroup.DOFade(1f, 0.3f);
        viewModeCanvasGroup.interactable = true;

        SelectDeck(_currentDeckIndex);
    }
    #endregion

    #region 기타 UI 및 버튼 로직
    void InstantiateUnitSlots()
    {
        for (int i = 0; i < 9; i++)
        {
            GameObject slotGO = Instantiate(unitSlotPrefab, unitSlotParent);
            DeckUnitSlot slotScript = slotGO.GetComponent<DeckUnitSlot>();
            if (slotScript != null)
            {
                _unitSlots.Add(slotScript);
                int slotIndex = i;
                slotScript.GetComponent<Button>().onClick.AddListener(() => OnUnitSlotClicked(slotIndex));
            }
        }
    }

    private void UpdateUnitSlotsUI()
    {
        List<int> currentDeckUnits = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds;
        for (int i = 0; i < _unitSlots.Count; i++)
        {
            int unitId = currentDeckUnits[i];
            bool isEmpty = (unitId == -1);

            _unitSlots[i].SetData(isEmpty, i);
        }
        UpdateCompleteButtonState();
    }

    private void UpdateCompleteButtonState()
    {
        List<int> currentDeck = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds;
        bool isDeckEmpty = !currentDeck.Exists(id => id != -1);
        completeButton.interactable = !isDeckEmpty;
    }

    void OnUnitSlotClicked(int slotIndex)
    {
        Debug.Log($"{_currentDeckIndex}번 덱의 {slotIndex + 1}번 슬롯 클릭됨");
    }

    private void OnUnitSelected(int slotIndex, int unitId)
    {
        PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds[slotIndex] = unitId;
        UpdateUnitSlotsUI();
    }

    private void OnResetClicked()
    {
        Debug.Log("초기화 버튼 클릭");
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
            confirmationPopup.Open(() => StartCoroutine(CompleteFormationRoutine()));
        }
        else
        {
            StartCoroutine(CompleteFormationRoutine());
        }
    }

    private IEnumerator CompleteFormationRoutine()
    {
        if (confirmationPopup.gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(0.3f);
        }

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

    private void OnAutoFormClicked()
    {
        Debug.Log("자동 편성 버튼 클릭됨");
    }
    private void OnRelicButtonClicked()
    {
        Debug.Log("유물 전환 패널 열기 시도");
    }

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