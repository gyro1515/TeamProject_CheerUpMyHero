using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.TextCore.Text;
using static BaseHQ;
using static PlayerHQ;

// 현재 HQ에만 적용 중입니다.
public class UIHpBarContainer : BaseUI
{
    [Header("체력바 컨테이터 세팅")]
    [SerializeField] GameObject uiHpBarPrefab;
    IEventSubscriber<SpawnHQEvent> spawnHQEventSubscriber;
    private void Awake()
    {
        // 구독하고 해제할 필요가 없는 이유:
        // 이 오브젝트는 배틀씬 시작과 동시에 생성되고, 배틀씬이 끝나면 파괴됨
        // 씬 중간에 오브젝트가 활성화/비활성화 되는 것이 아니므로, 구독과 해제를 OnEnable/OnDisable에서 할 필요가 없음
        spawnHQEventSubscriber = EventManager.GetSubscriber<SpawnHQEvent>();
        spawnHQEventSubscriber.Subscribe(AddHpBar);
    }
    /*public UIHpbar AddHpBar(BaseCharacter character, EUIHpBarType type, Vector2? hpBarSize = null)
    {
        // 여기서 오브젝트 풀에서 가져오기
        UIHpbar hpBar = Instantiate(uiHpBarPrefab,gameObject.transform).GetComponent<UIHpbar>();
        hpBar.HpBarInit(character, type, hpBarSize);
        return hpBar;
    }*/
    private void OnDisable()
    {
        spawnHQEventSubscriber.Unsubscribe(AddHpBar);
    }
    void AddHpBar(SpawnHQEvent e)
    {
        UIHpbar hpBar = Instantiate(uiHpBarPrefab, gameObject.transform).GetComponent<UIHpbar>();
        hpBar.HpBarInit(e.baseHQ, e.type, e.hpBarSize);
    }
}
