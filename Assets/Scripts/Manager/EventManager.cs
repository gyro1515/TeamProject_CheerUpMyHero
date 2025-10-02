using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : SingletonMono<EventManager>
{
    // 이벤트 저장소
    private readonly Dictionary<Type, Delegate> eventTable = new Dictionary<Type, Delegate>();
    protected override void Awake()
    {
        base.Awake();
        // 씬 언로드될 때마다 이벤트 초기화
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        eventTable.Clear();
    }
    public void Subscribe<T>(Action<T> callback) where T : struct
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
        {
            eventTable[typeof(T)] = (Action<T>)del + callback;
        }
        else
        {
            eventTable[typeof(T)] = callback;
        }
    }

    public void Unsubscribe<T>(Action<T> callback) where T : struct
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
        {
            var currentDel = (Action<T>)del - callback;
            if (currentDel == null)
                eventTable.Remove(typeof(T));
            else
                eventTable[typeof(T)] = currentDel;
        }
    }

    public void Publish<T>(T eventData) where T : struct
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
        {
            (del as Action<T>)?.Invoke(eventData);
        }
    }
    private void OnSceneUnloaded(Scene scene)
    {
        eventTable.Clear();
    }
}
