using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static InputManager;
using static Unity.Burst.Intrinsics.X86.Avx;

public enum FromUI
{
    MainScreen,
    UIMenu
}
public class UIManager : SingletonMono<UIManager>, ISceneResettable
{
    public const string UIPrefabPath = "Prefabs/UI/";

    private bool _isCleaning;
    private Dictionary<string, BaseUI> _uiDictionary = new Dictionary<string, BaseUI>();
    //private BaseUI _currentOpenedPopup = null;
    private readonly Stack<IBackButtonHandler> _uiStack = new Stack<IBackButtonHandler>();

    // [로딩] 로딩 상태 관리 플래그 및 UI 참조 변수
    private const string LoadingUIName = "UI_Loading";
    private bool _isLoading = false;
    public FromUI fromUI { get; set;}
    private GameObject _loadingUIInstance;
    private CanvasGroup _loadingCanvasGroup;

    protected override void Awake()
    {
        base.Awake();
        InputManager.Instance.gameObject.SetActive(true); // InputManager 강제 초기화
        //SceneManager.sceneLoaded += OnSceneLoaded;
        EventManager.GetSubscriber<BackButtonPressedEvent>().Subscribe(_ => BackButtonPressed());
        EventManager.GetSubscriber<AddUIStackEvent>().Subscribe(PushUI);
        EventManager.GetSubscriber<RemoveUIStackEvent>().Subscribe(_ => PopUI());

        // [로딩] 로딩 UI를 미리 생성하고 비활성화해 둡니다.
        InitializeLoadingUI();
    }
    /*private void OnEnable()
    {
        // 씬 언로드 이벤트 구독
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }*/
    //private void Update()
    //{
    //    // 뒤로가기 버튼(Escape 키)이 눌렸는지 확인
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        // 현재 열려있는 팝업이 있다면 닫음
    //        if (_currentOpenedPopup != null)
    //        {
    //            _currentOpenedPopup.CloseUI();
    //            _currentOpenedPopup = null;
    //        }
    //    }
    //}
    //public void SetCurrentPopup(BaseUI ui)
    //{
    //    _currentOpenedPopup = ui;
    //}
    //public void ClearCurrentPopup(BaseUI ui)
    //{
    //    // 닫으려는 UI가 현재 열려있는 UI가 맞는지 확인 후 비움
    //    if (_currentOpenedPopup == ui)
    //    {
    //        _currentOpenedPopup = null;
    //    }
    //}
    private void Start()
    {
        // *** 씬 전환마다 리소스 정리하려면 추가 필요***
        SceneLoader.Instance.SceneResettables.Add(this);
    }
    private void OnDisable()
    {
        /*// 씬 언로드 이벤트 해제 (메모리 누수 방지)
        SceneManager.sceneUnloaded -= OnSceneUnloaded;*/
    }

    // UI 순서 관리
    public static void PubishAddUIStackEvent(IBackButtonHandler ui)
    {
        Instance.PushUI(new AddUIStackEvent { ui = ui });
        //Instance.onAddUIStackEventPub.Publish(new AddUIStackEvent { ui = ui });
    }
    public static void PublishRemoveUIStackEvent()
    {
        Instance.PopUI();
        //Instance.onRemoveUIStackEventPub.Publish();
    }
    void PushUI(AddUIStackEvent eventStruct)
    {
        _uiStack.Push(eventStruct.ui);
        //Debug.Log($"UIManager: UI 스택에서 추가: {eventStruct.ui.ToString()} / {_uiStack.Count}");
    }

    // UI가 닫힐 때 스택에서 제거
    void PopUI()
    {
        if (_uiStack.Count > 0)
        {
            var tmp = _uiStack.Pop();
            //Debug.Log($"UIManager: UI 스택에서 제거: {tmp.ToString()} / {_uiStack.Count}");
        }
    }
    void BackButtonPressed()
    {
        // [로딩] 로딩 중일때 뒤로가기 버튼 막기
        if (_isLoading)
        {
            Debug.Log("로딩 중이므로 뒤로가기를 무시합니다.");
            return;
        }


        //Debug.Log($"UIManager: 뒤로 가기 버튼 눌림{_uiStack.Count}");
        // 스택에 UI가 하나라도 있다면
        if (_uiStack.Count > 0)
        {
            // 스택의 가장 위에 있는 UI에게 뒤로 가기 처리를 위임
            IBackButtonHandler topUI = _uiStack.Peek();
            topUI?.OnBackPressed();
        }
        else
        {
            Debug.Log("뒤로 가기: 열린 UI 없음 (게임 종료 로직 등 수행)");
#if UNITY_EDITOR
            // 에디터에서는 플레이 모드를 종료
            //EditorApplication.isPlaying = false;
#else
        // 실제 빌드된 환경에서는 애플리케이션 종료
        Application.Quit();
#endif
        }
    }
    // ================================
    // UI 관리
    // ================================
    public void OpenUI<T>() where T : BaseUI
    {
        var ui = GetUI<T>();
        ui?.OpenUI();
    }

    public void CloseUI<T>() where T : BaseUI
    {
        if (IsExistUI<T>())
        {
            var ui = GetUI<T>();
            ui?.CloseUI();
        }
    }

    public T GetUI<T>() where T : BaseUI
    {
        if (_isCleaning) return null;

        string uiName = GetUIName<T>();

        BaseUI ui;
        if (IsExistUI<T>())
            ui = _uiDictionary[uiName];
        else
            ui = CreateUI<T>();

        return ui as T;
    }

    private T CreateUI<T>() where T : BaseUI
    {
        if (_isCleaning) return null;

        string uiName = GetUIName<T>();
        if (_uiDictionary.TryGetValue(uiName, out var prevUi) && prevUi != null)
        {
            Destroy(prevUi.gameObject);
            _uiDictionary.Remove(uiName);
        }

        // 1. 프리팹 로드
        string path = GetPath<T>();
        //Debug.Log(path);
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError($"[UIManager] Prefab not found: {path}");
            return null;
        }

        // 2. 인스턴스 생성
        GameObject go = Instantiate(prefab, gameObject.transform);

        // 3. 컴포넌트 획득
        T ui = go.GetComponent<T>();
        if (ui == null)
        {
            Debug.LogError($"[UIManager] Prefab has no component : {uiName}");
            Destroy(go);
            return null;
        }

        // 4. Dictionary 등록
        _uiDictionary[uiName] = ui;

        return ui;
    }

    public bool IsExistUI<T>() where T : BaseUI
    {
        string uiName = GetUIName<T>();
        return _uiDictionary.TryGetValue(uiName, out var ui) && ui != null;
    }


    // ================================
    // path 헬퍼
    // ================================
    private string GetPath<T>() where T : BaseUI
    {
        return UIPrefabPath + GetUIName<T>();
    }

    private string GetUIName<T>() where T : BaseUI
    {
        return typeof(T).Name;
    }

    // 씬 로드 시 다시 구독
    /*private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("OnSceneLoaded 호출");
        EventManager.Subscribe<BackButtonPressedEvent>(_ => BackButtonPressed());
        EventManager.Subscribe<AddUIStackEvent>(PushUI);
        EventManager.Subscribe<RemoveUIStackEvent>(_ => PopUI());
    }*/
    // ================================
    // 리소스 정리
    // ================================
    private void OnSceneUnloaded(Scene scene)
    {
        //Debug.Log("OnSceneUnloaded 호출");
        CleanAllUIs();
        // 이건 씬 로더에서 하겠습니다.
        //StartCoroutine(CoUnloadUnusedAssets());
    }

    private void CleanAllUIs()
    {
        if (_isCleaning) return;
        _isCleaning = true;

        try
        {
            foreach (var ui in _uiDictionary.Values)
            {
                if (ui == null) continue;
                // Close 프로세스 추가 가능
                Destroy(ui.gameObject);
            }
            _uiDictionary.Clear();
        }
        finally
        {
            _isCleaning = false;
        }
    }
    // 씬 전환 시 오브젝트 클리어용
    public void OnSceneReset()
    {
        CleanAllUIs();
        _uiStack.Clear();

        //[로딩] 로딩 중 씬이 전환될 경우를 대비해 로딩 상태도 초기화
        if (_isLoading)
        {
            HideLoading();
        }
    }


    /*// UI 뿐만 아니라 전체 오브젝트 관리 시스템측면에서도 있으면 좋음
    private IEnumerator CoUnloadUnusedAssets()
    {
        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }*/



    // ================================
    // [로딩 스크린](터치, 뒤로가기 잠금)
    // ================================

    private void InitializeLoadingUI()
    {
        if (_loadingUIInstance != null) return;

        GameObject prefab = Resources.Load<GameObject>(UIPrefabPath + LoadingUIName);
        if (prefab == null)
        {
            Debug.LogError($"[UIManager] Loading UI Prefab not found: {UIPrefabPath + LoadingUIName}");
            return;
        }

        _loadingUIInstance = Instantiate(prefab, transform);

        //캔버스 그룹
        _loadingCanvasGroup = _loadingUIInstance.GetComponent<CanvasGroup>();

        _loadingUIInstance.SetActive(false); // 처음에는 비활성화
    }

    //로딩 호출
    public void ShowLoading()
    {
        if (_isLoading) return;

        _isLoading = true;
        if (_loadingUIInstance != null)
        {
            _loadingUIInstance.SetActive(true);
            _loadingCanvasGroup.blocksRaycasts = true; // 터치 막기
        }
    }
    //로딩 종료
    public void HideLoading()
    {
        _isLoading = false;
        if (_loadingUIInstance != null)
        {
            _loadingUIInstance.SetActive(false);
            _loadingCanvasGroup.blocksRaycasts = false; // 터치 허용
        }
    }

}
