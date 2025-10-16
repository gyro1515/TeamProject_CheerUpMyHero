using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class BackendManager : SingletonMono<BackendManager>
{
    //서버와 통신하는 함수를 모아둘 예정

    //추가 예정
    //플레이어 id
    //플레이어 자원
    //분석 결과 보내기



    //현재 초기화 상태를 나타냄
    public Task<bool> InitializationTask { get; private set; }
    //enum으로 아래처럼 풀어서 쓸 수도 있음
    //public enum BackendState
    //{
    //    NotInitialized, // 초기화 시작 전
    //    Initializing,   // 초기화 진행 중
    //    Initialized,    // 초기화 성공 및 서비스 가능 상태
    //    Failed          // 초기화 실패
    //}

    //현재 초기화 상태를 제어
    private TaskCompletionSource<bool> _initializationTcs;

    protected override void Awake()
    {
        base.Awake();

        if (InitializationTask == null)
        {
            _initializationTcs = new TaskCompletionSource<bool>();
            InitializationTask = _initializationTcs.Task;
            InitializeAndLoginAsync();
        }
    }


    //맨 처음 시작시 단 한번만 호출됨(UGS 초기화는 두 번 안됨.)
    async void InitializeAndLoginAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();

            Debug.Log($"<color=cyan>UGS 초기화 성공!</color>");

            // 2. 익명 로그인 시도 (초기화가 성공해야 호출 가능)

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

            _initializationTcs.SetResult(true);
        }
        catch (Exception e)
        {
            Debug.LogError($"<color=red>BackendManager 초기화 실패: {e.Message}</color>");
            Debug.LogException(e);
            _initializationTcs.SetResult(false);
        }
    }

    async Task<bool> SignInAnonymouslyAsync()
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

    // 초기화가 완료될 때까지 기다림
    public static async Task<bool> EnsureInstanceAndInitializedAsync()
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

    // 서버 통신 가능여부 종합 체크
    private static async Task<bool> CanCommunicateAsync(string apiKey)
    {
        // 1. 초기화 확인
        if (!await EnsureInstanceAndInitializedAsync())
            return false;

        // 2. 네트워크 확인

        // 3. 서비스 상태 확인 (점검 등)

        // 4. 과도한 호출 방지
        // 각 api가 호출되는 시점 저장하고 비교

        return true;
    }



    // ===================================================================
    //           ▼ Public Static API (외부에 노출되는 깔끔한 창구) ▼
    // ===================================================================

    //예시 코드
    public static async Task SaveDataAsync(Dictionary<string, object> data)
    {
        if (!await CanCommunicateAsync(nameof(SaveDataAsync)))
        {
            Debug.LogError("백엔드 매니저 준비 실패: 데이터 저장 불가");
            return;
        }
        await Instance.InternalSaveDataAsync(data);
    }

    // ===================================================================
    //           ▼ Private Instance Implementations (실제 로직) ▼
    // ===================================================================
    private async Task InternalSaveDataAsync(Dictionary<string, object> data)
    {
        // TODO: 실제 클라우드 저장 로직 구현 필요
        Debug.LogWarning("InternalSaveDataAsync가 아직 구현되지 않았습니다.");
        await Task.CompletedTask; // 컴파일러 경고 제거 및 비동기 시그니처 유지 용도
    }

}
