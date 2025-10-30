using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class DeckPresetController : BaseUI, IBackButtonHandler
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
    [SerializeField] private DeckNameEditPanel editNamePanel; // 이름 수정 UI 패널
    [SerializeField] private LaterUpdatePopup laterUpdatePopup; // 이름 수정 UI 패널

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

    [Header("시너지 UI")]
    [SerializeField] UIDeckSynergy uiDeckSynergy;

    // --- 내부 변수 ---
    private MainScreenUI _mainScreenUI;
    private UIStageSelect _stageSelectUI;
    private UIArtifact _uIArtifact;
    private int _currentDeckIndex = 1;
    // 시너지별 카운트 저장용 딕셔너리
    Dictionary<UnitSynergyType, int> synergyCounts = new Dictionary<UnitSynergyType, int>();

    //튜토리얼
    private UITutorialDeck _tourDeck;
    private IEventSubscriber<SynergyDataUpdatedEvent> _synergyUpdateSubscriber;
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) //테스트 코드
        {
            OnAutoFormClicked();
        }
    }*/
    private void Awake()
    {
        uiDeckSynergy.Init();// 생성자 꼬이지 않게 여기서 먼저 초기화
        Debug.Log(GameManager.IsTutorialCompleted);
        if (!GameManager.IsTutorialCompleted)
        {
            _tourDeck = UIManager.Instance.GetUI<UITutorialDeck>();
            _tourDeck?.CloseUI();
        }
        _synergyUpdateSubscriber = EventManager.GetSubscriber<SynergyDataUpdatedEvent>();
        _synergyUpdateSubscriber.Subscribe(OnDeckRulesChanged);
    }
    private void OnEnable()
    {
        UIManager.PubishAddUIStackEvent(this);
        ValidateDeckAndUpdateUI();
        if (!GameManager.IsTutorialCompleted) _tourDeck?.OpenUI();
    }

    private void Start()
    {
        _currentDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        _mainScreenUI = UIManager.Instance.GetUI<MainScreenUI>();
        _stageSelectUI = UIManager.Instance.GetUI<UIStageSelect>();
        _uIArtifact = UIManager.Instance.GetUI<UIArtifact>();
        _uIArtifact.CloseUI();

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
        confirmNameButton.onClick.AddListener(() => { OnConfirmNameChange().Forget(); });
        cancelNameButton.onClick.AddListener(editNamePanel.CloseUI);
        autoButton.onClick.AddListener(OnAutoFormClicked);
        relicButton.onClick.AddListener(OnRelicButtonClicked);

        // UI 초기 상태 설정
        editNameCanvasGroup.alpha = 0;
        editNameCanvasGroup.interactable = false;
        editNameCanvasGroup.blocksRaycasts = false;

        SelectDeck(_currentDeckIndex);
    }
    private void OnDisable()
    {
        UIManager.PublishRemoveUIStackEvent();
        _synergyUpdateSubscriber?.Unsubscribe(OnDeckRulesChanged);
        _tourDeck?.CloseUI();
    }
    #region UI 생성 및 업데이트

    public void SelectDeck(int deckIndex)
    {
        _currentDeckIndex = deckIndex;
        PlayerDataManager.Instance.ActiveDeckIndex = deckIndex;

        deckTabController.UpdateTabs(deckIndex);

        UpdateUnitSlotsUI();
    }
    private void OnDeckRulesChanged(SynergyDataUpdatedEvent e)
    {
        Debug.Log("건물/시너지 변경 감지. 덱 유효성 검사 시작...");
        ValidateDeckAndUpdateUI();
    }
    private void UpdateUnitSlotsUI()
    {
        // 251023: DeckPreset의 baseUnitDatas를 사용하도록 변경
        //List<int> currentDeckUnits = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds;
        List<BaseUnitData> currentDeckUnitDatas = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].BaseUnitDatas;
        for (int i = 0; i < unitSlots.Count; i++)
        {
            //int unitId = currentDeckUnits[i];

            //PlayerDataManager에서 TempCardData를 가져옵니다
            //var unitData = (unitId == -1) ? null : DataManager.PlayerUnitData.GetData(unitId);

            //DeckUnitSlot의 SetData 함수에 unitData와 슬롯 번호를 전달
            //unitSlots[i].SetData(unitData, i);
            unitSlots[i].SetData(currentDeckUnitDatas[i], i);
        }
        UpdateCompleteButtonState();
        ValidateDeckAndUpdateUI();
        UpdateSynergyUI();
    }
    private void ValidateDeckAndUpdateUI()
    {
        // 1. 덱 데이터 및 슬롯 제한 가져오기
        List<BaseUnitData> currentDeck = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].BaseUnitDatas;
        int maxEpicUnits = PlayerDataManager.Instance.EpicUnitSlots;
        int maxRareUnits = PlayerDataManager.Instance.RareUnitSlots;

        // 2. 현재 유효한 등급 유닛 카운터
        int validEpicCount = 0;
        int validRareCount = 0;

        bool isDeckValid = true; // 덱이 유효한지(편성 완료 가능한지)
        string popupMessage = ""; // 팝업에 띄울 메시지

        // 3. (핵심) 모든 덱 슬롯 UI를 순회
        for (int i = 0; i < unitSlots.Count; i++)
        {
            if (unitSlots[i] == null) continue; // 슬롯 UI가 없으면 건너뛰기

            BaseUnitData unitData = currentDeck[i]; // 해당 슬롯의 데이터

            if (unitData == null)
            {
                // 빈 슬롯은 항상 유효
                unitSlots[i].SetValidationState(true);
                continue;
            }

            // 4. 등급별 유효성 검사
            if (unitData.rarity == Rarity.epic)
            {
                if (validEpicCount < maxEpicUnits)
                {
                    // 허용 범위 내의 에픽
                    unitSlots[i].SetValidationState(true);
                    validEpicCount++;
                }
                else
                {
                    // 허용 범위를 초과한 에픽
                    unitSlots[i].SetValidationState(false); // ✨ 불투명 레이어 켜기
                    isDeckValid = false;
                    popupMessage = $"병영 레벨이 낮아 \n에픽 유닛을 사용할 수 없습니다.\n덱을 수정해주세요.";
                }
            }
            else if (unitData.rarity == Rarity.rare)
            {
                if (validRareCount < maxRareUnits)
                {
                    // 허용 범위 내의 레어
                    unitSlots[i].SetValidationState(true);
                    validRareCount++;
                }
                else
                {
                    // 허용 범위를 초과한 레어
                    unitSlots[i].SetValidationState(false); // ✨ 불투명 레이어 켜기
                    isDeckValid = false;
                    popupMessage = $"병영 레벨이 낮아\n레어 유닛을 사용할 수 없습니다.\n덱을 수정해주세요.";
                }
            }
            else // Common
            {
                unitSlots[i].SetValidationState(true); // 커먼은 항상 유효
            }
        }

        // 5. 최종 버튼 상태 결정 및 팝업 띄우기
        completeButton.interactable = isDeckValid;
        if (!isDeckValid)
        {
            laterUpdatePopup.Show(popupMessage);
        }
    }

    private void UpdateSynergyUI()
    {
        uiDeckSynergy?.CheckDeckUnitSynergy(PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].BaseUnitDatas);
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
        //FadeManager.FadeInUI(editNameCanvasGroup);
        editNamePanel.OpenUI();

        string currentName = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].DeckName;
        deckNameInputField.text = currentName;
        deckNameInputField.ActivateInputField();
    }

    private async UniTaskVoid OnConfirmNameChange()
    {
        try
        {
            string newName = deckNameInputField.text;
            if (string.IsNullOrWhiteSpace(newName)) return;

            PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].DeckName = newName;
            await PlayerDataManager.Instance.SaveDataToCloudAsync();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogWarning("에러 팝업: 에러가 나서 이름을 바꾸지 못했습니다.");
        }
        finally
        {
            //ExitEditMode();
            editNamePanel.CloseUI();
        }
    }

    public void ExitEditMode()
    {
        //FadeManager.FadeOutUI(editNameCanvasGroup);
        //editNamePanel.CloseUI();
        viewModeCanvasGroup.DOFade(1f, 0.3f);
        viewModeCanvasGroup.interactable = true;
        SelectDeck(_currentDeckIndex);
    }
    #endregion

    #region 버튼 클릭 이벤트 함수
    void OnUnitSlotClicked(int slotIndex)
    {
        Debug.Log($"{_currentDeckIndex}번 덱의 {slotIndex + 1}번 슬롯 클릭됨 -> 유닛 선택창 열기");
        //unitCardSelectPanel.gameObject.SetActive(true);
        unitCardSelectPanel.OpenUI();
        unitCardSelectPanel.SetDeckSlotNum(slotIndex);
    }

    public void OnUnitSelected(int slotIndex, int unitId)
    {
        AudioManager.PlayOneShot(DataManager.AudioData.cardEquipSE, 0.8f);
        PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds[slotIndex] = unitId;
        PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].BaseUnitDatas[slotIndex] = DataManager.PlayerUnitData.GetData(unitId);
        UpdateUnitSlotsUI();
    }

    private void OnResetClicked()
    {
        List<int> currentDeckUnitIds = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds;
        List<BaseUnitData> currnetDeckUnitDatas = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].BaseUnitDatas;
        
        for (int i = 0; i < currentDeckUnitIds.Count; i++)
        {
            currentDeckUnitIds[i] = -1;
            currnetDeckUnitDatas[i] = null;
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
            confirmationPopup.Open(() => { CompleteFormationDirect().Forget();});
        }
        else
        {
            CompleteFormationDirect().Forget();
        }
    }

    //버튼에 등록하는 것 중에서 맨 마지막에 실행된다. => void로 해도 않을까?
    private async UniTaskVoid CompleteFormationDirect()
    {
        Debug.Log("편성 완료. 모든 덱 정보를 저장하고 전투씬으로 이동");

        try
        {
            await PlayerDataManager.Instance.SaveDataToCloudAsync();
            SceneLoader.Instance.StartLoadScene(SceneState.BattleScene);
        }
        catch (Exception ex) 
        {
            Debug.LogException(ex);
            Debug.LogWarning("에러 팝업: 에러가 나서 덱을 저장하지 못했습니다.");
        }

        
        /* if (_stageSelectUI != null)
         {
             FadeManager.Instance.SwitchGameObjects(gameObject, _stageSelectUI.gameObject);
         }
         else
         {
             Debug.LogError("UIManager에서 UIStageSelect를 찾을 수 없습니다!");
         }*/
    }
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void OnAutoFormClicked()
    {
        Debug.Log("등급을 고려한 자동 편성 시작");

        // --- 1. 현재 덱 상태 및 빈 슬롯 확인 ---
        List<int> currentUnitIds = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].UnitIds;
        List<BaseUnitData> baseUnitDatas = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex].BaseUnitDatas;
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

        // --- 2. 병영 레벨에 따른 슬롯 제한 및 현재 덱 상태 계산 ---
        // (PlayerDataManager가 이미 계산해 둔 값을 가져옴)
        int maxEpicUnits = PlayerDataManager.Instance.EpicUnitSlots;
        int maxRareUnits = PlayerDataManager.Instance.RareUnitSlots;

        // 현재 덱에 이미 편성된 유닛들의 등급별 카운트
        int currentEpicCount = baseUnitDatas.Count(data => data != null && data.rarity == Rarity.epic);
        int currentRareCount = baseUnitDatas.Count(data => data != null && data.rarity == Rarity.rare);

        Debug.Log($"현재 덱 상태: 에픽 {currentEpicCount}/{maxEpicUnits}, 레어 {currentRareCount}/{maxRareUnits}");

        // --- 3. 덱에 편성할 후보 유닛 목록 생성 (등급별 분리) ---

        // 3-1. 덱에 없는, 소유한 모든 유닛 '데이터' 가져오기
        List<BaseUnitData> availableUnits = PlayerDataManager.Instance.OwnedCardData.Values
            .Where(unitData => unitData != null && !currentUnitIds.Contains(unitData.idNumber)) // 덱에 이미 없는 유닛
            .ToList();

        // 3-2. 등급별로 분리
        List<BaseUnitData> epicCandidates = availableUnits.Where(u => u.rarity == Rarity.epic).ToList();
        List<BaseUnitData> rareCandidates = availableUnits.Where(u => u.rarity == Rarity.rare).ToList();
        List<BaseUnitData> commonCandidates = availableUnits.Where(u => u.rarity == Rarity.common).ToList();

        // 3-3. 각 후보 리스트 섞기 (같은 등급 내에서는 무작위)
        ShuffleList(epicCandidates);
        ShuffleList(rareCandidates);
        ShuffleList(commonCandidates);

        Debug.Log($"편성 가능 후보: 에픽 {epicCandidates.Count}명, 레어 {rareCandidates.Count}명, 커먼 {commonCandidates.Count}명");

        // --- 4. 빈 슬롯에 우선순위(에픽 -> 레어 -> 커먼)대로 채우기 ---
        foreach (int slotIndexToFill in emptySlotIndexes)
        {
            BaseUnitData unitToPlace = null;

            // 1순위: 에픽 슬롯이 남았고 (현재 < 최대), 에픽 후보가 있는가?
            if (currentEpicCount < maxEpicUnits && epicCandidates.Count > 0)
            {
                unitToPlace = epicCandidates[0];
                epicCandidates.RemoveAt(0); // 사용한 후보는 목록에서 제거
                currentEpicCount++; // 덱의 에픽 카운트 증가
                Debug.Log($"빈 슬롯 {slotIndexToFill}에 [에픽] 유닛 {unitToPlace.unitName} 배치");
            }
            // 2순위: 레어 슬롯이 남았고, 레어 후보가 있는가?
            else if (currentRareCount < maxRareUnits && rareCandidates.Count > 0)
            {
                unitToPlace = rareCandidates[0];
                rareCandidates.RemoveAt(0);
                currentRareCount++;
                Debug.Log($"빈 슬롯 {slotIndexToFill}에 [레어] 유닛 {unitToPlace.unitName} 배치");
            }
            // 3순위: 커먼 후보가 있는가? (커먼은 슬롯 제한 없음)
            else if (commonCandidates.Count > 0)
            {
                unitToPlace = commonCandidates[0];
                commonCandidates.RemoveAt(0);
                Debug.Log($"빈 슬롯 {slotIndexToFill}에 [커먼] 유닛 {unitToPlace.unitName} 배치");
            }
            else
            {
                // 모든 후보(커먼 포함)를 다 썼는데도 슬롯이 비었으면 중단
                Debug.Log("더 이상 채울 후보 유닛이 없습니다.");
                break; // foreach 루프 중단
            }

            currentUnitIds[slotIndexToFill] = unitToPlace.idNumber;
            baseUnitDatas[slotIndexToFill] = unitToPlace;
        }

        // 덱 장착 오디오 재생
        AudioManager.PlayOneShot(DataManager.AudioData.cardEquipSE);

        // 변경된 덱 정보로 UI를 새로고침
        UpdateUnitSlotsUI();
    }
    private void OnRelicButtonClicked() 
    {

        FadeManager.Instance.SwitchGameObjects(gameObject, _uIArtifact.gameObject);

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
    public void OnBackPressed()
    {
        Debug.Log("뒤로가기 버튼 눌림 - 메인 화면으로 이동");
        GoToMainScene();
    }
}