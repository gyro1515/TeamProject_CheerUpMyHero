using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

//!!중요!!
//새로운 풀링 오브젝트 추가시 주의 사항
//1. 풀링해야할 것을 enum으로 지정 + "Prefabs/ObjPooling/" 위치에 enum과 같은 이름으로 프리팹 넣기
//2. 프리팹에 BasePoolable 또는 그걸 상속받는 클래스 붙이기
//3. ObjectPoolManager.Instance.Get(PoolType)으로 오브젝트 불러오기
//4. 오브젝트 반납 로직 작성 필수: BasePoolable을 GetComponet해서 ReleaseSelf 호출

public enum PoolType
{
    TestBulletV2,
    EnemyUnit1,
    PlayerUnit1,
    PlayerUnit2,
    PlayerUnit3,
    PlayerUnit4,
    PlayerUnit5,
    PlayerUnit6,
    PlayerUnit7,
    PlayerUnit8,
    PlayerUnit9,
    PlayerUnit10,
    PlayerUnit11,
    EnemyUnit2,
    EnemyUnit3,
    EnemyUnit4,
    EnemyUnit5,
    EnemyUnit6,
}

public class ObjectPoolManager : SingletonMono<ObjectPoolManager>, ISceneResettable
{
    private Array enums;

    private bool _isCleaning;

    private const string POOLPATH = "Prefabs/ObjPooling/";

    private List<Transform> poolTransforms = new();

    //풀을 담을 딕셔너리(IObjectPool 내장 인터페이스 사용 => 나중에 ObjectPool말고도 별도 클래스를 만들어 활용 가능)
    private Dictionary<PoolType, IObjectPool<GameObject>> pools = new Dictionary<PoolType, IObjectPool<GameObject>>();

    //현재 활성화된 풀 오브젝트 리스트(반납 처리용)
    private List<BasePoolable> allActivePoolables = new();
    //풀 오브젝트 캐싱용
    private Dictionary<GameObject, BasePoolable> poolableCache = new Dictionary<GameObject, BasePoolable>();

    //씬 바뀔 때 모든 풀 반납 처리
    /*private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene current)
    {
        CleanAll();
    }*/


    protected override void Awake()
    {
        base.Awake();

        enums = Enum.GetValues(typeof(PoolType));

        InitPool();
    }
    private void Start()
    {
        // *** 씬 전환마다 리소스 정리하려면 추가 필요***
        SceneLoader.Instance.SceneResettables.Add(this);
    }
    void InitPool()
    {
        // 이 부분에서 rootContainer의 부모를 이 매니저 게임 오브젝트로 하지 않으면 씬 전환시마다 @Pool_Root가 삭제될텐데
        // 그럼 오브젝트 풀링으로 사용된 애들도 같이 날아갈 거고요.
        // 이때 다시 이 풀링 매니저를 사용하면 제대로 될까요...?
        // 제 느낌은 Get()할때 null 뜰거 같긴 합니다. 풀은 있지만 참조가 안되는...
        //---> 수정 중  

        Transform rootContainer = new GameObject("@Pool_Root").transform;
        rootContainer.SetParent(gameObject.transform);
        foreach (PoolType type in enums)
        {
            //중복 체크
            if (pools.ContainsKey(type))
            {
                Debug.LogWarning($"[ObjectPoolManager] 풀 로드 오류: '{type}' 타입의 풀이 이미 존재합니다.");
                continue;
            }

            //1. 프리팹 로드
            GameObject prefab = Resources.Load<GameObject>(POOLPATH + type.ToString());
            if (prefab == null)
            {
                Debug.LogError($"[ObjectPoolManager] 프리팹 로드 실패: Resources/{POOLPATH + type.ToString()}");
            }

            //2. 오브젝트를 담을 빈 게임 오브젝트 생성

            Transform typeContainer = new GameObject(type.ToString() + "_Pool").transform;
            typeContainer.SetParent(rootContainer);
            poolTransforms.Add(typeContainer);

            //3. 풀 생성
            IObjectPool<GameObject> objectPool = new ObjectPool<GameObject>
            (
                createFunc: () => CreateObject(prefab, type, typeContainer),      // 풀에 오브젝트가 없을 때 새로 생성하는 방법 => 람다 
                actionOnGet: OnGetObject,       // 풀에서 오브젝트를 꺼낼 때(Get) 할 행동 => 주로 SetActive(true), 위치 초기화 등
                actionOnRelease: OnReleaseObject, // 풀에 오브젝트를 반납할 때(Release) 할 행동 => 주로 SetActive(false)
                actionOnDestroy: OnDestroyObject, // 풀이 가득 차서 오브젝트를 파괴할 때 할 행동
                collectionCheck: true,          // 동일한 오브젝트가 풀에 중복으로 들어가는 것을 방지 (안전장치)
                defaultCapacity: 10,            // 풀의 기본 크기
                maxSize: 1000                     // 풀의 최대 크기
            );

            pools.Add( type, objectPool );
        }
    }

    private GameObject CreateObject(GameObject obj, PoolType type, Transform container)
    {
        if (_isCleaning) return null;

        GameObject gameObject = Instantiate(obj);
        
        gameObject.transform.SetParent(container);

        // 생성된 오브젝트가 자신의 풀을 알도록 설정
        BasePoolable poolable = gameObject.GetComponent<BasePoolable>();
        if( poolable != null)
        {
            poolable.SetPool(pools[type]);
            //poolableCache[obj] = poolable;
            //Debug.Log($"캐싱!: {obj.name}");
        }
            
        else
            Debug.LogError($"[ObjectPoolManager] 풀 로드 오류: '{gameObject.name}' 프리팹에 BasePoolable 컴포넌트가 없습니다.");
        return gameObject;
    }

    private void OnGetObject(GameObject obj)
    {
        if (_isCleaning) return;

        obj.SetActive(true);


        //if (poolableCache.TryGetValue(obj, out BasePoolable poolable))
        //{
        //    if (!allActivePoolables.Contains(poolable))
        //    {
        //        allActivePoolables.Add(poolable);
        //    }
        //}
        //else
        //{
        //    Debug.Log("Poolable가 없다고??");
        //}
    }

    private void OnReleaseObject(GameObject obj) 
    {
        //if (_isCleaning) return;

        //if (poolableCache.TryGetValue(obj, out BasePoolable poolable))
        //{
        //    allActivePoolables.Remove(poolable);
        //}
        obj.SetActive(false);
    }

    private void OnDestroyObject(GameObject obj)
    {
        if (_isCleaning) return;
        Debug.LogWarning($"[ObjectPoolManager] 풀 경고: '{obj}' 타입 수가 최대에 도달해서 파괴되었습니다");
        Destroy(obj);
    }

    public GameObject Get(PoolType pooltype)
    {
        if (_isCleaning) return null;

        if (!pools.ContainsKey(pooltype))
        {
            Debug.LogError($"[ObjectPoolManager] 풀 로드 실패: '{pooltype.ToString()}' 이름의 풀이 존재하지 않습니다.");
            return null;
        }

        return pools[pooltype].Get();
    }



    void CleanAll()
    {
        if (_isCleaning) return;
        _isCleaning = true;

        try
        {
            foreach (IObjectPool<GameObject> obj in pools.Values)
            {
                if (obj == null) continue;
                // Close 프로세스 추가 가능
                obj.Clear();
            }

            foreach (Transform transform in poolTransforms)
            {
                if (transform == null) continue;
                // Close 프로세스 추가 가능

                for (int i = 0; i < transform.childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
        }
        finally
        {
            _isCleaning = false;
        }
    }
    // 씬 전환 시 오브젝트 클리어용
    public void OnSceneReset()
    {
        CleanAll();
    }
}
