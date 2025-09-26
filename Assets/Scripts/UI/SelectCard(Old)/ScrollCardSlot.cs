using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollCardSlot : MonoBehaviour
{
    private int cardIndex;

    [SerializeField] Button button;
    [SerializeField] GameObject checkIndicator;
    [SerializeField] GameObject cannotSelectIndicator;
    private bool isChecked = false;

    public event Action<int, bool> onSelectSlot;

    public int testResourcesNeeded = 50;

    //나중에 추가될 것
    //카드 데이터(아이콘, 데이터 등 포함)

    //카드 아이콘

    //카드 텍스트
    //[SerializeField] TMP_Text descriptionText;


    public void Init(int index)
    {
        cardIndex = index;
        button.onClick.AddListener(SelectCardToggle);
    }

    private void OnDisable()
    {
        button?.onClick.RemoveAllListeners();
        Destroy(this.gameObject);
    }

    void SelectCardToggle()
    {
        if (!isChecked)
        {
            checkIndicator.SetActive(true);
        }
        else
        {
            checkIndicator.SetActive(false);
        }

        isChecked = !isChecked;

        //cardindex번 카드 버튼이 눌렸다고 외부에 전달
        onSelectSlot?.Invoke(cardIndex, isChecked);
    }


    public void ButtonFormExternal()
    {
        SelectCardToggle();
    }


    //선택 가능 여부 계산 함수
    public void CalculateCanCheck()
    {
        //리소스 매니저에서 자원 가져오기
        //일단 테스트 자원 100;
        if (testResourcesNeeded > 100)
        {
            cannotSelectIndicator.SetActive(true);
        }

    }
}
