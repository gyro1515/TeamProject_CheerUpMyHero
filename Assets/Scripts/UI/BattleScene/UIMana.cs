using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMana : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI manaText;
    [SerializeField] Image manaGage;
    private void Start()
    {
        GameManager.Instance.Player.OnCurManaChanged += SetManaText;
        SetManaText(GameManager.Instance.Player.CurMana, GameManager.Instance.Player.MaxMana);
    }
    void SetManaText(float curMana, float maxMana)
    {
        manaText.text = $"{(int)curMana} / {(int)maxMana}";
        manaGage.fillAmount = curMana / maxMana;
    }
}
