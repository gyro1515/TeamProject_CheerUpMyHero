using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISwipeIndex : MonoBehaviour
{
    [Header("스와이프 인덱스 UI 설정")]
    [SerializeField] List<GameObject> uiList = new List<GameObject>();

    public void SetIndexOn(int idx)
    {
        for (int i = 0; i < uiList.Count; i++)
        {
            uiList[i].SetActive(i == idx);
        }
    }
    
}
