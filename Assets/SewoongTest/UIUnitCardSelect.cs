using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitCardSelect : BaseUI
{
    [SerializeField] InfiniteScroll infiniteScroll;
    [SerializeField] Button selectButton;

    private void OnEnable()
    {
        selectButton?.onClick.AddListener(onSelectButtonPress);
    }

    private void OnDisable()
    {
        selectButton?.onClick.RemoveListener(onSelectButtonPress);
    }

    void onSelectButtonPress()
    {
        int selectedIndex = infiniteScroll.SendSelectedUnit();

        if (selectedIndex == -1)
        {
            Debug.Log("카드 선택이 정상적으로 이루어지지 않았습니다");
        }
        else
        {
            Debug.Log($"현재 선택된 카드 {selectedIndex}번");
        }
    }
}
