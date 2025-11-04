using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneState
{
    // 예시들
    None,
    MainScene,
    BattleScene,
    EmptyScene,
    WonJinTestScene,
    StartScene
}

// 씬 전환을 관리하는 스크립트, 게임 시작 시 자동으로 생성되며, 씬 전환을 담당
public class SceneLoader : SingletonMono<SceneLoader>
{
    // 여기에 씬들 한번에 다 추가
    private readonly Dictionary<SceneState, string> sceneNames = new()
    {
        // 예시들
        /*{ SceneState.TopDown,     "TopDownScene" },
        { SceneState.FlappyPlane, "FlappyPlaneScene" },
        { SceneState.TheStack,    "TheStackScene" }*/
        // 추가
        { SceneState.MainScene,   "MainScene"   },
        { SceneState.BattleScene, "BattleScene" },
        { SceneState.EmptyScene,  "EmptyScene"  },
        { SceneState.StartScene,  "StartScene"  },

    };
    // 매니저 오브젝트 정리 용
     public List<ISceneResettable> SceneResettables { get; private set; } = new();

    // 키 모아두기 예시
    public const string SelCharSKey = "SelectedCharacter";

    public static bool IsChange { get; private set; } = false; // 씬 전환 시 그 후 상호작용 작동 안하도록

    // 현재 로드된 씬의 SceneState를 저장할 프로퍼티
    public static SceneState CurrentSceneState { get 
        {
            if (Instance != null)
                return Instance.currentSceneState;

            return SceneState.None;
        } 
    }
    SceneState currentSceneState;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] // 하이어아키 창에 게임오브젝트를 만들지 않아도 자동 생성
    private static void Init()
    {
        if (Instance != null)
        {
            SceneManager.sceneLoaded += Instance.OnSceneLoaded;
        }
    }
    protected override void Awake()
    {
        base.Awake();
    }
    static void Load(SceneState state, LoadSceneMode mode = LoadSceneMode.Single)
    {
        IsChange = true; // 씬 전환 시작

        SceneManager.LoadScene(Instance.sceneNames[state], mode);
    }

    public static string GetSceneName(SceneState state)
    {
        return Instance.sceneNames.TryGetValue(state, out var name) ? name : null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == SceneState.EmptyScene.ToString())
            return; // 빈 씬일 땐 페이드인 실행 X
        StartCoroutine(SceneLoaded());
    }
    static IEnumerator NextSceneSequence(SceneState nextScene)
    {
        yield return FadeManager.Instance.FadeOut(); // 페이드 아웃 끝나고

        Debug.Log("빈 씬으로 전환");
        Load(SceneState.EmptyScene);

        // 리소스 정리하고 가비지 컬렉션 호출
        Instance.ResetObject();
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();

        Debug.Log("다음 씬으로");
        // 비동기 vs 동기 로딩: 일단 체감은 동기가 더 빠른 거 같기도...
        // 비동기
        /*AsyncOperation op = SceneManager.LoadSceneAsync(nextScene.ToString(), LoadSceneMode.Single);
        op.allowSceneActivation = false; // 로딩만 하고, 전환은 보류
        // 로딩 대기
        while (op.progress < 0.9f)
            yield return null;
        // 로딩 끝났으니 씬 전환
        op.allowSceneActivation = true;*/

        // 동기
        Load(nextScene);
    }
    public void StartLoadScene(SceneState nextScene)
    {
        // 씬 전환을 시작할 때 어떤 씬으로 가는지 CurrentSceneState에 기록
        currentSceneState = nextScene;

        StartCoroutine(NextSceneSequence(nextScene));
    }
    static IEnumerator SceneLoaded()
    {
        yield return FadeManager.Instance.FadeIn(); // 페이드 인
        //Debug.Log("씬 전환 완료");
        IsChange = false;
    }
    private void ResetObject()
    {
        foreach (var reset in SceneResettables)
        {
            reset.OnSceneReset();
        }
    }
}