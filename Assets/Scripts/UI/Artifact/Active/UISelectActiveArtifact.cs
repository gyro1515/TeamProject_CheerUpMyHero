using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class UISelectActiveArtifact : BaseUI
{
    [Header("액티브 유물 선택 UI 세팅")]
    [SerializeField] Button toSelCardBtn;
    [SerializeField] Button toSelPassiveBtn;
    [SerializeField] Button removeAllArtifactBtn;
    [SerializeField] UIRemoveAllAfPanel removeAllArtifactPanel;


    private void Awake()
    {
        toSelCardBtn.onClick.AddListener(ToSerlCard);
        toSelPassiveBtn.onClick.AddListener(ToSelPassive);
        removeAllArtifactBtn.onClick.AddListener(() => { removeAllArtifactPanel?.SetActive(true); });
    }
    void ToSerlCard()
    {
        Debug.Log("카드 선택으로");
    }
    void ToSelPassive()
    {
        Debug.Log("패시브 유물 선택으로");
    }

}
