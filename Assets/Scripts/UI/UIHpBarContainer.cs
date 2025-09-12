using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// 현재 HQ에만 적용 중입니다.
public class UIHpBarContainer : BaseUI
{
    [Header("체력바 컨테이터 세팅")]
    [SerializeField] GameObject uiHpBarPrefab;

    public UIHpbar AddHpBar(BaseCharacter character, EUIHpBarType type, Vector2? hpBarSize = null)
    {
        // 여기서 오브젝트 풀에서 가져오기
        UIHpbar hpBar = Instantiate(uiHpBarPrefab,gameObject.transform).GetComponent<UIHpbar>();
        hpBar.HpBarInit(character, type, hpBarSize);
        return hpBar;
    }
}
