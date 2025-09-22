using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacterHpBar : MonoBehaviour
{
    [Header("체력바 세팅")]
    [SerializeField] Transform hpBarTransform;
    [SerializeField] GameObject hpBarGO;
    BaseCharacter _character;
    private void Awake()
    {
        _character = GetComponent<BaseCharacter>();
        _character.OnCurHpChane += SetHpBar;
        _character.OnDead += SetHpBarActiveFalse;
        SetHpBarActiveFalse();
    }
    void SetHpBar(float curHp, float maxHp)
    {
        if(!hpBarTransform || curHp == maxHp) return;
        if(!hpBarGO.activeSelf) hpBarGO.SetActive(true);
        hpBarTransform.localScale = new Vector3(curHp/maxHp, 1f, 1f);
    }
    void SetHpBarActiveFalse()
    {
        hpBarGO.SetActive(false);
    }
}
