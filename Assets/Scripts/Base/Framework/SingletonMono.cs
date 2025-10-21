using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    // 기본값 true → 파괴 불능
    protected virtual bool IsPersistent => true;
    static bool isDestroyed = false;
    public static T Instance
    {
        get
        {
            // 씬에 없더라도 생성되도록
            if (instance == null && !isDestroyed)
            {
                var singletonGO = new GameObject($"{typeof(T)}");
                instance = singletonGO.AddComponent<T>(); // Awake() 실행
            }
            
            return instance;
        }
    }

    protected virtual void Awake() // 씬에 올려 놓았다면 바로 실행 될 것
    {
        if (instance == null)
        {
            //if(!instance.IsUnityNull())
            if(!System.Object.ReferenceEquals(instance, null))
            {
                //if (instance.gameObject) Destroy(instance.gameObject);
                Debug.LogWarning($"페이크 널 {typeof(T).Name}이(가) 존재합니다."); 
                return; // 페이크 널 방지
            }
            //Debug.Log($"싱글톤 {typeof(T).Name}이(가) 생성되었습니다.");
            instance = this as T;
            if (IsPersistent) // 파괴 불능이면
                DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
        }
        else if (instance != this)
        {
            // 현재는 씬 하나라 중복 생성될 일은 없겠지만, 혹시 모르니까
            Debug.LogWarning($"중복된 {typeof(T).Name} 싱글톤이 발견되어 파괴됩니다.");
            Destroy(gameObject); // 중복 제거
        }
        else
        {
            Debug.LogWarning($"이미 싱글톤 {typeof(T).Name}이(가) 존재합니다."); // 이게 가능한가?
        }
    }

    public virtual void Release() // 추천 받은 기능, 씬 전환 시 해당 매니저를 파괴할 때도 있을테니까
    {
        if (instance == null) return;
        if (instance.gameObject) Destroy(instance.gameObject);

        instance = null;
    }
    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            //if(IsPersistent) isDestroyed = true;
            instance = null;
        }
        /*if (instance && instance.gameObject)
        {
            Destroy(instance);
            Destroy(instance.gameObject);
        }*/
        if (IsPersistent) isDestroyed = true; // 파시 생성 안되도록
        instance = null;

    }
}
