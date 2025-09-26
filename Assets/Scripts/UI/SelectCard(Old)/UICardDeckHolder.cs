//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class UICardDeckHolder : MonoBehaviour
//{
//    private UISelectCard uISelectCard;

//    [SerializeField] UIDeckSlot[] slots;

//    private void Awake()
//    {
//        //현재 구조: 동적생성하긴 불필요하게 리소스 쓰고, 인스펙터에 연결하기 귀찮아서 만들고 보니 뭔가 이도저도 아닌 느낌
//        int childCount = transform.childCount;
//        Transform[] childTransformsArray = new Transform[childCount];
//        slots = new UIDeckSlot[childCount];

//        for (int i = 0; i < childCount; i++)
//        {
//            childTransformsArray[i] = transform.GetChild(i);
//            UIDeckSlot slot = childTransformsArray[i].GetComponent<UIDeckSlot>();
//            if (slot != null)
//            {
//                slots[i] = slot;
//            }
//            else
//            {
//                Debug.LogError($"[UISelectCard - UIUICardDeckHolder] 자식 오브젝트에 UIDeckSlot이 아닌 것이 포함되어 있습니다");
//            }
//        }
//    }


//    //참조 전달받는 용도
//    public void Init(UISelectCard uISelectCard)
//    {
//        this.uISelectCard = uISelectCard;
//    }


//    private void OnEnable()
//    {
//        for (int i = 0; i < slots.Length; i++)
//        {
//            slots[i].SetIndex(i);
//            slots[i].onManualEmptySlot += uISelectCard.CancelDeckBySlot;
//        }

//        uISelectCard.ReloadDeck();
//    }

//    private void OnDisable()
//    {
//        for (int i = 0; i < slots.Length; i++)
//        {
//            slots[i].onManualEmptySlot -= uISelectCard.CancelDeckBySlot;
//        }
//    }

//    //나중에 int -> CardData등으로 변경
//    public void DeployDeck(List<int> deckList)
//    {
//        int listSize = deckList.Count;
        
//        for (int i = 0; i < listSize; i++)
//        {
//            slots[i].SetDeckSlot(deckList[i]);
//        }

//        for (int i = listSize; i < slots.Length; i++)
//        {
//            slots[i].EmptySlot();
//        }
//    }

//}
