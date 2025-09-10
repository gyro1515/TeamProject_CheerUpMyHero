using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public enum EUIHpBarType
{
    None, Player, PlayerUnit, EnemyUnit
}
public class UIHpbar : MonoBehaviour
{
    [Header("체력바 세팅")]
    [SerializeField] Image hpBarImg;
    BaseCharacter _character;
    public void HpBarInit(BaseCharacter character, EUIHpBarType type)
    {
        _character = character;
        _character.OnCurHpChane += SetHpBar;
        _character.OnDead += DestoryUIHpBar;
        Color color = Color.white;
        switch (type)
        {
            case EUIHpBarType.Player:
                color = Color.green;
                break;
            case EUIHpBarType.PlayerUnit:
                color = Color.blue;
                break;
            case EUIHpBarType.EnemyUnit:
                color = Color.red;
                break;
        }
        hpBarImg.color = color;
        SetHpBarPos();
    }
    private void Update()
    {
        if (_character == null) return;
        SetHpBarPos();
    }
    private void FixedUpdate()
    {
        
    }
    void SetHpBar(float curHp, float maxHp)
    {
        hpBarImg.fillAmount = curHp / (float)maxHp;
    }
    void DestoryUIHpBar()
    {
        _character.OnCurHpChane -= SetHpBar;
        _character.OnDead -= DestoryUIHpBar;
        _character = null;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    void SetHpBarPos()
    {
        gameObject.transform.position = Camera.main.WorldToScreenPoint(_character.gameObject.transform.position + _character.HpBarPosByCharacter);
    }

}
