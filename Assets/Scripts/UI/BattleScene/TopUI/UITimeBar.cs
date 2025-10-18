using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UITimeBar : MonoBehaviour
{
    [SerializeField] Image[] smallTimeBar = new Image[20];
    
    private WaitForSeconds wait30s;
    private int timeIndex = 0;
    IEventSubscriber<TimeSyncEvent> timeSyncEventSub;
    private void Awake()
    {
        timeSyncEventSub = EventManager.GetSubscriber<TimeSyncEvent>();
        timeSyncEventSub.Subscribe(StartTimer);
    }
    private void OnDisable()
    {
        timeSyncEventSub.Unsubscribe(StartTimer);
    }
    void StartTimer(TimeSyncEvent timerSyncEvent)
    {
        float waitTime = timerSyncEvent.waveTime / 4;
        //Debug.Log($"타이머 시작:{timerSyncEvent.waveTime} => {waitTime}");
        wait30s = new WaitForSeconds(waitTime);
        StartCoroutine(thirtySeconds());
    }

    IEnumerator thirtySeconds()
    {
        while (timeIndex < smallTimeBar.Length)
        {
            yield return wait30s;
            smallTimeBar[timeIndex].color = Color.black;
            timeIndex++;
        }
    }
}
#region 시간 동기화 이벤트
public struct TimeSyncEvent
{
    public float waveTime;
    public int maxWaveCount;
}
#endregion
