using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : SingletonMono<EventManager>, ISceneResettable
{
    // 싱글톤 인스턴스 접근 안되게 하기
    private new static EventManager Instance => SingletonMono<EventManager>.Instance;

    // 이벤트 저장소
    private readonly Dictionary<Type, Delegate> eventTable = new Dictionary<Type, Delegate>();
    
    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        // *** 씬 전환마다 리소스 정리하려면 추가 필요***
        SceneLoader.Instance.SceneResettables.Add(this);
    }
    
    public static void Subscribe<T>(Action<T> callback) where T : struct
    {
        if (Instance.eventTable.TryGetValue(typeof(T), out var del))
        {
            Instance.eventTable[typeof(T)] = (Action<T>)del + callback;
        }
        else
        {
            Instance.eventTable[typeof(T)] = callback;
        }
        //Debug.Log("[EventManager] 구독");

    }

    public static void Unsubscribe<T>(Action<T> callback) where T : struct
    {
        if (Instance.eventTable.TryGetValue(typeof(T), out var del))
        {
            var currentDel = (Action<T>)del - callback;
            if (currentDel == null)
                Instance.eventTable.Remove(typeof(T));
            else
                Instance.eventTable[typeof(T)] = currentDel;
            //Debug.Log("[EventManager] 구독해제");
        }   
    }
    // 이벤트 발행용, 한 번만 실행할 때 사용
    public static void Publish<T>(T eventData) where T : struct
    {
        if (Instance.eventTable.TryGetValue(typeof(T), out var del))
        {
            (del as Action<T>)?.Invoke(eventData);
        }
    }
    // 이벤트 캐싱용, 여러 번 실행할 때 사용
    public static Action<T> GetEventDelegateForPublish<T>() where T : struct
    {
        if (!Instance.eventTable.TryGetValue(typeof(T), out var del))
        {
            Instance.eventTable[typeof(T)] = null;
        }
        return Instance.eventTable[typeof(T)] as Action<T>;
    }

    public void OnSceneReset()
    {
        //Debug.Log("[EventManager] 씬 전환: 이벤트 테이블 초기화");
        eventTable.Clear();
    }
}

