using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AdManager : SingletonMono<AdManager>
{
    // ===================================================================
    //           ▼ 내부 변수 및 설정 (Private) ▼
    // ===================================================================

    private GoogleMobileAdsConsentController _consentController;
    private bool _isInitialized = false;
    private RewardedAd _rewardedAd;

    // 테스트/실제 ID 분기 처리
    private const string TEST_UNIT_ID = "ca-app-pub-3940256099942544/5224354917"; // 구글 제공 테스트 ID
    private const string REAL_UNIT_ID = "ca-app-pub-xxxxxxxxxxxxxxxx/xxxxxxxxxx"; // 실제 ID 입력

    private string AD_UNIT_ID;

    protected override void Awake()
    {
        base.Awake();

        // 1. ID 설정 (에디터나 개발 빌드면 테스트 ID)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        AD_UNIT_ID = TEST_UNIT_ID;
#else
        //_adUnitId = REAL_UNIT_ID;
#endif

        // 2. 동적 생성 대응: ConsentController 자동 부착
        _consentController = GetComponent<GoogleMobileAdsConsentController>();
        if (_consentController == null)
        {
            _consentController = gameObject.AddComponent<GoogleMobileAdsConsentController>();
        }
    }

    // ===================================================================
    //           ▼ Public Static API (외부 노출용) ▼
    // ===================================================================

    /// <summary>
    /// 광고 시스템 초기화 (BackendManager에서 호출)
    /// </summary>
    public static async UniTask InitializeAsync()
    {
        // Instance가 없으면 알아서 생성(SingletonMono 특성) 후 내부 로직 호출
        await Instance.InternalInitializeAsync();
    }

    /// <summary>
    /// 보상형 광고 시청 (성공 시 true 반환)
    /// </summary>
    public static async UniTask<bool> ShowRewardedAdAsync()
    {
#if UNITY_WEBGL
        return true;
#endif

        // 초기화 안 됐으면 실패 처리
        if (!Instance._isInitialized)
        {
            Debug.LogWarning("광고 시스템이 아직 초기화되지 않았습니다.");
            return false;
        }

        return await Instance.InternalShowRewardedAdAsync();
    }

    // ===================================================================
    //           ▼ Private Instance Logic (실제 구현부) ▼
    // ===================================================================

    private async UniTask InternalInitializeAsync()
    {
        if (_isInitialized) return;

        Debug.Log("Google Mobile Ads 초기화 시작...");

        MobileAds.SetiOSAppPauseOnBackground(true);

        // 동의 절차 (가짜 스크립트가 있다면 즉시 통과됨)
        var consentTcs = new UniTaskCompletionSource<bool>();
        _consentController.GatherConsent((error) => consentTcs.TrySetResult(true));
        await consentTcs.Task;

        if (_consentController.CanRequestAds) //if 문이지만 현재 100% 실행됨
        {
            var initTcs = new UniTaskCompletionSource<bool>();

            //광고 초기화 로직
            MobileAds.Initialize(initStatus =>
            {
                // 1. 혹시라도 초기화 데이터가 없거나 에러가 있는지 확인 (안전장치)
                if (initStatus == null)
                {
                    Debug.LogError("구글 광고 SDK 초기화 실패 (Status is null)");
                    // 초기화 실패했음을 알림 (예외 처리)
                    initTcs.TrySetException(new Exception("Google Mobile Ads Init Failed"));
                    return;
                }

                // 2. 미디에이션(어댑터) 상태 로그: 간단히 확인하고 싶다면 주석 해제
                /*
                foreach (var item in initStatus.getAdapterStatusMap())
                {
                    Debug.Log($"Adapter: {item.Key} - {item.Value.InitializationState}");
                }
                */

                // 3. 성공 처리
                Debug.Log("구글 광고 SDK 초기화 완료!");
                _isInitialized = true;
                initTcs.TrySetResult(true);
            });
            await initTcs.Task;

            LoadRewardedAd(); // 광고 미리 로드
        }
    }

    /// <summary>
    /// 광고 로드 로직
    /// </summary>
    private void LoadRewardedAd()
    {
        // 기존 광고가 있다면 정리 (DestroyAd Snippet 반영)
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        Debug.Log("보상형 광고 로드 중...");
        var adRequest = new AdRequest();

        RewardedAd.Load(AD_UNIT_ID, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("광고 로드 실패: " + error);
                return;
            }

            Debug.Log("광고 로드 성공!");
            _rewardedAd = ad;
            
            // [공식 예제 반영] 이벤트 리스너 등록
            RegisterEventHandlers(_rewardedAd);
            
            // [공식 예제 반영] SSV 설정 (필요시 사용, 지금은 주석 처리)
            // ServerSideVerification(_rewardedAd); 
        });
    }

    /// <summary>
    /// 이벤트 핸들러 등록
    /// </summary>
    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"광고 수익 발생: {adValue.Value} {adValue.CurrencyCode}");
            // 여기에 Firebase Analytics 등의 매출 로그를 붙이면 좋습니다.
        };

        ad.OnAdImpressionRecorded += () => Debug.Log("광고 노출됨");
        ad.OnAdClicked += () => Debug.Log("광고 클릭됨");
        ad.OnAdFullScreenContentOpened += () => Debug.Log("광고 전체화면 열림");
        
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("광고 열기 실패: " + error);
            // 실패 시 바로 다음 광고 로드 시도
            LoadRewardedAd();
        };
    }
    

    /// <summary>
    /// SSV 옵션 설정 (나중에 필요하면 사용)
    /// </summary>
    private void ServerSideVerification(RewardedAd ad)
    {
        var options = new ServerSideVerificationOptions
        {
            // 예를 들어 유저 ID를 보내서 서버에서 검증하게 함
            CustomData = "USER_ID_12345" 
        };
        ad.SetServerSideVerificationOptions(options);
    }

    private async UniTask<bool> InternalShowRewardedAdAsync()
    {
        // 광고가 준비되지 않았을 때
        if (_rewardedAd == null || !_rewardedAd.CanShowAd())
        {
            Debug.LogWarning("광고가 준비되지 않았습니다. 다시 로드합니다.");
            LoadRewardedAd();
            return false;
        }

        var completionSource = new UniTaskCompletionSource<bool>();
        bool isRewardEarned = false;

        // 닫힘 이벤트 처리 (여기서 결과 반환 및 재로드)
        // 주의: 람다식 내부에서 외부 변수 캡처
        _rewardedAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("광고 닫힘.");
            
            // 1. Task 완료 처리 (UniTask 종료)
            completionSource.TrySetResult(isRewardEarned);

            // 2. [공식 예제 반영] 닫힌 후 다음 광고 미리 로드 (ReloadAd)
            LoadRewardedAd(); 
        };

        // 광고 열기 실패 시 처리
        _rewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("보여주기 실패");
            completionSource.TrySetResult(false);
            LoadRewardedAd();
        };

        // 광고 보여주기
        _rewardedAd.Show((Reward reward) =>
        {
            // 보상 콜백
            Debug.Log($"보상 획득 성공: {reward.Type} {reward.Amount}");
            isRewardEarned = true;
        });

        // 광고가 닫힐 때까지 대기 (게임 흐름 제어에 용이)
        return await completionSource.Task;
    }

    // ===================================================================
    //           ▼ [추가] 기존 코드 호환용 (Legacy Support) ▼
    // ===================================================================

    /// <summary>
    /// [호환용] 기존 콜백 방식의 코드를 지원하기 위한 래퍼 메서드입니다.
    /// 기존 코드: AdManager.Instance.ShowRewardedAd(() => { ... });
    /// </summary>
    public void ShowRewardedAd(Action onReward)
    {
        // UniTask 메서드를 호출하되, 결과를 기다리지 않고(Fire-and-Forget) 실행합니다.
        ShowRewardedAdWithCallback(onReward).Forget();
    }

    /// <summary>
    /// 비동기 결과를 받아서 기존 콜백(Action)을 실행시켜주는 내부 로직
    /// </summary>
    private async UniTaskVoid ShowRewardedAdWithCallback(Action onReward)
    {
        // 1. 우리가 만든 최신 비동기 메서드 호출 (여기서 광고 닫힐 때까지 대기함)
        // Static으로 만들었든, Instance로 만들었든 내부 로직을 호출하면 됩니다.
        bool isSuccess = await InternalShowRewardedAdAsync();

        // 2. 광고 시청 성공(true)일 때만 기존 콜백 실행
        if (isSuccess)
        {
            Debug.Log("[Legacy Support] 광고 성공! 기존 콜백을 실행합니다.");
            onReward?.Invoke();
        }
        else
        {
            Debug.LogWarning("[Legacy Support] 광고 실패 또는 취소됨. 콜백을 실행하지 않습니다.");
            // 필요하다면 실패 팝업 등을 여기서 띄울 수도 있음
        }
    }
}