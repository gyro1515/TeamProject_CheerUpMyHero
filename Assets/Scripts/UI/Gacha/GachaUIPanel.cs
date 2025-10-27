using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class GachaUIPanel : BasePopUpUI 
{
    [Header("UI 참조")]
    [SerializeField] private Button backButton; 
    [SerializeField] private ContractPagesController contractPagesController;
    [SerializeField] private Button pullOneButton;        // 1회 뽑기 버튼
    [SerializeField] private Button pullTenButton;        // 10회 뽑기 버튼

    [Header("천장 시스템 UI")]
    [SerializeField] private TextMeshProUGUI limitedPityText; // 1페이지 천장 텍스트
    [SerializeField] private TextMeshProUGUI standardPityText; // 2페이지 천장 텍스트
    [Header("연출 패널")]
    [SerializeField] private GachaSequenceController gachaSequenceController;

    private IEventSubscriber<LimitedPityCountUpdatedEvent> _limitedPitySubscriber;
    private IEventSubscriber<StandardPityCountUpdatedEvent> _standardPitySubscriber;

    protected override void Awake()
    {
        base.Awake(); 

        backButton?.onClick.AddListener(OnBackButtonClicked);
        pullOneButton?.onClick.AddListener(OnPullOneClicked);
        pullTenButton?.onClick.AddListener(OnPullTenClicked);

        _limitedPitySubscriber = EventManager.GetSubscriber<LimitedPityCountUpdatedEvent>();
        _standardPitySubscriber = EventManager.GetSubscriber<StandardPityCountUpdatedEvent>();

        if (contractPagesController == null)
        {
            Debug.LogError("ContractPagesController가 GachaUIPanel에 연결되지 않았습니다!");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _limitedPitySubscriber.Subscribe(HandleLimitedPityUpdate);
        _standardPitySubscriber.Subscribe(HandleStandardPityUpdate);
        UpdateInitialPityCounters();
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        _limitedPitySubscriber?.Unsubscribe(HandleLimitedPityUpdate);
        _standardPitySubscriber?.Unsubscribe(HandleStandardPityUpdate);
    }
 
    private void HandleLimitedPityUpdate(LimitedPityCountUpdatedEvent e)
    {
        if (limitedPityText != null)
        {
            limitedPityText.text = $"{e.NewCount} / {PlayerDataManager.LIMITED_GACHA_PITY_LIMIT}";
        }
    }

    private void HandleStandardPityUpdate(StandardPityCountUpdatedEvent e)
    {
        if (standardPityText != null)
        {
            standardPityText.text = $"{e.NewCount} / {PlayerDataManager.STANDARD_GACHA_PITY_LIMIT}";
        }
    }
  

    private void UpdateInitialPityCounters()
    {
        if (PlayerDataManager.Instance == null) return;

        if (limitedPityText != null)
        {
            limitedPityText.text = $"{PlayerDataManager.Instance.LimitedGachaPityCount} / {PlayerDataManager.LIMITED_GACHA_PITY_LIMIT}";
        }
        if (standardPityText != null)
        {
            standardPityText.text = $"{PlayerDataManager.Instance.StandardGachaPityCount} / {PlayerDataManager.STANDARD_GACHA_PITY_LIMIT}";
        }
    }

    private async void OnPullOneClicked()
    {
        pullOneButton.interactable = false;
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

             PlayerDataManager.Instance.AddResource(ResourceType.Ticket, -1);

            // --- 2. 페이지별 천장 정보 가져오기 ---
            int currentPity = (currentPage == 0) ? PlayerDataManager.Instance.LimitedGachaPityCount : PlayerDataManager.Instance.StandardGachaPityCount;
            int pityLimit = (currentPage == 0) ? PlayerDataManager.LIMITED_GACHA_PITY_LIMIT : PlayerDataManager.STANDARD_GACHA_PITY_LIMIT;

            int resultId = -1;
            bool isEpicResult = false;

            // --- 3. 천장 확인 및 뽑기 실행 ---
            if (currentPity + 1 >= pityLimit)
            {
                Debug.LogWarning($"<color=yellow>[천장 발동!]</color> {currentPage + 1}페이지 {pityLimit}번째 뽑기, 확정 에픽!");

                if (currentPage == 0)
                {
                    Debug.Log("BackendManager [한정/픽업] (천장) 뽑기 호출 중...");
                    //'페이지별 확정 에픽 뽑기' 함수 요청 필요
                    resultId = await BackendManager.OneNormalGachaAsync(); // 임시
                }
                else // currentPage == 1
                {
                    Debug.Log("BackendManager [상시] (천장) 뽑기 호출 중...");
                    resultId = await BackendManager.OneNormalGachaAsync(); 
                }

                // 천장 발동 시, 서버 결과와 상관없이 클라이언트에서 강제 에픽 ID 할당 
                resultId = GetForcedEpicResult(currentPage);
                isEpicResult = true;
            }
            else
            {
                // ---  페이지별 뽑기 함수 분기  ---
                if (currentPage == 0)
                {
                    Debug.Log("BackendManager [한정/픽업] 뽑기 호출 중...");
                    //"한정 뽑기" 함수 만들어주면 교체
                    // resultId = await BackendManager.OneLimitedGachaAsync();
                    resultId = await BackendManager.OneNormalGachaAsync(); // 임시
                }
                else // currentPage == 1
                {
                    Debug.Log("BackendManager [상시] 뽑기 호출 중...");
                    resultId = await BackendManager.OneNormalGachaAsync();
                }
                isEpicResult = IsResultEpic(resultId);
                // ------------------------------------
            }

            gachaSequenceController.StartGachaSequence(new List<int> { resultId });
            // 1회 뽑기 결과도 List<int>로 만들어서 연출 컨트롤러에게 전달
            //List<int> results = new List<int> { resultId };
            //gachaSequenceController.StartGachaSequence(results);
            // ------------------------------------

            // --- 5. 페이지별 천장 카운터 업데이트 ---
            if (currentPage == 0) PlayerDataManager.Instance.UpdateLimitedPityCount(isEpicResult);
            else PlayerDataManager.Instance.UpdateStandardPityCount(isEpicResult);
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
                //pullTenButton.interactable = PlayerDataManager.Instance.GetResourceAmount(ResourceType.Ticket) >= 10;
            }
            else
            {
                pullOneButton.interactable = false;
                //pullTenButton.interactable = false;
            }
        }
    }


    // 10회 뽑기 함수 (주석 해제 및 수정)
    private async void OnPullTenClicked() 
    {
        //pullOneButton.interactable = false;
        pullTenButton.interactable = false;
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
            PlayerDataManager.Instance.AddResource(ResourceType.Ticket, -10);

            List<int> resultIds = new List<int>(); // 10개 결과를 담을 리스트
            //bool gotEpicInBatch = false; // 10회 뽑기 중 에픽 나왔는지 확인용

            // --- 2. 1회 뽑기 로직을 10번 반복 ---
            for (int i = 0; i < 10; i++)
            {
                // 페이지별 천장 정보 매번 가져오기
                int currentPity = (currentPage == 0) ? PlayerDataManager.Instance.LimitedGachaPityCount : PlayerDataManager.Instance.StandardGachaPityCount;
                int pityLimit = (currentPage == 0) ? PlayerDataManager.LIMITED_GACHA_PITY_LIMIT : PlayerDataManager.STANDARD_GACHA_PITY_LIMIT;

                int resultId = -1;
                bool isEpicResult = false;

                // 천장 확인 및 뽑기 실행
                if (currentPity + 1 >= pityLimit)
                {
                    Debug.LogWarning($"<color=yellow>[천장 발동!]</color> {currentPage + 1}페이지 (뽑기 {i + 1}/10), 확정 에픽!");
                    if (currentPage == 0) resultId = await BackendManager.OneNormalGachaAsync();
                    else resultId = await BackendManager.OneNormalGachaAsync();

                    resultId = GetForcedEpicResult(currentPage);
                    isEpicResult = true;
                }
                else
                {
                    if (currentPage == 0) resultId = await BackendManager.OneNormalGachaAsync();
                    else resultId = await BackendManager.OneNormalGachaAsync();

                    isEpicResult = IsResultEpic(resultId);
                }

                resultIds.Add(resultId); // 결과 리스트에 추가
                //if (isEpicResult) gotEpicInBatch = true; // 에픽 나왔다고 기록

                //  매 뽑기마다 천장 카운터 업데이트 
                if (currentPage == 0) PlayerDataManager.Instance.UpdateLimitedPityCount(isEpicResult);
                else PlayerDataManager.Instance.UpdateStandardPityCount(isEpicResult);
            }

            // --- 3. 10개 결과를 연출 컨트롤러에게 전달 ---
            if (gachaSequenceController != null)
            {
                gachaSequenceController.StartGachaSequence(resultIds);
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
        }
    }

    private int GetForcedEpicResult(int pageIndex)
    { 
        return UnityEngine.Random.Range(125001, 130000);
    }
    private bool IsResultEpic(int id) 
    { 
        return id > 125000; 
    }
    private void OnBackButtonClicked()
    {
        CloseUI();
    }

    public override void OnBackPressed()
    {
        OnBackButtonClicked();
    }
}
