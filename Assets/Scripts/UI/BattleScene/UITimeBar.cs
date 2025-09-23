using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UITimeBar : MonoBehaviour
{
    [SerializeField] Image[] smallTimeBar = new Image[20];
    
    private WaitForSeconds wait30s = new WaitForSeconds(30f);
    private int timeIndex = 0;

    private void Start()
    {
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
