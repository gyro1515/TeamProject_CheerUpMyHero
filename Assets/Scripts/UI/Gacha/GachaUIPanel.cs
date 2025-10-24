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
        _limitedPitySubscriber.Subscribe(HandleLimitedPityUpdate);
        _standardPitySubscriber.Subscribe(HandleStandardPityUpdate);


        if (contractPagesController == null)
        {
            Debug.LogError("ContractPagesController가 GachaUIPanel에 연결되지 않았습니다!");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
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
      
            }
            else
            {
                PlayerDataManager.Instance.AddResource(ResourceType.Ticket, -1);
            }

            // --- 2. 페이지별 천장 정보 가져오기 ---
            int currentPity = (currentPage == 0) ? PlayerDataManager.Instance.LimitedGachaPityCount : PlayerDataManager.Instance.StandardGachaPityCount;
            int pityLimit = (currentPage == 0) ? PlayerDataManager.LIMITED_GACHA_PITY_LIMIT : PlayerDataManager.STANDARD_GACHA_PITY_LIMIT;

            int resultId = -1;
            bool isEpicResult = false;

            // --- 3. 천장 확인 및 뽑기 실행 ---
            if (currentPity + 1 >= pityLimit)
            {
                Debug.LogWarning($"<color=yellow>[천장 발동!]</color> {currentPage + 1}페이지 {pityLimit}번째 뽑기, 확정 에픽!");

                // ---  천장 뽑기에도 페이지 분기 적용 ---
                if (currentPage == 0)
                {
                    Debug.Log("BackendManager [한정/픽업] (천장) 뽑기 호출 중...");
                    // 한정 확정 에픽 뽑기 함수로 교체
                    resultId = await BackendManager.OneNormalGachaAsync(); 
                }
                else // currentPage == 1
                {
                    Debug.Log("BackendManager [상시] (천장) 뽑기 호출 중...");
                    resultId = await BackendManager.OneNormalGachaAsync(); 
                }

                resultId = GetForcedEpicResult(currentPage);
                isEpicResult = true;
            }
            else
            {
                if (currentPage == 0)
                {
                    Debug.Log("BackendManager [한정/픽업] 뽑기 호출 중...");
                    //"한정 뽑기" 함수로 교체
                    resultId = await BackendManager.OneNormalGachaAsync();
                }
                else // currentPage == 1
                {
                    Debug.Log("BackendManager [상시] 뽑기 호출 중...");
                    resultId = await BackendManager.OneNormalGachaAsync();
                }
                isEpicResult = IsResultEpic(resultId);
                // ------------------------------------
            }

            PostProcessGachaResult(resultId); // 결과 로그 출력

            // --- 4. 페이지별 천장 카운터 업데이트 ---
            if (currentPage == 0) PlayerDataManager.Instance.UpdateLimitedPityCount(isEpicResult);
            else PlayerDataManager.Instance.UpdateStandardPityCount(isEpicResult);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            // --- 5. 버튼 활성화 (잔액 확인 후) ---
            // PlayerDataManager 인스턴스 null 체크 (안전 코드)
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
 

    private void OnPullTenClicked()
    {
        Debug.Log("--- 10회 뽑기 버튼 클릭됨 ---");
        //pullOneButton.interactable = false;
    //    try
    //    {
    //        int resultId = await BackendManager.OneNormalGachaAsync();
    //        Debug.Log(resultId);
    //        PlayerDataManager.Instance.AddResource(ResourceType.Ticket, -10);
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogException(ex);
    //    }
    //    pullOneButton.interactable = true;
    }

    private int GetForcedEpicResult(int pageIndex)
    { 
        return UnityEngine.Random.Range(125001, 130000);
    }
    private bool IsResultEpic(int id) 
    { 
        return id > 125000; 
    }
    private void PostProcessGachaResult(int id) 
    {
        // ID 값의 범위에 따라 등급을 판단하고 다른 색깔/메시지로 로그 출력
        if (id > 125000) // 예: 125000 초과는 에픽
        {
            Debug.Log($"<color=magenta>Epic 결과:</color> {id}");
        }
        else if (id > 115000) // 예: 115000 초과는 레어
        {
            Debug.Log($"<color=cyan>Rare 결과:</color> {id}");
        }
        else if (id == -1) // -1은 실패
        {
            Debug.LogWarning("가챠 실패 (-1)");
        }
        else // 그 외는 커먼
        {
            Debug.Log($"Common 결과: {id}");
        }
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
