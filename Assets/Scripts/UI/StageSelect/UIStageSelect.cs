using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 나중에 플레이어 데이터에 들어갈 것들
//[System.Serializable]
//public class SubStageData
//{
//    public string displayName;      // "1-1" 같은 표시용 이름
//    public bool isUnlocked = false; // 해금 여부
//}

//[System.Serializable]
//public class MainStageData
//{
//    public string displayName;          // "1. 겨울왕국"
//    public bool isUnlocked = false;     // 메인 스테이지 해금 여부
//    public List<SubStageData> subStages = new List<SubStageData>(); // 서브 스테이지 리스트
//}
public class UIStageSelect : BaseUI, IBackButtonHandler
{
    [Header("스테이지 선택UI 설정")]
    [SerializeField] Transform stageSlotsParent; // 슬롯들이 생성될 부모
    [SerializeField] GameObject selectSlotPrefab;
    [SerializeField] Button returnToSelDeckBtn;

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRect; // stageSlotsParent와 동일한 오브젝트 연결

    [Header("운명, 도전 UI")]
    [SerializeField] private UIDestinyRoullette _destinyUI;

    //private DeckPresetController _deckPresetController;
    private MainScreenUI _mainScreenUI;
    private List<UISelecStageSlot> _allStageSlots = new List<UISelecStageSlot>();
    private UIMenu uiMenu;
    
    private void Awake()
    {

        // 돌아가기 버튼 설정
        returnToSelDeckBtn.onClick.AddListener(MoveToSelDeck);

        // 모든 스테이지 슬롯을 생성하고 초기화
        CreateAllStageSlots();
        //_destinyUI.CloseUI();
    }
    private void Start()
    {
        _mainScreenUI = UIManager.Instance.GetUI<MainScreenUI>();
        _destinyUI = UIManager.Instance.GetUI<UIDestinyRoullette>();
        uiMenu = UIManager.Instance.GetUI<UIMenu>();

    }
    private void OnEnable()
    {
        StartCoroutine(CoScrollToLatestClearedStage());
        UIManager.PubishAddUIStackEvent(this);

    }
    private void OnDisable()
    {
        UIManager.PublishRemoveUIStackEvent();
    }
    /// 모든 메인/서브 스테이지를 하나의 목록으로 생성하는 함수
    private void CreateAllStageSlots()
    {
        // 기존에 생성된 슬롯이 있다면 모두 삭제
        foreach (Transform child in stageSlotsParent)
        {
            Destroy(child.gameObject);
        }

        _allStageSlots.Clear();

        // SettingDataManager에서 전체 스테이지 데이터를 가져옵니다.
        List<MainStageData> allStageData = SettingDataManager.Instance.MainStageData;

        // 모든 메인 스테이지를 순회
        for (int mainIndex = 0; mainIndex < allStageData.Count; mainIndex++)
        {
            for (int subIndex = 0; subIndex < allStageData[mainIndex].subStages.Count; subIndex++)
            {
                UISelecStageSlot slot = Instantiate(selectSlotPrefab, stageSlotsParent).GetComponent<UISelecStageSlot>();
                SubStageData subData = allStageData[mainIndex].subStages[subIndex];

                string stageName = subData.displayName;
                bool isUnlocked = allStageData[mainIndex].isUnlocked && subData.isUnlocked;

                slot.Initialize(stageName, isUnlocked);

                int capturedMainIndex = mainIndex;
                int capturedSubIndex = subIndex;
                _allStageSlots.Add(slot);

                slot.SelectButton.onClick.AddListener(() => MoveToBattle(capturedMainIndex, capturedSubIndex));
            }
        }
    }
    /// 가장 마지막에 클리어한 스테이지의 인덱스를 찾는 함수
    private int GetLatestClearedStageIndex()
    {
        List<MainStageData> allStageData = SettingDataManager.Instance.MainStageData;
        int latestIndex = 0;
        int flatIndex = 0;

        for (int mainIndex = 0; mainIndex < allStageData.Count; mainIndex++)
        {
            // 메인 스테이지가 잠겨있으면 그 이후는 볼 필요 없음
            if (!allStageData[mainIndex].isUnlocked) break;

            for (int subIndex = 0; subIndex < allStageData[mainIndex].subStages.Count; subIndex++)
            {
                if (allStageData[mainIndex].subStages[subIndex].isUnlocked)
                {
                    // 해금된 스테이지를 만나면 그 위치를 기록
                    latestIndex = flatIndex;
                }
                else
                {
                    // 아직 해금 안된 스테이지를 만나면 바로 종료
                    return latestIndex;
                }
                flatIndex++;
            }
        }
        return latestIndex;
    }

    /// 특정 슬롯이 보이도록 스크롤을 이동시키는 코루틴
    private IEnumerator CoScrollToLatestClearedStage()
    {
        // UI 레이아웃이 계산될 때까지 한 프레임 기다립니다.
        yield return null;

        int targetIndex = GetLatestClearedStageIndex();
        if (targetIndex < 0 || targetIndex >= _allStageSlots.Count)
        {
            yield break; // 유효하지 않은 인덱스면 종료
        }

        //목표 슬롯의 RectTransform을 가져옵니다.
        RectTransform targetSlotRect = _allStageSlots[targetIndex].GetComponent<RectTransform>();

        // 캔버스 업데이트를 강제로 실행하여 정확한 위치를 계산하도록 합니다.
        Canvas.ForceUpdateCanvases();

        // 목표 슬롯의 위치로 Content 패널의 anchoredPosition.y 값을 변경합니다.
        //(목표 슬롯의 y좌표만큼 Content를 위로 올립니다)
        contentRect.anchoredPosition = new Vector2(
            contentRect.anchoredPosition.x,
            -targetSlotRect.anchoredPosition.y - 70f
        );
    }

    void MoveToBattle(int mainIdx, int subIdx)
    {
        
        Debug.Log($"{mainIdx + 1}-{subIdx + 1} 스테이지 선택 운명 선택으로 이동");

        // PlayerDataManager에 선택된 스테이지 정보를 저장
        PlayerDataManager.Instance.SelectedStageIdx = (mainIdx, subIdx);

        // 전투 씬을 로드
        // SceneLoader.Instance.StartLoadScene(SceneState.BattleScene);

        if (_destinyUI != null)
        {
            FadeManager.Instance.SwitchGameObjects(gameObject, _destinyUI.gameObject);
        }
        else
        {
            Debug.Log("운명 UI null임");
        }
    }

    void MoveToSelDeck()
    {
        FromUI origin = UIManager.Instance.fromUI;
        Debug.Log("메인으로 이동");
        if (origin == FromUI.UIMenu)
        {
            // 2a. "UIMenu"에서 왔으면 UIMenu로 돌아갑니다.
            Debug.Log("UIMenu로 돌아가기");
            if (uiMenu != null)
            {
                FadeManager.Instance.SwitchGameObjects(gameObject, uiMenu.gameObject);
            }
            else
            {
                Debug.LogError("UIMenu 오브젝트가 UIStageSelect에 연결되지 않았습니다!");
            }
        }
        else // FromUI.MainScreen (또는 기본값)
        {
            // 2b. "MainScreen"에서 왔으면 MainScreen으로 돌아갑니다.
            Debug.Log("메인(덱)으로 이동");
            if (_mainScreenUI != null)
            {
                FadeManager.Instance.SwitchGameObjects(gameObject, _mainScreenUI.gameObject);
            }
            else
            {
                Debug.LogError("UIManager에서 _mainScreenUI 찾을 수 없습니다!");
            }
        }
    }

    public void OnBackPressed()
    {
        Debug.Log("뒤로가기 버튼: 메인으로 이동");
        MoveToSelDeck();
    }
}