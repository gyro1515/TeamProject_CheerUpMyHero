using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

// 덱 편성 UI 전체를 총괄하는 메인 컨트롤러 스크립트
public class DeckPresetController : MonoBehaviour
{
    [Header("--- UI 연결 ---")]
    [Header("덱 탭 버튼 (순서대로 5개)")]
    [SerializeField] private List<Button> deckTabButtons;

    [Header("유닛 슬롯 설정")]
    [SerializeField] private GameObject unitSlotPrefab; // 유닛 슬롯 프리팹
    [SerializeField] private Transform unitSlotParent;  // 슬롯들이 생성될 부모 Panel

    [Header("시너지 UI 설정")]
    [SerializeField] private GameObject synergyIconPrefab; // 시너지 아이콘 프리팹
    [SerializeField] private Transform synergyIconParent;  // 아이콘들이 생성될 부모 Panel

    [Header("기능 버튼")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Button completeButton;
    [SerializeField] private Button autoButton;
    [SerializeField] private Button adviserButton;
    [SerializeField] private Button relicButton;

    [Header("외부 패널 연결")]
    [SerializeField] private ConfirmationPopup confirmationPopup;

    private MainScreenUI _mainScreenUI;

    // --- 내부 변수 ---
    private List<DeckUnitSlot> _unitSlots = new List<DeckUnitSlot>(); // 생성된 슬롯 스크립트들을 담을 리스트
    private int _currentDeckIndex = 1;

    void Start()
    {
        InstantiateUnitSlots();
        _currentDeckIndex = PlayerDataManager.Instance.ActiveDeckIndex;
        _mainScreenUI = UIManager.Instance.GetUI<MainScreenUI>();

        for (int i = 0; i < deckTabButtons.Count; i++)
        {
            int deckIndex = i + 1;
            deckTabButtons[i].onClick.AddListener(() => SelectDeck(deckIndex));
        }

        resetButton.onClick.AddListener(OnResetClicked);
        completeButton.onClick.AddListener(OnCompleteClicked);
        autoButton.onClick.AddListener(OnAutoFormClicked);
        adviserButton.onClick.AddListener(GoToMainScene);
        relicButton.onClick.AddListener(OnRelicButtonClicked);

        SelectDeck(_currentDeckIndex);
        completeButton.interactable = true; //테스트 코드

    }

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

        for (int i = 0; i < deckTabButtons.Count; i++)
        {
            deckTabButtons[i].image.color = (i + 1 == deckIndex) ? Color.yellow : Color.white;
        }

        UpdateUnitSlotsUI();
    }

    private void UpdateUnitSlotsUI()
    {
        List<int> currentDeckUnits = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex];
        for (int i = 0; i < _unitSlots.Count; i++)
        {
            int unitId = currentDeckUnits[i];

            bool isEmpty = (unitId == -1);

            _unitSlots[i].SetData(isEmpty, i);
        }
        //  UpdateCompleteButtonState(); 테스트 위해 주석

        UpdateSynergyUI();

    }

    private void UpdateSynergyUI()
    {
        // 1. 기존에 있던 시너지 아이콘들을 모두 삭제
        foreach (Transform child in synergyIconParent)
        {
            Destroy(child.gameObject);
        }

        //  현재 덱 구성으로 어떤 시너지가 활성화됐는지 계산 
        // List<SynergyData> activeSynergies = SynergyManager.Instance.CalculateSynergies(currentDeckUnits);

        //  활성화된 시너지 개수만큼 프리팹으로 아이콘을 생성
        // foreach (var synergy in activeSynergies)
        // {
        //     GameObject iconGO = Instantiate(synergyIconPrefab, synergyIconParent);
        //     // iconGO.GetComponent<SynergyIconUI>().SetData(synergy); // 아이콘에 데이터 전달
        // }
    }

    void OnUnitSlotClicked(int slotIndex)
    {
        Debug.Log($"{_currentDeckIndex}번 덱의 {slotIndex}번 슬롯 클릭됨");
    }

    private void OnUnitSelected(int slotIndex, int unitId)
    {
        Debug.Log($"{slotIndex}번 슬롯에 {unitId}번 유닛을 배치합니다.");
        PlayerDataManager.Instance.DeckPresets[_currentDeckIndex][slotIndex] = unitId;

        UpdateUnitSlotsUI();
    }

    private void OnResetClicked()
    {
        Debug.Log($"{_currentDeckIndex}번 덱 초기화");
        List<int> currentDeck = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex];
        for (int i = 0; i < currentDeck.Count; i++)
        {
            currentDeck[i] = -1;
        }

        UpdateUnitSlotsUI();
    }

    private void OnCompleteClicked()
    {
        List<int> currentDeck = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex];
        bool hasEmptySlot = currentDeck.Contains(-1);
        bool dontAskAgain = PlayerPrefs.GetInt("DontAskAgain_EmptyDeck", 0) == 1;

        if (hasEmptySlot && !dontAskAgain)
        {
            confirmationPopup.Open(CompleteFormation);
        }
        else
        {
            CompleteFormation();
        }
    }

    private void CompleteFormation()
    {
        Debug.Log("편성 완료. 모든 덱 정보를 저장하고 씬을 이동합니다.");
        PlayerDataManager.Instance.SaveDecks();
        //스테이지 선택 씬으로 이동하는 코드
    }

    // 편성 완료 버튼의 활성화 상태를 업데이트하는 함수
    private void UpdateCompleteButtonState()
    {
        List<int> currentDeck = PlayerDataManager.Instance.DeckPresets[_currentDeckIndex];
        bool isDeckEmpty = true;
        foreach (int unitId in currentDeck)
        {
            if (unitId != -1)
            {
                isDeckEmpty = false;
                break;
            }
        }
        completeButton.interactable = !isDeckEmpty;
    }

    //'자동 편성' 버튼 클릭 시 (후순위)
    private void OnAutoFormClicked()
    {
        Debug.Log("자동 편성 버튼");
    }
    private void OnRelicButtonClicked()
    {
        Debug.Log("유물 전환 패널을 엽니다.");
    }
    public void GoToMainScene()
    {
        Debug.Log("메인 화면으로 돌아갑니다.");

        if (_mainScreenUI != null)
        {
            FadeManager.Instance.SwitchGameObjects(gameObject, _mainScreenUI.gameObject);
        }
        else
        {
            Debug.LogError("UIManager에서 MainScreenUI를 찾을 수 없습니다!");
        }
    }

    public void ResetDontAskAgainSetting()//테스트 코드
    {
        PlayerPrefs.DeleteKey("DontAskAgain_EmptyDeck");
        PlayerPrefs.Save(); // 변경사항 저장

        Debug.Log("'다시 묻지 않음' 설정이 초기화되었습니다!");
    }
}