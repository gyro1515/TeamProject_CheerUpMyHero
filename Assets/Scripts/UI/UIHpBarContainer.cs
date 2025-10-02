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
    private void Awake()
    {
        EventManager.Instance.Subscribe<SpawnHQEvent>((spawnHQEvent)=> AddHpBar(spawnHQEvent.baseHQ, spawnHQEvent.type, spawnHQEvent.hpBarSize));
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<SpawnHQEvent>(AddHpBar);
    }
    private void OnDisable()
    {
        EventManager.Instance.Unsubscribe<SpawnHQEvent>(AddHpBar);

    }
    public UIHpbar AddHpBar(BaseCharacter character, EUIHpBarType type, Vector2? hpBarSize = null)
    {
        // 여기서 오브젝트 풀에서 가져오기
        UIHpbar hpBar = Instantiate(uiHpBarPrefab,gameObject.transform).GetComponent<UIHpbar>();
        hpBar.HpBarInit(character, type, hpBarSize);
        return hpBar;
    }
    void AddHpBar(SpawnHQEvent e)
    {
        UIHpbar hpBar = Instantiate(uiHpBarPrefab, gameObject.transform).GetComponent<UIHpbar>();
        hpBar.HpBarInit(e.baseHQ, e.type, e.hpBarSize);
    }
}
