using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.SceneManagement;

// 구독자에게 노출될 인터페이스
public interface IEventSubscriber<T> where T : struct
{
    void Subscribe(Action<T> callback);
    void Unsubscribe(Action<T> callback);
}
// 발행자에게 노출될 인터페이스
public interface IEventPublisher<T> where T : struct
{
    void Publish(T eventData);
}
public interface IEventClearable
{
    void Clear();
}
//public class EventManager : SingletonMono<EventManager>, ISceneResettable // 2안
public class EventManager : SingletonMono<EventManager>
{
    // EventChannel은 두 인터페이스를 모두 구현
    private class EventChannel<T> : IEventClearable, IEventSubscriber<T>, IEventPublisher<T> where T : struct
    {
        // 구독자들이 등록될 델리게이트
        private Action<T> _onPublish;
        public void Subscribe(Action<T> callback)
        {
            // 중복 구독 방지
            _onPublish -= callback;
            _onPublish += callback;
        }
        public void Unsubscribe(Action<T> callback)
        {
            if (_onPublish == null) return;
            _onPublish -= callback;
        }
        public void Publish(T eventData)
        {
            _onPublish?.Invoke(eventData);
        }
        public void Clear()
        {
            _onPublish = null;
        }
    }
    // 싱글톤 인스턴스 접근 안되게 하기
    private new static EventManager Instance => SingletonMono<EventManager>.Instance;
    // 이벤트 저장소
    private readonly Dictionary<Type, object> _channels = new Dictionary<Type, object>();

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += Loaded;
        SceneManager.sceneUnloaded += Unloaded;
    }
    #region 테스트
    [SerializeField] int eventCnt = 0;
    private void Update()
    {
        eventCnt = _channels.Count;
    }
    private void Loaded(Scene scene, LoadSceneMode mode)
    {
        // 테스트
        Debug.Log($"{_channels.Count}");
    }
    private void Unloaded(Scene scene)
    {
        // 테스트
        Debug.Log($"{_channels.Count}");
    }
    #endregion
    /* private void Start()
     {
         // *** 씬 전환마다 리소스 정리하려면 추가 필요***
         //SceneLoader.Instance.SceneResettables.Add(this);
     }*/
    protected override void OnDestroy()
    {
        base.OnDestroy();
        foreach (var channelPair in _channels)
        {
            if (channelPair.Value is IEventClearable channel)
            {
                channel.Clear(); // 구독자 목록(델리게이트) = null
            }
        }
        _channels.Clear();
    }
    // 해당 타입의 이벤트 채널을 가져오거나, 없으면 새로 생성
    private static EventChannel<T> GetChannel<T>() where T : struct
    {
        Type type = typeof(T);
        if (!Instance._channels.TryGetValue(type, out var channel))
        {
            channel = new EventChannel<T>();
            Instance._channels[type] = channel;
        }
        return (EventChannel<T>)channel;
    }
    public static void Subscribe<T>(Action<T> callback) where T : struct
    {
        GetChannel<T>().Subscribe(callback);
    }
    public static void Unsubscribe<T>(Action<T> callback) where T : struct
    {
        if (Instance._channels.TryGetValue(typeof(T), out var channel))
        {
            (channel as EventChannel<T>)?.Unsubscribe(callback);
        }
    }
    // 이벤트 발행, 한 번만 실행할 때 사용
    public static void Publish<T>(T eventData) where T : struct
    {
        if (Instance._channels.TryGetValue(typeof(T), out var channel))
        {
            (channel as EventChannel<T>)?.Publish(eventData);
        }
    }
    // 이벤트 발행, 계속 실행할 때 이벤트 캐싱해서 사용 용도
    // 발행(Publish)만 가능하도록, 반독 발행용
    public static IEventPublisher<T> GetPublisher<T>() where T : struct
    {
        return GetChannel<T>();
    }
    // 구독/구독 해제 기능만 외부에 노출하고 싶을 때 사용할 수 있는 메서드, 반복 구독/구독 해제용
    public static IEventSubscriber<T> GetSubscriber<T>() where T : struct
    {
        return GetChannel<T>();
    }
}

