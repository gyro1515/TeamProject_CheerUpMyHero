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

    [Header("--- UI 그룹 ---")]
    [SerializeField] private CanvasGroup viewModeCanvasGroup; // 평상시 UI 그룹
    [SerializeField] private CanvasGroup editNameCanvasGroup; // 이름 수정 UI 그룹

    [Header("--- 하위 컨트롤러 ---")]
    [SerializeField] private DeckTabController deckTabController;


    [Header("--- EditNameGroup UI 연결 ---")]
    [SerializeField] private TMP_InputField deckNameInputField;
    [SerializeField] private Button confirmNameButton;
    [SerializeField] private Button cancelNameButton;

    [Header("시너지 UI 설정")]
    [SerializeField] private GameObject synergyIconPrefab;
    [SerializeField] private Transform synergyIconParent;

    [Header("기능 버튼")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button completeButton; //adviserbtn
    [SerializeField] private Button adviserButton; //backbtn
    [SerializeField] private Button relicButton;
    [SerializeField] private Button autoButton;

    [Header("외부 패널 연결")]
    [SerializeField] private ConfirmationPopup confirmationPopup;
    [SerializeField] private UIUnitCardSelect unitCardSelectPanel; //임의로 지어 놓은 것
  
    [Header("유닛 슬롯 설정")]
    [SerializeField] private List<DeckUnitSlot> unitSlots;
    // --- 내부 변수 ---
    private MainScreenUI _mainScreenUI;
    private UIStageSelect _stageSelectUI;
    private int _currentDeckIndex = 1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) //테스트 코드
        {
            OnUnitSelected(0, 100012);
            OnUnitSelected(1, 100013);
            OnUnitSelected(2, 100014);

        }
    }
    private void Start()
    {
        _currentDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        _mainScreenUI = UIManager.Instance.GetUI<MainScreenUI>();
        _stageSelectUI = UIManager.Instance.GetUI<UIStageSelect>();

        for (int i = 0; i < unitSlots.Count; i++)
        {
            int slotIndex = i;
            unitSlots[i].GetComponent<Button>().onClick.AddListener(() => OnUnitSlotClicked(slotIndex));
        }

        deckTabController.Initialize();
        deckTabController.OnTabSelected += SelectDeck;
        deckTabController.OnEditIconClicked += EnterEditMode;
        // 나머지 기능 버튼들에 이벤트 연결
        resetButton.onClick.AddListener(OnResetClicked);
        completeButton.onClick.AddListener(OnCompleteClicked);
        adviserButton.onClick.AddListener(GoToMainScene);
        confirmNameButton.onClick.AddListener(OnConfirmNameChange);
        cancelNameButton.onClick.AddListener(ExitEditMode);
        autoButton.onClick.AddListener(OnAutoFormClicked);

        // UI 초기 상태 설정
        editNameCanvasGroup.alpha = 0;
        editNameCanvasGroup.interactable = false;
        editNameCanvasGroup.blocksRaycasts = false;

        SelectDeck(_currentDeckIndex);
    }

    #region UI 생성 및 업데이트

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
        for (int i = 0; i < unitSlots.Count; i++)
        {
            int unitId = currentDeckUnits[i];

            //PlayerDataManager에서 TempCardData를 가져옵니다
            var unitData = (unitId == -1) ? null : PlayerDataManager.Instance.GetUnitData(unitId);

            //DeckUnitSlot의 SetData 함수에 unitData와 슬롯 번호를 전달
            unitSlots[i].SetData(unitData, i);
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
        FadeManager.FadeInUI(editNameCanvasGroup);

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
        FadeManager.FadeOutUI(editNameCanvasGroup);
        viewModeCanvasGroup.DOFade(1f, 0.3f);
        viewModeCanvasGroup.interactable = true;
        SelectDeck(_currentDeckIndex);
    }
    #endregion

    #region 버튼 클릭 이벤트 함수
    void OnUnitSlotClicked(int slotIndex)
    {
        Debug.Log($"{_currentDeckIndex}번 덱의 {slotIndex + 1}번 슬롯 클릭됨 -> 유닛 선택창 열기");
        unitCardSelectPanel.gameObject.SetActive(true);
        unitCardSelectPanel.SetDeckSlotNum(slotIndex);
    }

    public void OnUnitSelected(int slotIndex, int unitId)
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
    private void OnAutoFormClicked()
    {
        Debug.Log("자동 편성 시작");

        //현재 덱의 빈 슬롯이 몇 개인지, 어느 위치인지 확인함
        List<int> currentUnitIds = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds;
        List<int> emptySlotIndexes = new List<int>();
        for (int i = 0; i < currentUnitIds.Count; i++)
        {
            if (currentUnitIds[i] == -1)
            {
                emptySlotIndexes.Add(i);
            }
        }

        if (emptySlotIndexes.Count == 0)
        {
            Debug.Log("빈 슬롯이 없어 자동 편성을 할 수 없습니다.");
            return;
        }

        // 플레이어가 보유한 모든 유닛 ID 목록
        List<int> ownedUnitIds = new List<int>(PlayerDataManager.Instance.cardDic.Keys);

        //이미 현재 덱에 편성된 유닛은 후보에서 제외
        ownedUnitIds.RemoveAll(id => currentUnitIds.Contains(id));

        //남은 후보 유닛들을 무작위로 섞기
        for (int i = 0; i < ownedUnitIds.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, ownedUnitIds.Count);
            int temp = ownedUnitIds[i];
            ownedUnitIds[i] = ownedUnitIds[randomIndex];
            ownedUnitIds[randomIndex] = temp;
        }

        //빈 슬롯에 섞인 유닛들을 순서대로 채워 넣기
        int unitsToFill = Mathf.Min(emptySlotIndexes.Count, ownedUnitIds.Count);
        for (int i = 0; i < unitsToFill; i++)
        {
            int slotIndexToFill = emptySlotIndexes[i];
            int unitIdToPlace = ownedUnitIds[i];
            currentUnitIds[slotIndexToFill] = unitIdToPlace;
        }

        //변경된 덱 정보로 UI를 새로고침
        UpdateUnitSlotsUI();
    }
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