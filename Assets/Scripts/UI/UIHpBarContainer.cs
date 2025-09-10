using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class UIHpBarContainer : BaseUI
{
    [Header("체력바 컨테이터 세팅")]
    [SerializeField] GameObject uiHpBarPrefab;

    //ObjectPool<UIHpbar> uiHpBarPool;

    public void Awake()
    {
        //uiHpBarPool = new ObjectPool<UIHpbar>(CreateUIHpBar,);

    }
    UIHpbar CreateUIHpBar()
    {
        UIHpbar hpBar = Instantiate(uiHpBarPrefab).GetComponent<UIHpbar>();
        return hpBar;
    }
    public UIHpbar AddHpBar(BaseCharacter character, EUIHpBarType type)
    {
        UIHpbar hpBar = Instantiate(uiHpBarPrefab,gameObject.transform).GetComponent<UIHpbar>();
        hpBar.HpBarInit(character, type);
        return hpBar;
    }
}
