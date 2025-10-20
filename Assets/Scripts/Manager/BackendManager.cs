using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;

public class BackendManager : SingletonMono<BackendManager>
{
    //서버와 통신하는 함수를 모아둘 예정

    //추가 예정
    //플레이어 id
    //플레이어 자원
    //분석 결과 보내기
    //...

    #region 필드 모음
    //네트워크 캐시 변수들
    // 마지막으로 네트워크 확인 성공/실패 여부
    private static bool _isNetworkAvailableCache = false;
    // 마지막으로 네트워크 확인을 수행한 시간
    private static float _lastNetworkCheckTime = -5f;
    // 캐시 유효 시간 (초)
    private const float NETWORK_CACHE_DURATION = 5.0f;
    // 인터넷 확인용 주소
    private const string NETWORK_CHECK_URL = "https://connectivitycheck.gstatic.com/generate_204";

    //분석 켜짐 or 꺼짐
    public static bool IsAnalyticsCollectionStarted { get; private set; } = false;

    //현재 초기화 상태를 나타냄
    public UniTask<bool> InitializationTask { get; private set; }
    //UniTask.State를 가짐: Pending, Succeeded, Faulted, Canceled.

    //현재 초기화 상태를 제어
    private UniTaskCompletionSource<bool> _initializationTcs;

    #endregion

    protected override void Awake()
    {
        base.Awake();

        _initializationTcs = new UniTaskCompletionSource<bool>();
        InitializationTask = _initializationTcs.Task;
        InitializeAndLoginAsync().Forget();

    }


    //맨 처음 시작시 단 한번만 호출됨(UGS 초기화는 두 번 안됨.)
    async UniTaskVoid InitializeAndLoginAsync()
    {
        try
        {
            // 1. UGS 서비스 초기화
            await UnityServices.InitializeAsync();

            Debug.Log($"<color=cyan>UGS 초기화 성공!</color>");

            // 2. 익명 로그인 시도 (초기화가 성공해야 호출 가능)
            // 현재는 자동으로 하지만, 나중에 google 계정 연동이라던가 생기면 분리 필요

            // 이미 로그인 되어있는지 확인 후, 안되어 있을 때만 시도
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                bool signInSuccess = await SignInAnonymouslyAsync();

                if (!signInSuccess )
                {
                    throw new Exception("익명 로그인에 실패했습니다.");
                }
            }
            else
            {
                Debug.Log($"<color=yellow>이미 로그인되어 있습니다. Player ID: {AuthenticationService.Instance.PlayerId}</color>");
            }

            //3. AnalyticsData 활성화
            StartAnalytics();

            _initializationTcs.TrySetResult(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"<color=red>BackendManager 초기화 실패: {e.Message}</color>");
            Debug.LogException(e);
            _initializationTcs.TrySetResult(false);
        }
    }

    //로그인
    async UniTask<bool> SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            // 로그인 성공시 결과 표시
            Debug.Log($"<color=cyan>익명 로그인 성공! PlayerID: {AuthenticationService.Instance.PlayerId}</color>");

            return true;
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);

            return false;
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);

            return false;
        }
    }

    //AnalyticsData 수집 
    //나중에 추가해야 될것: 최초 실행시, 데이터 수집 동의 여부. 테스트때는 없어도 무방
    void StartAnalytics()
    {
        AnalyticsService.Instance.StartDataCollection();
        IsAnalyticsCollectionStarted = true;

        Debug.Log($"<color=cyan>데이터 수집 동의 완료. 분석 데이터가 자동으로 서버에 전송됩니다.</color>");
    }


    #region 서버와 통신 가능 여부 체크
    // 서비스 초기화가 완료될 때까지 기다림
    public static async UniTask<bool> EnsureInstanceAndInitializedAsync()
    {
        var instance = Instance;
        if (instance == null)
        {
            // 앱 종료 시점 등에서 발생할 수 있는 NullReferenceException 방지
            Debug.LogError("BackendManager 인스턴스를 가져올 수 없습니다. 앱이 종료되는 중일 수 있습니다.");
            return false;
        }

        return await Instance.InitializationTask;
    }

    //인터넷 연결 선제적 확인
    private static async UniTask<bool> IsNetworkAvailableAsync(bool forceCheck = false)
    {
        //캐시가 만료되지 않았다면, 이전 네트워크 결과 불러오기
        if (!forceCheck && Time.realtimeSinceStartup - _lastNetworkCheckTime < NETWORK_CACHE_DURATION)
        {
            return _isNetworkAvailableCache;
        }

        // 캐시가 만료되었거나, 강제 확인이 요청된 경우 실제 네트워크 확인 수행
        _lastNetworkCheckTime = Time.realtimeSinceStartup;

        // 1차 확인: 기기상 인터넷 연결 여부
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            _isNetworkAvailableCache = false;
            return false;
        }

        // 2차 확인: 실제 인터넷 네트워크 연결 여부
        var request = UnityWebRequest.Head(NETWORK_CHECK_URL);
        request.timeout = 4;

        try
        {
            await request.SendWebRequest();
            bool success = request.result == UnityWebRequest.Result.Success;
            _isNetworkAvailableCache = success; // 결과 캐싱

            if (!success)
            {
                Debug.LogWarning($"네트워크 확인 실패: {request.error}");
            }
            return success;
        }
        catch (Exception ex)
        {
            Debug.LogError($"네트워크 확인 중 예외 발생: {ex.Message}");
            _isNetworkAvailableCache = false; // 예외 발생 시 실패로 캐싱
            return false;
        }
        finally
        {
            request.Dispose(); //네트워크 관련 등 GC가 처리 못하는 것 수동 처리
        }
    }


    // 서버 통신 가능여부 종합 체크
    // 반환값에 enum을 추가하면 실패 이유도 같이 반환 가능
    private static async UniTask<bool> CanCommunicateAsync(string apiKey) //apikey = 메서드 이름
    {
        // 1. 초기화 확인
        if (!await EnsureInstanceAndInitializedAsync())
            return false;

        // 2. 네트워크 확인
        if (!await IsNetworkAvailableAsync())
            return false;
        // 3. 서비스 상태 확인 (점검 등)

        // 4. 과도한 호출 방지
        // 각 api가 호출되는 시점 저장하고 비교

        // 5. 로그인 유효 확인

        return true;
    }

    #endregion


    // ===================================================================
    //           ▼ Public Static API (외부에 노출되는 깔끔한 창구) ▼
    // ===================================================================



    //예시 코드
    public static async UniTask SaveDataAsync(Dictionary<string, object> data)
    {
        if (!await CanCommunicateAsync(nameof(SaveDataAsync)))
        {
            Debug.LogError("서버 연결 불가: 데이터 저장 불가");
            //이유도 같이 나오게 할 예정

            return;
        }
        await Instance.InternalSaveDataAsync(data);
    }

    //Analytic는 인터넷 연결 없이도 저장 후 데이터 전송
    public static async UniTask SendStageAnalyticsAsync(Dictionary<string, object> data) //일단 딕셔너리로 적긴 했는데, struct 활용 예정
    {
        //서비스 초기화 여부랑 Analytic 활성화 여부만 체크
        //Analytic는 사용자가 제공 거부를 할 수 있으므로 강제로 키지 않음
        if (!IsAnalyticsCollectionStarted)
        {
            Debug.Log("Analytic가 실행되지 않아 통계가 전송되지 않습니다.");
            return;
        }

        if (!await EnsureInstanceAndInitializedAsync())
            return;

        await Instance.InternalSendStageAnalyticsAsync(data);
    }


    public static async UniTask<int> OneNormalGachaAsync()
    {
        if (!await CanCommunicateAsync(nameof(SaveDataAsync)))
        {
            Debug.LogError("서버 연결 불가: 데이터 저장 불가");
            //이유도 같이 나오게 할 예정

            return -1;
        }

        return await Instance.InternalOneNormalGachaAsync();
    }



    // ===================================================================
    //           ▼ Private Instance Implementations (실제 로직) ▼
    // ===================================================================
    private async UniTask InternalSaveDataAsync(Dictionary<string, object> data)
    {
        // TODO: 실제 클라우드 저장 로직 구현 필요
        Debug.LogWarning("InternalSaveDataAsync가 아직 구현되지 않았습니다.");
        await UniTask.CompletedTask; // 컴파일러 경고 제거 및 비동기 시그니처 유지 용도
    }

    private async UniTask InternalLoadDataAsync(Dictionary<string, object> data)
    {
        // TODO: 실제 클라우드 로드 로직 구현 필요
        Debug.LogWarning("InternalLoadDataAsync가 아직 구현되지 않았습니다.");
        await UniTask.CompletedTask; // 컴파일러 경고 제거 및 비동기 시그니처 유지 용도
    }

    private async UniTask InternalSendStageAnalyticsAsync(Dictionary<string, object> data)
    {
        // TODO: 실제 통계 전송 작업 로직 필요
        Debug.LogWarning("InternalSendStageAnalyticsAsync가 아직 구현되지 않았습니다.");
        await UniTask.CompletedTask; // 컴파일러 경고 제거 및 비동기 시그니처 유지 용도
    }

    private async UniTask<int> InternalOneNormalGachaAsync()
    {
        try
        {
            //클라우드에서 가챠 실행
            var module = new GachaModuleBindings(CloudCodeService.Instance);
            var result = await module.DrawGachaItem();

            return result;
        }
        catch (CloudCodeException exception)
        {
            Debug.LogException(exception);
            //실패시 -1 반환
            return -1;
        }
    }

}

