using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneLoadManager : SingletonMono<SceneLoadManager>
{
    private Dictionary<SceneState, SceneBase> _scenes = new();
    private SceneBase _prevScene;
    private SceneBase _currentScene;

    Coroutine _loadingCoroutine;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoadedComplete;
    }

    protected override void Awake()
    {
        base.Awake();
        // 씬 클래스와 enum 매핑 (씬 만들어지면 주석해제)
        _scenes.Add(SceneState.MainScene, new MainScene());
        _scenes.Add(SceneState.BattleScene, new BattleScene()); // 일단 테스트로 원진 테스트씬으로
        _scenes.Add(SceneState.WonJinTestScene, new WonJinTestScene());
    } 

    public void LoadScene(SceneState sceneState)
    {
        // 진행중인 씬 로딩이 있으면 정지
        if (_loadingCoroutine != null) StopCoroutine(_loadingCoroutine);

        // 로드할 씬이 등록되었는지 확인
        if (!_scenes.TryGetValue(sceneState, out var scene))
        {
            Debug.LogError($"SceneState가 없습니다. : {sceneState}");
            return;
        }

        // 로드할 씬이 현재 씬과 같은지 확인
        if (_currentScene == scene) return;

        _loadingCoroutine = StartCoroutine(LoadSceneProcess(sceneState));
    }

    IEnumerator LoadSceneProcess(SceneState sceneState)
    {
        // 씬 타입에 따른 씬 오브젝트
        // 기존 씬이 있다면 종료 콜백 실행
        var scene = _scenes[sceneState];
        _currentScene?.OnSceneExit();

        // 이전 씬과 현재 씬의 기록을 전환
        _prevScene = _currentScene;
        _currentScene = scene;

        //씬 로딩 시작 전에 페이드 아웃 효과 실행
        yield return FadeManager.Instance.FadeOut();

        // 씬 로딩 시작
        var operation = SceneManager.LoadSceneAsync(sceneState.ToString());
        operation.allowSceneActivation = false;

        // 씬 로드하면서 불러올 데이터가 있으면 로딩
        // API 및 어드레서블등의 비동기 사용을 안하고 있어서 예제에서는 불필요
        _currentScene.SceneLoading();

        // operation.allowSceneActivation 가 false 일떄는 0.9 까지만 진행됨
        while (operation.progress < 0.9f)
            yield return null;

        // 씬 전환 허가
        operation.allowSceneActivation = true;

        // 씬 전환 완료 기다리기
        while (!operation.isDone)
            yield return null;

        // 한프레임 전환 대기
        // (씬의 있는 오브젝트 - OnEnable / Awake / Start 등 초기화 대기) 
        yield return null;


        // 로딩된 씬의 진입 콜백 실행
        _currentScene.OnSceneEnter();
        _loadingCoroutine = null;
    }

    private void OnSceneLoadedComplete(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeManager.Instance.FadeIn());
        if (_currentScene != null)
        {
            _currentScene.OnSceneEnter();
        }
    }

    protected override void OnDestroy() //나중에 수정해야할 수도
    {
        base.OnDestroy();
        SceneManager.sceneLoaded -= OnSceneLoadedComplete;
    } 
}
