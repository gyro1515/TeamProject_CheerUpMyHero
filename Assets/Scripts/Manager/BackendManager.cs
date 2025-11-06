using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudCode.GeneratedBindings.CheerUpMyHero.CloudCode;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Rendering.DebugUI;

public enum CommunicationStatus
{
    Success,
    Failure_NotInitialized,
    Failure_NetworkUnavailable,
    Failure_UserCancelled,
    // 필요하다면 다른 실패 원인 추가 (e.g., Failure_ServiceMaintenance)
}

public class BackendManager : SingletonMono<BackendManager>
{
    //서버와 통신하는 함수를 모아둘 예정

    //해야 할것: 고유한 재시도 로직
    //예: 재화 관련의 경우 writelock 다시 받아오기

    #region 작업 큐 관련 내부 클래스

    // 모든 요청이 구현해야 할 기본 인터페이스
    private interface IQueuedRequest
    {
        // 요청을 실행하고 완료될 때까지 기다리는 비동기 메서드
        UniTask ExecuteAsync();
    }

    // 반환 값이 없는 (UniTask) 비동기 작업을 위한 요청 클래스
    private class QueuedRequest : IQueuedRequest
    {
        private readonly Func<UniTask> _action;
        private readonly UniTaskCompletionSource<bool> _tcs;

        public UniTask Task => _tcs.Task;

        public QueuedRequest(Func<UniTask> action)
        {
            _action = action;
            _tcs = new UniTaskCompletionSource<bool>();
        }

        public async UniTask ExecuteAsync()
        {
            try
            {
                await _action();
                _tcs.TrySetResult(true);
            }
            catch (Exception e)
            {
                _tcs.TrySetException(e);
            }
        }
    }

    // 반환 값이 있는 (UniTask<T>) 비동기 작업을 위한 제네릭 요청 클래스
    private class QueuedRequest<T> : IQueuedRequest
    {
        // Func<UniTask<T>>: T 타입의 값을 반환하는 비동기 메서드를 담는 델리게이트
        private readonly Func<UniTask<T>> _action;
        // UniTaskCompletionSource<T>: UniTask<T>의 완료/실패/취소 상태를 수동으로 제어
        private readonly UniTaskCompletionSource<T> _tcs;

        public UniTask<T> Task => _tcs.Task;

        public QueuedRequest(Func<UniTask<T>> action)
        {
            _action = action;
            _tcs = new UniTaskCompletionSource<T>();
        }

        public async UniTask ExecuteAsync()
        {
            try
            {
                // 실제 비동기 작업을 실행하고 결과를 받아옴
                var result = await _action();
                // 작업이 성공적으로 완료되었음을 알리고 결과를 설정
                _tcs.TrySetResult(result);
            }
            catch (Exception e)
            {
                // 예외가 발생하면 Task를 실패 상태로 만듦
                _tcs.TrySetException(e);
            }
        }
    }
    #endregion

    #region 필드 모음
    //네트워크 캐시 변수들
    // 마지막으로 네트워크 확인 성공/실패 여부
    private static bool _isNetworkAvailableCache = false;
    // 마지막으로 네트워크 확인을 수행한 시간
    private static float _lastNetworkCheckTime = -5f;
 

    //쓰기 제한 WriteLock
    private Dictionary<string, string> writeLocks = new();

    

    //분석 켜짐 or 꺼짐
    public static bool IsAnalyticsCollectionStarted { get; private set; } = false;

    //현재 초기화 상태를 나타냄
    public UniTask<bool> InitializationTask { get; private set; }
    public static bool isInitializationCompeleted = false;

    //현재 초기화 상태를 제어
    private UniTaskCompletionSource<bool> _initializationTcs;

    private readonly Queue<IQueuedRequest> _requestQueue = new Queue<IQueuedRequest>();
    // 큐가 현재 처리 중인지 여부를 나타내는 플래그
    private bool _isProcessingQueue = false;
    // 여러 스레드에서 동시에 큐에 접근하는 것을 방지하기 위한 잠금 객체 (안전장치)
    private readonly object _queueLock = new object();

    #endregion

    protected override void Awake()
    {
        base.Awake();


        _initializationTcs = new UniTaskCompletionSource<bool>();
        InitializationTask = _initializationTcs.Task;
        InitializeAndLoginAsync().Forget();

    }

    public static async UniTask CheckInterentAsync()
    {
        //처음 말고 두번째 부터 초기화면으로 왔을때 실행
        if (!isInitializationCompeleted)
        {
            return;
        }

        try
        {

            while (!await IsNetworkAvailableAsync(true))
            {
                Debug.LogWarning($"인터넷 연결에 실패했습니다.");

                // UIManager를 통해 재시도 팝업을 띄우고 사용자 응답을 기다립니다.
                bool userWantsToRetry = await UIManager.Instance.GetUI<NoticeNetworkError>().ShowAndWaitForResponse("인터넷 연결을 확인해주세요.\n다시 시도하시겠습니까?", true);

                if (userWantsToRetry)
                {
                    Debug.Log("사용자가 재시도를 선택했습니다. 네트워크 상태를 다시 확인합니다.");
                    await UniTask.Delay(500); // 잠시 후 재시도
                }
            }

            await PlayerDataManager.Instance.InitializeResourcesAsync();
            await PlayerDataManager.Instance.LoadDataFromCloundAsync();
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
        }
    }


    //맨 처음 시작시 단 한번만 호출됨(UGS 초기화는 두 번 안됨.)
    async UniTaskVoid InitializeAndLoginAsync()
    {
        try
        {
            //0.인터넷 확인
            while (!await IsNetworkAvailableAsync(true))
            {
                Debug.LogWarning($"인터넷 연결에 실패했습니다.");

                // UIManager를 통해 재시도 팝업을 띄우고 사용자 응답을 기다립니다.
                bool userWantsToRetry = await UIManager.Instance.GetUI<NoticeNetworkError>().ShowAndWaitForResponse("인터넷 연결을 확인해주세요.\n다시 시도하시겠습니까?", true);

                if (userWantsToRetry)
                {
                    Debug.Log("사용자가 재시도를 선택했습니다. 네트워크 상태를 다시 확인합니다.");
                    await UniTask.Delay(500); // 잠시 후 재시도
                }
            }

            var options = new InitializationOptions();

#if USERTEST && !UNITY_EDITOR
            options.SetEnvironmentName("usertest");
#else
            options.SetEnvironmentName("dev");
#endif

            // 1. UGS 서비스 초기화
            await UnityServices.InitializeAsync(options);

            Debug.Log($"<color=cyan>UGS 초기화 성공!</color>");

            // 2. 익명 로그인 시도 (초기화가 성공해야 호출 가능)
            // 현재는 자동으로 하지만, 나중에 google 계정 연동이라던가 생기면 분리 필요

            // 이미 로그인 되어있는지 확인 후, 안되어 있을 때만 시도
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                bool signInSuccess = await SignInAnonymouslyAsync();

                if (!signInSuccess)
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

            //4. 재화, 데이터 관련 세팅
            await StartEconomyAndClound();
            

            _initializationTcs.TrySetResult(true);

            Debug.Log($"<color=cyan>모든 서비스 준비 완료!</color>");

            //플레이어 데이터 매니저가 비동기를 가질 때까지는 외부에서 자원 넣어줘야 할듯
            await PlayerDataManager.Instance.InitializeResourcesAsync();
            await PlayerDataManager.Instance.LoadDataFromCloundAsync();
            isInitializationCompeleted = true;
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

            UIManager.Instance.GetUI<StartUI>().SetPlayerId(AuthenticationService.Instance.PlayerId);

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

    //Economy, CloundSave 관련
    async UniTask StartEconomyAndClound()
    {
        //Economy 서비스 초기화
        await EconomyService.Instance.Configuration.SyncConfigurationAsync();

        Debug.Log("Economy 서비스 초기화");
    }


    #region 큐 관련 메서드
    private async UniTaskVoid ProcessQueueAsync()
    {
        while (true)
        {
            IQueuedRequest currentRequest;

            // lock을 이용해 큐에서 작업을 꺼내는 동안 다른 스레드의 접근을 막음
            lock (_queueLock)
            {
                // 큐에 더 이상 처리할 작업이 없으면 루프 종료
                if (_requestQueue.Count == 0)
                {
                    _isProcessingQueue = false; // 처리 완료 상태로 변경
                    return;
                }
                currentRequest = _requestQueue.Dequeue();
            }

            try
            {
                // 큐에서 꺼낸 작업을 실행하고 완료될 때까지 기다림
                await currentRequest.ExecuteAsync();
            }
            catch (Exception e)
            {
                // ExecuteAsync 내부에서 예외를 처리하지만, 만약을 위한 로깅
                Debug.LogError($"[BackendManager] 요청 처리 중 예상치 못한 오류 발생: {e.Message}");
            }
        }
    }

    private UniTask<T> EnqueueRequestAsync<T>(Func<UniTask<T>> action, string apiName)
    {
        // action을 직접 전달하는 대신, ExecuteWithRetryAsync로 감싸서 전달합니다.
        var request = new QueuedRequest<T>(() => ExecuteWithRetryAsync(action, apiName));

        lock (_queueLock)
        {
            // 큐에 요청 추가
            _requestQueue.Enqueue(request);

            // 현재 큐가 처리 중이 아니라면, 새로운 처리 루프 시작
            if (!_isProcessingQueue)
            {
                _isProcessingQueue = true;
                ProcessQueueAsync().Forget(); // Forget()으로 호출. 이 루프의 완료를 기다리지 않음.
            }
        }

        // 외부 호출자는 이 Task를 await하게 됨.
        // 이 Task는 나중에 ProcessQueueAsync 루프 안에서 완료됨.
        return request.Task;
    }

    // 반환 값이 없는 버전을 위한 오버로딩
    private UniTask EnqueueRequestAsync(Func<UniTask> action, string apiName)
    {
        var request = new QueuedRequest(() => ExecuteWithRetryAsync(action, apiName));

        lock (_queueLock)
        {
            _requestQueue.Enqueue(request);
            if (!_isProcessingQueue)
            {
                _isProcessingQueue = true;
                ProcessQueueAsync().Forget();
            }
        }
        return request.Task;
    }
    #endregion

    #region 재시도 로직
    private async UniTask<T> ExecuteWithRetryAsync<T>(Func<UniTask<T>> action, [CallerMemberName] string apiName = "")
    {
        while (true)
        {
            try
            {
                // 실제 작업을 실행하고 성공하면 결과를 즉시 반환
                return await action();
            }
            // 재화 WriteLock 충돌 시 자동 재시도
            catch (EconomyException ex) when (ex.Reason == EconomyExceptionReason.Conflict)
            {
                Debug.LogWarning($"[{apiName}] 재화 충돌 감지. 데이터 동기화 후 자동 재시도합니다.");
                await InternalLoadEconomyData(); // 최신 재화 정보(WriteLock 포함) 갱신
                continue; // 팝업 없이 즉시 재시도
            }
            // UGS의 일반적인 네트워크 오류
            catch (RequestFailedException ex)
            {
                Debug.LogError($"[{apiName}] 네트워크 오류: {ex.Message}");
                // UIManager를 통해 팝업을 띄우고 사용자 선택을 기다림
                bool shouldRetry = await UIManager.Instance.GetUI<NoticeNetworkError>().ShowAndWaitForResponse("서버 통신 실패, 네트워크 연결이 불안정합니다.\n다시 시도하시겠습니까?");

                if (shouldRetry)
                {
                    continue; // while 루프의 처음으로 돌아가 재시도
                }
                // 사용자가 '취소'를 선택하면 새로운 예외를 발생시켜 외부로 알림
                throw new OperationCanceledException("사용자가 작업을 취소했습니다.");
            }
            
            catch (Exception e)
            {
                // 재시도 대상이 아닌 다른 모든 예외는 그대로 다시 throw
                Debug.LogError($"[{apiName}] 처리 불가능한 예외: {e.Message}");
                throw;
            }
        }
    }

    // 반환값이 없는 UniTask 버전을 위한 오버로딩
    private async UniTask ExecuteWithRetryAsync(Func<UniTask> action, [CallerMemberName] string apiName = "")
    {
        await ExecuteWithRetryAsync<bool>(async () =>
        {
            await action();
            return true; // 제네릭 메서드 재사용을 위한 더미 값 반환
        }, apiName);
    }
    #endregion


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
    private static async UniTask<bool> IsNetworkAvailableAsync(bool forceCheck)
    {
#if UNITY_WEBGL
        return true;
#else
        //캐시가 만료되지 않았다면, 이전 네트워크 결과 불러오기
        if (!forceCheck && Time.realtimeSinceStartup - _lastNetworkCheckTime < Constants.NETWORK_CACHE_DURATION && _isNetworkAvailableCache)
        {
            return true;
        }

        // 캐시가 만료되었거나, 강제 확인이 요청된 경우 실제 네트워크 확인 수행
        _lastNetworkCheckTime = Time.realtimeSinceStartup;

        // 1차 확인: 기기상 인터넷 연결 여부
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning($"기기의 인터넷을 켜주세요");
            _isNetworkAvailableCache = false;
            return false;
        }

        // 2차 확인: 실제 인터넷 네트워크 연결 여부
        var request = UnityWebRequest.Head(Constants.NETWORK_CHECK_URL);
        request.timeout = 4;

        try
        {
            await request.SendWebRequest();
            bool success = request.result == UnityWebRequest.Result.Success;
            _isNetworkAvailableCache = success; // 결과 캐싱

            if (!success)
            {
                Debug.LogWarning($"네트워크 확인 실패: {request.error}");
                return false;
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
#endif

    }


    // 서버 통신 가능여부 종합 체크
    // 반환값에 enum을 추가하면 실패 이유도 같이 반환 가능
    private static async UniTask<CommunicationStatus> CanCommunicateAsync(string apiKey, bool forceCheck = false) //apikey = 메서드 이름
    {
        // 1. 초기화 확인
        if (!await EnsureInstanceAndInitializedAsync()) 
        {
            Debug.LogWarning($"초기화가 완료되지 않았습니다.");
            return CommunicationStatus.Failure_NotInitialized;
        }
            
        // 2. 네트워크 확인
        while (!await IsNetworkAvailableAsync(forceCheck))
        {
            Debug.LogWarning($"인터넷 연결에 실패했습니다.");

            // UIManager를 통해 재시도 팝업을 띄우고 사용자 응답을 기다립니다.
            bool userWantsToRetry = await UIManager.Instance.GetUI<NoticeNetworkError>().ShowAndWaitForResponse("인터넷 연결을 확인해주세요.\n다시 시도하시겠습니까?");

            if (userWantsToRetry)
            {
                Debug.Log("사용자가 재시도를 선택했습니다. 네트워크 상태를 다시 확인합니다.");
                await UniTask.Delay(500); // 잠시 후 재시도
            }
            else
            {
                Debug.Log("사용자가 재시도를 취소했습니다.");
                return CommunicationStatus.Failure_UserCancelled; // 사용자가 취소했음을 반환
            }
        }


        // 3. 서비스 상태 확인 (점검 등)

        // 4. 과도한 호출 방지
        // 각 api가 호출되는 시점 저장하고 비교

        // 5. 로그인 유효 확인

        return CommunicationStatus.Success;
    }

#endregion


    // ===================================================================
    //           ▼ Public Static API (외부에 노출되는 깔끔한 창구) ▼
    // ===================================================================



    //예시 코드
    public static async UniTask SaveDataAsync(Dictionary<string, object> data)
    {        
        var status = await CanCommunicateAsync(nameof(SaveDataAsync));
        if (status != CommunicationStatus.Success)
        {
            // 성공이 아닌 모든 경우에 예외를 던지고 종료
            throw new InvalidOperationException($"서버 통신 사전 검사 실패: {status}");
        }
        await Instance.EnqueueRequestAsync(() => Instance.InternalSaveDataAsync(data), nameof(SaveDataAsync));
    }

    public static async UniTask<AllCloudData> LoadDataAsync()
    {
        var status = await CanCommunicateAsync(nameof(LoadDataAsync));
        if (status != CommunicationStatus.Success)
        {
            throw new InvalidOperationException("서버와 통신할 수 없는 상태입니다.");
        }
        return await Instance.EnqueueRequestAsync(() => Instance.InternalLoadDataAsync(), nameof(LoadDataAsync));
    }

    public static async UniTask<GachaResult> OneNormalGachaAsync()
    {
        var status = await CanCommunicateAsync(nameof(OneNormalGachaAsync));
        if (status != CommunicationStatus.Success)
        {
            throw new InvalidOperationException("서버와 통신할 수 없는 상태입니다.");
        }

        return await Instance.EnqueueRequestAsync(() => Instance.InternalOneNormalGachaAsync(), nameof(OneNormalGachaAsync));
    }

    public static async UniTask<GachaResult> OnePickupGachaAsync()
    {
        var status = await CanCommunicateAsync(nameof(OnePickupGachaAsync));
        if (status != CommunicationStatus.Success)
        {
            throw new InvalidOperationException("서버와 통신할 수 없는 상태입니다.");
        }

        return await Instance.EnqueueRequestAsync(() => Instance.InternalOnePickupGachaAsync(), nameof(OnePickupGachaAsync));
    }


    public static async UniTask<GachaResult> TenNormalGachaAsync()
    {
        var status = await CanCommunicateAsync(nameof(TenNormalGachaAsync));
        if (status != CommunicationStatus.Success)
        {
            throw new InvalidOperationException("서버와 통신할 수 없는 상태입니다.");
        }

        return await Instance.EnqueueRequestAsync(() => Instance.InternalTenNormalGachaAsync(), nameof(TenNormalGachaAsync));
    }

    public static async UniTask<GachaResult> TenPickupGachaAsync()
    {
        var status = await CanCommunicateAsync(nameof(TenPickupGachaAsync));
        if (status != CommunicationStatus.Success)
        {
            throw new InvalidOperationException("서버와 통신할 수 없는 상태입니다.");
        }

        return await Instance.EnqueueRequestAsync(() => Instance.InternalTenPickupGachaAsync(), nameof(TenPickupGachaAsync));
    }


    //현재 플레이어 데이터매니저를 통하는데, 리팩토링 예정
    //받는쪽에서 널이면 팝업 띄워야함
    //사실 이렇게 하지말고 팝업처리까지 백엔드매니저에서 해야함
    public static async UniTask<Dictionary<ResourceType, int>> LoadEconomyData()
    {
        var status = await CanCommunicateAsync(nameof(LoadEconomyData));
        if (status != CommunicationStatus.Success)
        {
            throw new InvalidOperationException("서버와 통신할 수 없는 상태입니다.");
        }

        return await Instance.EnqueueRequestAsync(() => Instance.InternalLoadEconomyData(), nameof(LoadEconomyData));
    }

    //나중에 서버로 이사가야 함
    public static async UniTask ChangeEconomy(string id, int amount)
    {
        var status = await CanCommunicateAsync(nameof(ChangeEconomy));
        if (status != CommunicationStatus.Success)
        {
            throw new InvalidOperationException("서버와 통신할 수 없는 상태입니다.");
        }

        await Instance.EnqueueRequestAsync(() => Instance.InternalChangeEnconmyAsync(id, amount), nameof(ChangeEconomy));
    }


    //서버 ID : 자원 enum 매칭 
    public static string EconomyEnumToId(ResourceType resource)
    {
        string result = string.Empty;
        switch (resource)
        {
            case ResourceType.Gold:
                result = Constants.GOLD_ID;
                break;
            case ResourceType.Wood:
                result = Constants.WOOD_ID;
                break;
            case ResourceType.Iron:
                result = Constants.IRON_ID;
                break;
            case ResourceType.Ticket:
                result = Constants.TICKET_ID;
                break;
            case ResourceType.MagicStone:
                result = Constants.MAGICSTONE_ID;
                break;
            case ResourceType.Bm:
                result = Constants.BM_ID;
                break;
            case ResourceType.Food:
                result = string.Empty;
                Debug.LogWarning("Food는 서버에 저장되지 않으므로 빈값을 반환합니다.");
                break;
        }
        return result;
    }

    public static bool EconomyIdToEnum(string id, out ResourceType resource)
    {
        bool result = true;

        switch (id)
        {
            case Constants.GOLD_ID:
                resource = ResourceType.Gold;
                break;
            case Constants.WOOD_ID:
                resource = ResourceType.Wood;
                break;
            case Constants.IRON_ID:
                resource = ResourceType.Iron;
                break;
            case Constants.TICKET_ID:
                resource = ResourceType.Ticket;
                break;
            case Constants.MAGICSTONE_ID:
                resource = ResourceType.MagicStone;
                break;
            case Constants.BM_ID:
                resource = ResourceType.Bm;
                break;
            default:
                resource = ResourceType.Food;
                Debug.LogWarning($"Unknown resource type: {id}");
                result = false;
                break;
        }
        return result;
    }

    // ===================================================================
    //           ▼ Private Instance Implementations (실제 로직) ▼
    // ===================================================================
    private async UniTask InternalSaveDataAsync(Dictionary<string, object> data)
    {
        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            throw;
        }
    }

    private async UniTask<AllCloudData> InternalLoadDataAsync()
    {
        AllCloudData cloudData = new();

        string normalPityKey = null;
        string pickiupPityKey = null;
        
        try
        {
            //서버에서 "동적 키"값 가져와서 클라우드에서 다시 플레이어에서 다시 데이터 가져옴
            //그러나 아직 배너 아이디를 상수값으로 사용하고 있기 때문에, 완벽하지 않음 => 완벽하게 하려면 RemoteConfig로 배너 이름을 가지고 있고, 그걸 처음에 불러와야 함
            //일단은 가챠 배너(노말, 픽업 두가지) 자체가 한동안 달라지지 않는 것을 가정하고 구현. 배너 내부 데이터는 현재 RemoteConfig로 바로 수정 가능
            List<GachaBannerClientInfo> bannerInfoList = await InternalLoadGachaPityAsync();

            var bannerDictionary = bannerInfoList.ToDictionary(b => b.BannerId);

            if (bannerDictionary.TryGetValue(Constants.NORMAL_GACHA_KEY, out var normalBannerInfo))
            {
                // '상시 모집' 배너 처리 로직
                normalPityKey = Constants.PITY_COUNT_KEY_PREFIX + normalBannerInfo.BannerId;
                cloudData.NormalPityThreshold = normalBannerInfo.PityThreshold;
            }

            if (bannerDictionary.TryGetValue(Constants.PICKUP_GACHA_KEY, out var pickupBannerInfo))
            {
                // '픽업 모집' 배너 처리 로직
                pickiupPityKey = Constants.PITY_COUNT_KEY_PREFIX + pickupBannerInfo.BannerId;
                cloudData.PickupPityThreshold = pickupBannerInfo.PityThreshold; 

            }

            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { Constants.PLAYER_DATA_KEY, normalPityKey, pickiupPityKey });

            if (playerData.TryGetValue(Constants.PLAYER_DATA_KEY, out var savedata))
            {
                cloudData.PlayerSaveData = savedata.Value.GetAs<PlayerSaveData>();  
            }

            if (playerData.TryGetValue(normalPityKey, out var normalPityAmount))
            {
                cloudData.NormalPity = normalPityAmount.Value.GetAs<int>();
            }
            else
                cloudData.NormalPity = 0;
            Debug.Log(Constants.PITY_COUNT_KEY_PREFIX + normalPityKey);

            if (playerData.TryGetValue(pickiupPityKey, out var pickupPityAmount))
            {
                cloudData.PickupPity = pickupPityAmount.Value.GetAs<int>();
            }
            else
                cloudData.NormalPity = 0;

            return cloudData;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            throw;
        }

    }

    private async UniTask<List<GachaBannerClientInfo>> InternalLoadGachaPityAsync()
    {
        try
        {
            //클라우드에서 가챠 배너 가져오기
            var module = new GachaModuleV2Bindings(CloudCodeService.Instance);
            var result = await module.GetGachaBanners();

            return result;
        }
        catch (CloudCodeException exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }

    private async UniTask<GachaResult> InternalOneNormalGachaAsync()
    {
        try
        {
            //클라우드에서 가챠 실행
            var module = new GachaModuleV2Bindings(CloudCodeService.Instance);
            var result = await module.DrawGachaOne(Constants.NORMAL_GACHA_KEY);

            return result;
        }
        catch (CloudCodeException exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }

    private async UniTask<GachaResult> InternalOnePickupGachaAsync()
    {
        try
        {
            //클라우드에서 가챠 실행
            var module = new GachaModuleV2Bindings(CloudCodeService.Instance);
            var result = await module.DrawGachaOne(Constants.PICKUP_GACHA_KEY);

            return result;
        }
        catch (CloudCodeException exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }

    private async UniTask<GachaResult> InternalTenNormalGachaAsync()
    {
        try
        {
            //클라우드에서 가챠 실행
            var module = new GachaModuleV2Bindings(CloudCodeService.Instance);
            var result = await module.DrawGachaTen(Constants.NORMAL_GACHA_KEY);

            return result;
        }
        catch (CloudCodeException exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }

    private async UniTask<GachaResult> InternalTenPickupGachaAsync()
    {
        try
        {
            //클라우드에서 가챠 실행
            var module = new GachaModuleV2Bindings(CloudCodeService.Instance);
            var result = await module.DrawGachaTen(Constants.PICKUP_GACHA_KEY);

            return result;
        }
        catch (CloudCodeException exception)
        {
            Debug.LogException(exception);
            throw;
        }
    }


    private async UniTask<Dictionary<ResourceType, int>> InternalLoadEconomyData()
    {
        try 
        {
            GetBalancesResult initialBalancesResult = await EconomyService.Instance.PlayerBalances.GetBalancesAsync();
            List<PlayerBalance> playerBalances = initialBalancesResult.Balances;

            Dictionary<ResourceType, int> resourcesValue = new();
            foreach (PlayerBalance balance in playerBalances)
            {
                if (EconomyIdToEnum(balance.CurrencyId, out ResourceType resource))
                {
                    resourcesValue.Add(resource, Convert.ToInt32(balance.Balance)); // economy 최대값을 설정해 두어서 int 초과할 일은 없음
                    writeLocks[balance.CurrencyId] = balance.WriteLock;
                }
                else
                {
                    throw new InvalidOperationException("오류: 자원을 불러오는 내부 로직에 문제가 있습니다.");
                }
            }

            return resourcesValue;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }



    private async UniTask InternalChangeEnconmyAsync(string id, int amount)
    {
        
        string currentWriteLock = string.Empty;

        if (writeLocks.ContainsKey(id))
        {
            currentWriteLock = writeLocks[id];
        }
        else
        {
            Debug.LogWarning("재화가 동기화되지 않았습니다");
            //그냥 넘어가게 하면, 알아서 오류를 뱉을 것. EconomyException을 새로 정의 못해서 이렇게 함.
        }

        if (amount == 0)
        {
            Debug.LogWarning("0만큼 변할 수 없습니다.");
            return;
        }



        try
        {
            if (amount > 0)
            {
                var incrementOptions = new IncrementBalanceOptions { WriteLock = currentWriteLock };
                PlayerBalance incrementResult = await EconomyService.Instance.PlayerBalances.IncrementBalanceAsync(id, amount, incrementOptions);
                writeLocks[id] = incrementResult.WriteLock;
            }
            else
            {
                var decrementOptions = new DecrementBalanceOptions { WriteLock = currentWriteLock };
                PlayerBalance decrementResult = await EconomyService.Instance.PlayerBalances.DecrementBalanceAsync(id, -amount, decrementOptions);
                writeLocks[id] = decrementResult.WriteLock;
            }
        }

        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
        
    }

}

public class StageResultEvent : Unity.Services.Analytics.Event
{
    public StageResultEvent() : base("StageResult")
    {
    }

    public bool isHeroArriveStage_Bool { set { SetParameter(Constants.IS_HERO_ARRIVE, value); } }
    public bool isStageChallenge_Bool { set { SetParameter(Constants.IS_STAGE_CHALLENGE, value); } }
    public bool isStageCleard_Bool { set { SetParameter(Constants.IS_STAGE_CLEARED, value); } }
    public bool isStageClearedButTryAgain_Bool { set { SetParameter(Constants.IS_STAGE_CLEARED_BUT_TRY, value); } }
    public string stageChallengeData_String { set { SetParameter(Constants.STAGE_CHALLENGE_DATA, value); } }
    public string stageConstruction_String { set { SetParameter(Constants.STAGE_CONSTRUCTION, value); } }
    public int stageDestinyId_Int { set { SetParameter(Constants.STAGE_DESTNIY_ID, value); } }
    public int stageId_Int { set { SetParameter(Constants.STAGE_ID, value); } }
    public int stageSupplyLevel_Int { set { SetParameter(Constants.STAGE_SUPPLY_LEVEL, value); } }
    public float stageTimeTaken_Float { set { SetParameter(Constants.STAGE_TIME_TAKEN, value); } }
    public string stageUsedArtifat_String { set { SetParameter(Constants.STAGE_USED_ARTIFACT, value); } }
    public string stageUsedUnit_String { set { SetParameter(Constants.STAGE_USED_UNIT, value); } }
}


//클라우드 세이브에 있는 모든 데이터 모음
public class AllCloudData
{
    public PlayerSaveData PlayerSaveData;

    public int NormalPity;
    public int NormalPityThreshold;

    public int PickupPity;
    public int PickupPityThreshold;

}

