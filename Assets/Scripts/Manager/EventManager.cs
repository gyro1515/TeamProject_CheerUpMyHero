using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
// 먼저 캐싱하고 뒤늦게 구독/구독해제 해도 이벤트가 캐싱한 델리게이트에 추가/제거 되도록 하기 위해 이벤트 채널 클래스 사용
public class EventChannel<T> where T : struct
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
        _onPublish -= callback;
    }
    public void Publish(T eventData)
    {
        _onPublish?.Invoke(eventData);
    }
}
//public class EventManager : SingletonMono<EventManager>, ISceneResettable // 2안
public class EventManager : SingletonMono<EventManager>
{
    // 싱글톤 인스턴스 접근 안되게 하기
    private new static EventManager Instance => SingletonMono<EventManager>.Instance;
    // 이벤트 저장소
    private readonly Dictionary<Type, object> _channels = new Dictionary<Type, object>();

    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        // *** 씬 전환마다 리소스 정리하려면 추가 필요***
        //SceneLoader.Instance.SceneResettables.Add(this);
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
    // 이벤트 발행용, 한 번만 실행할 때 사용
    public static void Publish<T>(T eventData) where T : struct
    {
        if (Instance._channels.TryGetValue(typeof(T), out var channel))
        {
            (channel as EventChannel<T>)?.Publish(eventData);
        }
    }
    // 이벤트 캐싱용, 여러 번 실행할 때 사용(Update나 자주 호출해야 하는 곳에)
    public static EventChannel<T> GetPublisher<T>() where T : struct
    {
        return GetChannel<T>();
    }
    // **************************
    // 1.씬 전환 시 이벤트 테이블 초기화하면 좋은점: 씬 전환 오브젝트 파괴 시 이벤트 구독 해제할 필요 없음
    // 단점: 씬 전환 후에도 이벤트 유지해야 하는 경우는 별도 처리 필요
    // **************************
    // 2.반면 씬 전환 시 초기화 안 하면 좋은점: 씬 전환 후에도 이벤트 유지 가능
    // 단점: 씬 전환 오브젝트 파괴 시 이벤트 구독 해제 안 하면 메모리 누수 발생 가능성 있음
    // **************************
    // 현재는 1번 방식 채택, UIManager에서 구독하는건 OnSceneLoaded에서 다시 구독처리
    /*public void OnSceneReset()
    {
        //Debug.Log("[EventManager] 씬 전환: 이벤트 테이블 초기화");
        _channels.Clear();
    }*/
    // ****************** 2번으로 변경 251013_21:16
}

