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

    private string _adUnitId;

    protected override void Awake()
    {
        base.Awake();

        // 1. ID 설정 (에디터나 개발 빌드면 테스트 ID)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        _adUnitId = TEST_UNIT_ID;
#else
        _adUnitId = REAL_UNIT_ID;
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

        if (_consentController.CanRequestAds)
        {
            var initTcs = new UniTaskCompletionSource<bool>();
            MobileAds.Initialize(status =>
            {
                _isInitialized = true;
                initTcs.TrySetResult(true);
            });
            await initTcs.Task;

            LoadRewardedAd(); // 광고 미리 로드
        }
    }

    private void LoadRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        var adRequest = new AdRequest();
        RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("보상형 광고 로드 실패: " + error);
                return;
            }
            Debug.Log("보상형 광고 로드 성공");
            _rewardedAd = ad;
            _rewardedAd.OnAdFullScreenContentClosed += () => LoadRewardedAd(); // 닫히면 또 로드
        });
    }

    private async UniTask<bool> InternalShowRewardedAdAsync()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            var completionSource = new UniTaskCompletionSource<bool>();
            bool isRewardEarned = false;

            _rewardedAd.Show((Reward reward) =>
            {
                isRewardEarned = true;
                Debug.Log($"보상 획득: {reward.Type} {reward.Amount}");
            });

            // 광고 닫힘 대기 로직을 넣고 싶다면 여기서 completionSource 활용
            // 지금은 간단하게 보상 획득 여부만 반환
            return isRewardEarned;
        }
        else
        {
            Debug.LogWarning("광고가 준비되지 않았습니다. 다시 로드합니다.");
            LoadRewardedAd();
            return false;
        }
    }
}