using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BasePoolable : MonoBehaviour
{
    protected IObjectPool<GameObject> MyPool { get; private set; }

    public virtual void SetPool(IObjectPool<GameObject> pool)
    {
        MyPool = pool;
    }

    //오브젝트풀로 돌아가는 조건 각자 만들기
    public virtual void ReleaseSelf()
    {
        if (MyPool != null)
            MyPool.Release(this.gameObject);
        else
        {
            Debug.LogWarning($"[BasePoolable] 풀 경고: '{this.gameObject.name}'의 풀이 없습니다");
            Destroy(this.gameObject);
        }    
    }
}
