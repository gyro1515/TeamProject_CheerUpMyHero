//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

////메인로직, 카드 슬롯 간의 중재자 역할?
//public class UIScrollCard : MonoBehaviour
//{
//    private UISelectCard uISelectCard;
//    [SerializeField] Transform cardSlotTransform;
//    [SerializeField] ScrollCardSlot cardSlotPrefab;

//    public List<ScrollCardSlot> SpawnedSlots { get; private set; } = new ();

//    public void Init(UISelectCard uISelectCard)
//    {
//        SpawnedSlots.Clear();


//        this.uISelectCard = uISelectCard;
//        for (int i = 0; i < uISelectCard.CardList.Count; i++)
//        {
//            ScrollCardSlot slot = Instantiate<ScrollCardSlot>(cardSlotPrefab, cardSlotTransform);
//            slot.Init(i);

//            slot.onSelectSlot += uISelectCard.SetDeck;
//            SpawnedSlots.Add(slot);

//            //자원에 따라 카드 선택 불가 테스트용
//            if (i == 3)
//            {
//                slot.testResourcesNeeded = 2000;
//            }

//            slot.CalculateCanCheck();
//        }

//    }

//    private void OnDisable()
//    {
//        foreach (ScrollCardSlot slot in SpawnedSlots)
//        {
//            slot.onSelectSlot -= uISelectCard.SetDeck;
//        }
//    }


//}
