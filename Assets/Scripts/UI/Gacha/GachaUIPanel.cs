using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Unity.Services.CloudCode.GeneratedBindings.CheerUpMyHero.CloudCode;

public class GachaUIPanel : BaseUI, IBackButtonHandler
{
    [Header("UI 참조")]
    [SerializeField] private Button backButton; 
    [SerializeField] private ContractPagesController contractPagesController;
    [SerializeField] private Button pullOneButton;        // 1회 뽑기 버튼
    [SerializeField] private Button pullTenButton;        // 10회 뽑기 버튼
    [SerializeField] private Button characterInfoButton;        // 10회 뽑기 버튼

    [Header("천장 시스템 UI")]
    [SerializeField] private TextMeshProUGUI limitedPityText; // 1페이지 천장 텍스트
    [SerializeField] private Image limitedPityBackground;  
    [SerializeField] private TextMeshProUGUI standardPityText; // 2페이지 천장 텍스트
    [SerializeField] private Image standardPityBackground;
    [Header("천장 색상 설정")]
    [SerializeField] private Color defaultPityColor = Color.white; // 기본 배경색
    [SerializeField] private Color warningPityColor = Color.yellow; // 140+ 경고색
    [SerializeField] private int pityWarningThreshold = 140; // 경고 시작 횟수
    [Header("연출 패널")]
    [SerializeField] private GachaSequenceController gachaSequenceController;

    private IEventSubscriber<LimitedPityCountUpdatedEvent> _limitedPitySubscriber;
    private IEventSubscriber<StandardPityCountUpdatedEvent> _standardPitySubscriber;
    private MainScreenUI mainScreenUI;
    private UIMenu uiMenu;
     void Awake()
    {

        backButton?.onClick.AddListener(OnBackButtonClicked);
        pullOneButton?.onClick.AddListener(() => { OnPullOneClicked().Forget(); });
        pullTenButton?.onClick.AddListener(() => { OnPullTenClicked().Forget(); });

        _limitedPitySubscriber = EventManager.GetSubscriber<LimitedPityCountUpdatedEvent>();
        _standardPitySubscriber = EventManager.GetSubscriber<StandardPityCountUpdatedEvent>();

        if (contractPagesController == null)
        {
            Debug.LogError("ContractPagesController가 GachaUIPanel에 연결되지 않았습니다!");
        }
    }
    private void Start()
    {
        mainScreenUI = UIManager.Instance.GetUI<MainScreenUI>();
        uiMenu = UIManager.Instance.GetUI<UIMenu>();    
    }
    void OnEnable()
    {
        _limitedPitySubscriber.Subscribe(HandleLimitedPityUpdate);
        _standardPitySubscriber.Subscribe(HandleStandardPityUpdate);
        UpdateInitialPityCounters();
        UIManager.PubishAddUIStackEvent(this);
        if (PlayerDataManager.Instance != null)
        {
            int currentTickets = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Ticket);
            pullOneButton.interactable = currentTickets >= 1;  
            pullTenButton.interactable = currentTickets >= 10; 
        }
        else
        {
            // 플레이어 데이터가 없으면 둘 다 비활성화
            pullOneButton.interactable = false;
            pullTenButton.interactable = false;
        }
    }
 void OnDisable()
    {
        UIManager.PublishRemoveUIStackEvent();
        _limitedPitySubscriber?.Unsubscribe(HandleLimitedPityUpdate);
        _standardPitySubscriber?.Unsubscribe(HandleStandardPityUpdate);
    }
 
    private void HandleLimitedPityUpdate(LimitedPityCountUpdatedEvent e)
    {
        if (limitedPityText != null)
        {
            limitedPityText.text = $"{e.NewCount} / {PlayerDataManager.LimitedGachaPityLimit}";
            if (limitedPityBackground != null)
            {
                limitedPityBackground.color = (e.NewCount >= pityWarningThreshold) ? warningPityColor : defaultPityColor;
            }
        }
    }

    private void HandleStandardPityUpdate(StandardPityCountUpdatedEvent e)
    {
        if (standardPityText != null)
        {
            standardPityText.text = $"{e.NewCount} / {PlayerDataManager.StandardGachaPityLimit}";
            if (standardPityBackground != null)
            {
                standardPityBackground.color = (e.NewCount >= pityWarningThreshold) ? warningPityColor : defaultPityColor;
            }
        }
    }
  

    private void UpdateInitialPityCounters()
    {
        if (PlayerDataManager.Instance == null) return;

        if (limitedPityText != null)
        {
            int count = PlayerDataManager.Instance.LimitedGachaPityCount;
            limitedPityText.text = $"{PlayerDataManager.Instance.LimitedGachaPityCount} / {PlayerDataManager.LimitedGachaPityLimit}";
            if (limitedPityBackground != null)
            {
                limitedPityBackground.color = (count >= pityWarningThreshold) ? warningPityColor : defaultPityColor;
            }
        }
        if (standardPityText != null)
        {
            int count = PlayerDataManager.Instance.StandardGachaPityCount;
            standardPityText.text = $"{PlayerDataManager.Instance.StandardGachaPityCount} / {PlayerDataManager.StandardGachaPityLimit}";
            if (standardPityBackground != null)
            {
                standardPityBackground.color = (count >= pityWarningThreshold) ? warningPityColor : defaultPityColor;
            }
        }
    }
  
    private async UniTaskVoid OnPullOneClicked()
    {
        pullOneButton.interactable = false;
        UIManager.Instance.ShowLoading();
        //pullTenButton.interactable = false;
        Debug.Log("--- 1회 뽑기 버튼 클릭됨 ---");

        if (gachaSequenceController == null)
        {
            Debug.LogError("GachaSequenceController가 연결되지 않았습니다! 인스펙터 창을 확인하세요.");
            pullOneButton.interactable = true; // 버튼 다시 활성화
            return;
        }

        try
        {
            if (contractPagesController == null)
            {
                Debug.LogError("ContractPagesController 연결 없음!");
                return;
            }

            int currentPage = contractPagesController.CurrentPageIndex;
            Debug.Log($"--- {currentPage + 1}페이지 1회 뽑기 처리 시작 ---");

            // --- 1. 티켓 차감 로직 (먼저 실행) ---
            if (PlayerDataManager.Instance.GetResourceAmount(ResourceType.Ticket) < 1)
            {
                Debug.LogWarning("티켓 부족!");
                return;
            }

            // [가챠 서버 V2] 이제 티켓차감은 서버만에서 이루어짐
            // await PlayerDataManager.Instance.AddResource(ResourceType.Ticket, -1);

            // [가챠 서버 V2] 천장도 서버에서 알아서 계산
            // --- 2. 페이지별 천장 정보 가져오기 ---
            //int currentPity = (currentPage == 0) ? PlayerDataManager.Instance.LimitedGachaPityCount : PlayerDataManager.Instance.StandardGachaPityCount;
            //int pityLimit = (currentPage == 0) ? PlayerDataManager.LimitedGachaPityLimit : PlayerDataManager.StandardGachaPityLimit;

            //int resultId = -1;
            //bool isEpicResult = false;

            // --- 3. 천장 확인 및 뽑기 실행 ---
            //if (currentPity + 1 >= pityLimit)
            //{
            //    Debug.LogWarning($"<color=yellow>[천장 발동!]</color> {currentPage + 1}페이지 {pityLimit}번째 뽑기, 확정 에픽!");

            //    if (currentPage == 0)
            //    {
            //        Debug.Log("BackendManager [한정/픽업] (천장) 뽑기 호출 중...");
            //        //'페이지별 확정 에픽 뽑기' 함수 요청 필요
            //        resultId = await BackendManager.OnePickUpGachaAsync(); 
            //    }
            //    else // currentPage == 1
            //    {
            //        Debug.Log("BackendManager [상시] (천장) 뽑기 호출 중...");
            //        resultId = await BackendManager.OneNormalGachaAsync();
            //    }

            //    // 천장 발동 시, 서버 결과와 상관없이 클라이언트에서 강제 에픽 ID 할당 
            //    resultId = GetForcedEpicResult(currentPage);
            //    isEpicResult = true;
            //}
            //else
            //{
            //    // ---  페이지별 뽑기 함수 분기  ---
            //    if (currentPage == 0)
            //    {
            //        Debug.Log("BackendManager [한정/픽업] 뽑기 호출 중...");

            //        resultId = await BackendManager.OnePickUpGachaAsync();
            //    }
            //    else // currentPage == 1
            //    {
            //        Debug.Log("BackendManager [상시] 뽑기 호출 중...");
            //        resultId = await BackendManager.OneNormalGachaAsync();
            //    }
            //    isEpicResult = IsResultEpic(resultId);
            //    // ------------------------------------
            //}

            int resultId = -1;
            GachaResult result;
            if (currentPage == 0)
            {
                Debug.Log("BackendManager [한정/픽업] 뽑기 호출 중...");

                result = await BackendManager.OnePickupGachaAsync();
                
            }
            else // currentPage == 1
            {
                Debug.Log("BackendManager [상시] 뽑기 호출 중...");
                result = await BackendManager.OneNormalGachaAsync();
            }

            if (result.ResultUnit.Count > 0)
                resultId = result.ResultUnit[0].UnitId; //유닛 1개도 리스트로 나옴
            else // 빈 찬합오면 실패
                throw new Exception("가챠 결과 없음");

            //가챠 성공 여부 한번 더 확인
            if (result.UserCurrency == -1)
                throw new Exception("가챠 결과 없음");

            gachaSequenceController.StartGachaSequence(new List<int> { resultId });
            // 1회 뽑기 결과도 List<int>로 만들어서 연출 컨트롤러에게 전달
            //List<int> results = new List<int> { resultId };
            //gachaSequenceController.StartGachaSequence(results);
            // ------------------------------------

            // --- 페이지별 천장 카운터 업데이트 ---
            if (currentPage == 0) 
                PlayerDataManager.Instance.UpdateLimitedPityCount(result.CurrentPityCount);
            else 
                PlayerDataManager.Instance.UpdateStandardPityCount(result.CurrentPityCount);

            //로컬 재화 데이터 차감(서버엔 이미 반영)
            PlayerDataManager.Instance.ChangeResourceOnlyLocal(ResourceType.Ticket, result.UserCurrency);

            //5.9 얻은 유닛 해금
            PlayerDataManager.Instance.UnLockUnit(resultId);

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            // --- 6. 버튼 활성화 (잔액 확인 후) ---
            if (PlayerDataManager.Instance != null)
            {
                pullOneButton.interactable = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Ticket) >= 1;
                pullTenButton.interactable = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Ticket) >= 10;
            }
            else
            {
                pullOneButton.interactable = false;
                UIManager.Instance.HideLoading();
                //pullTenButton.interactable = false;
            }

            UIManager.Instance.HideLoading();
            await PlayerDataManager.Instance.SaveDataToCloudAsync();
        }
    }

    private async UniTaskVoid OnPullTenClicked()
    {
        //pullOneButton.interactable = false;
        pullTenButton.interactable = false;
        UIManager.Instance.ShowLoading();
        Debug.Log("--- 10회 뽑기 버튼 클릭됨 ---");

        try
        {
            if (contractPagesController == null)
            {
                Debug.LogError("ContractPagesController 연결 없음!");
                return;
            }

            int currentPage = contractPagesController.CurrentPageIndex;
            Debug.Log($"--- {currentPage + 1}페이지 10회 뽑기 처리 시작 ---");

            // --- 1. 10회 뽑기 티켓 차감 로직 ---
            if (PlayerDataManager.Instance.GetResourceAmount(ResourceType.Ticket) < 10)
            {
                Debug.LogWarning("티켓 부족 (10회)!");
                return; // 뽑기 중단 (finally에서 버튼 활성화됨)
            }

            // [가챠 서버 V2] 이제 티켓차감은 서버만에서 이루어짐
            //await PlayerDataManager.Instance.AddResource(ResourceType.Ticket, -10);

            List<int> resultIds = new List<int>(); // 10개 결과를 담을 리스트
            //bool gotEpicInBatch = false; // 10회 뽑기 중 에픽 나왔는지 확인용

            // [가챠 서버 V2] 이제 서버에서 한번에 10번을 뽑음
            // --- 2. 1회 뽑기 로직을 10번 반복 ---
            //for (int i = 0; i < 10; i++)
            //{
            //    // 페이지별 천장 정보 매번 가져오기
            //    int currentPity = (currentPage == 0) ? PlayerDataManager.Instance.LimitedGachaPityCount : PlayerDataManager.Instance.StandardGachaPityCount;
            //    int pityLimit = (currentPage == 0) ? PlayerDataManager.LimitedGachaPityLimit : PlayerDataManager.StandardGachaPityLimit;

            //    int resultId = -1;
            //    bool isEpicResult = false;

            //    // 천장 확인 및 뽑기 실행
            //    if (currentPity + 1 >= pityLimit)
            //    {
            //        Debug.LogWarning($"<color=yellow>[천장 발동!]</color> {currentPage + 1}페이지 (뽑기 {i + 1}/10), 확정 에픽!");
            //        if (currentPage == 0) resultId = await BackendManager.OnePickupGachaAsync();
            //        else resultId = await BackendManager.OneNormalGachaAsync();

            //        resultId = GetForcedEpicResult(currentPage);
            //        isEpicResult = true;
            //    }
            //    else
            //    {
            //        if (currentPage == 0) resultId = await BackendManager.OnePickupGachaAsync();
            //        else resultId = await BackendManager.OneNormalGachaAsync();

            //        isEpicResult = IsResultEpic(resultId);
            //    }

            //    resultIds.Add(resultId); // 결과 리스트에 추가
            //    //if (isEpicResult) gotEpicInBatch = true; // 에픽 나왔다고 기록

            //    //  매 뽑기마다 천장 카운터 업데이트 
            //    if (currentPage == 0) PlayerDataManager.Instance.UpdateLimitedPityCount(isEpicResult);
            //    else PlayerDataManager.Instance.UpdateStandardPityCount(isEpicResult);
            //}

            GachaResult result;
            if (currentPage == 0)
            {
                Debug.Log("BackendManager [한정/픽업] 뽑기 호출 중...");

                result = await BackendManager.TenPickupGachaAsync();

            }
            else // currentPage == 1
            {
                Debug.Log("BackendManager [상시] 뽑기 호출 중...");
                result = await BackendManager.TenNormalGachaAsync();
            }

            if (result.ResultUnit.Count == 10)
            {
                for (int i = 0; i < result.ResultUnit.Count; i++)
                {
                    resultIds.Add(result.ResultUnit[i].UnitId);
                }
            }
            else // 빈 찬합오면 실패
            {
                throw new Exception("가챠 결과가 10개가 아니거나 없음");
            }

            //가챠 성공 여부 한번 더 확인
            if (result.UserCurrency == -1)
                throw new Exception("가챠 결과 없음");

            // --- 3. 10개 결과를 연출 컨트롤러에게 전달 ---
            if (gachaSequenceController != null)
            {
                gachaSequenceController.StartGachaSequence(resultIds);
            }

            // --- 페이지별 천장 카운터 업데이트 ---
            if (currentPage == 0) 
                PlayerDataManager.Instance.UpdateLimitedPityCount(result.CurrentPityCount);
            else 
                PlayerDataManager.Instance.UpdateStandardPityCount(result.CurrentPityCount);

            //로컬 재화 데이터 차감(서버엔 이미 반영)
            PlayerDataManager.Instance.ChangeResourceOnlyLocal(ResourceType.Ticket, result.UserCurrency);

            //3.9 유닛 해금

            foreach (int id in resultIds)
            {
                PlayerDataManager.Instance.UnLockUnit(id);
            }

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            // --- 4. 버튼 활성화 (잔액 확인 후) ---
            if (PlayerDataManager.Instance != null)
            {
                pullOneButton.interactable = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Ticket) >= 1;
                pullTenButton.interactable = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Ticket) >= 10;
            }
            else
            {
                //pullOneButton.interactable = false;
                pullTenButton.interactable = false;
            }

            UIManager.Instance.HideLoading();
            await PlayerDataManager.Instance.SaveDataToCloudAsync();
        }
    }

    private int GetForcedEpicResult(int pageIndex)
    {
        List<int> epicPool;

        if (pageIndex == 0) // 1페이지 (한정/픽업)
        {
            epicPool = new List<int> {125001}; // 현재 픽업중인 유닛만 줌
        }
        else // 2페이지 (상시)
        {
            epicPool = new List<int> { 120003, 125001, 125002, 125003 };
        }


        // 2. 에픽 풀이 비어있는지 확인
        if (epicPool.Count == 0)
        {
            Debug.LogError($"[천장 오류] {pageIndex + 1}페이지의 에픽 풀이 비어있습니다!");
            return -1; // 실패
        }

        // 3. 에픽 목록 중에서 랜덤으로 한 명을 고릅니다.
        int randomIndex = UnityEngine.Random.Range(0, epicPool.Count);
        int resultId = epicPool[randomIndex];

        Debug.Log($"<color=yellow>[천장 발동!]</color> 확정 에픽 유닛 (ID: {resultId}) 반환.");

        return resultId;
    }
    private bool IsResultEpic(int id)
    {
        if (id == -1) return false;

        var unitData = DataManager.PlayerUnitData.GetData(id);

        if (unitData == null) return false; // 데이터가 없으면 에픽 아님

        return unitData.rarity == Rarity.epic;
    }
    private void OnBackButtonClicked()
    {
        FromUI origin = UIManager.Instance.fromUI;

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
            if (mainScreenUI != null)
            {
                FadeManager.Instance.SwitchGameObjects(gameObject, mainScreenUI.gameObject);
            }
            else
            {
                Debug.LogError("UIManager에서 _mainScreenUI 찾을 수 없습니다!");
            }
        }
    }

    public void OnBackPressed()
    {
        Debug.Log($"{gameObject.name} 뒤로 가기 버튼 눌림");
        OnBackButtonClicked();
    }
}
