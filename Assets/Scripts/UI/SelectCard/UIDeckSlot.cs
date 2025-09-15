using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDeckSlot : MonoBehaviour
{
    private int slotIndex;
    [SerializeField] Button button;
    [SerializeField] TMP_Text testText;
    [SerializeField] GameObject deployHighlighter;

    public event Action<int> onManualEmptySlot;


    private void OnEnable()
    {
        button.onClick.AddListener(ManualEmptySlot);

    }

    private void OnDisable()
    {
        button?.onClick.RemoveAllListeners();
    }

    public void SetIndex(int index)
    {
        slotIndex = index;  
    }


    //나중에 int -> CardData등으로 변경
    public void SetDeckSlot(int data)
    {
        button.gameObject.SetActive(true);
        testText.text = data.ToString();
        deployHighlighter.SetActive(true);
    }

    public void EmptySlot()
    {
        button.gameObject.SetActive(false);
        testText.text = "";
        deployHighlighter.SetActive(false);
    }

    void ManualEmptySlot()
    {
        onManualEmptySlot?.Invoke(slotIndex);
    }



}
