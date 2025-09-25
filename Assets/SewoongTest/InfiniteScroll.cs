using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 네임스페이스 추가

public class InfiniteScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private ScrollRect scrollRect;
    private RectTransform contentRect;

    private List<UIUnitCardInScroll> cardUIList = new(); 
    private List<int> allCardList = new();
    private List<int> filteredCardList = new();

    private List<int> testEvenList = new();

    private int indexOffset = -2; // 카드 UI의 첫번째 카드와 가운데 카드 사이 간격
    private int currentFirstCardIndex = 0; // 현재 contentRect의 맨 왼쪽 카드에 할당된 데이터의 인덱스

    private float cardWithSpaceSize;

    //위치 비교용
    private Vector2 prevContentAnchoredPos;

    //드래그 중인지 확인하는 플래그
    private bool isDragging = false;

    //드래그 후 Snap(중앙으로 돌아가는 중)임을 나타내는 플래그
    private bool isSnapping = false;
    //Snap 시작되는 속도
    private float snapVelocityThreshold = 400f;

    private bool isHidden = false;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        contentRect = scrollRect.content;
    }

    private void Start()
    {
        //테스트용 카드 int 리스트 생성 
        for (int i = 0; i < 20; i++)
        {
            allCardList.Add(i);
        }
        for (int i = 0; i < 0; i += 2)
        {
            testEvenList.Add(i);
        }

        StartCoroutine(Init(allCardList));

    }

    IEnumerator Init(List<int> defaultList)
    {
        //한 프레임 기다려야, HorizontalLayoutGroup에서 카드의 크기를 정함. 안 그러면 크기 0으로 나옴
        yield return null;

        //최초 카드 세팅
        for (int i = 0; i < contentRect.childCount; i++)
        {
            //UI 불러오기            
            if(contentRect.GetChild(i).TryGetComponent<UIUnitCardInScroll>(out UIUnitCardInScroll cardUI))
            {
                cardUIList.Add(cardUI);
            }
            else
            {
                Debug.Log($"{contentRect.GetChild(i).gameObject.name}: UIUnitCardInScroll가 없습니다");
            }
        }

        //카드 크기 + spacing 가져오기
        if (cardUIList.Count > 0)
        {
            float cardSize = cardUIList[0].GetComponent<RectTransform>().rect.width;
            HorizontalLayoutGroup layoutGroup = contentRect.GetComponent<HorizontalLayoutGroup>();
            float spacing = layoutGroup != null ? layoutGroup.spacing : 0;
            cardWithSpaceSize = cardSize + spacing;
        }

        ResetCardData(defaultList);
    }

    void ResetCardData(List<int> newList)
    {
        filteredCardList.Clear();
        filteredCardList.AddRange(newList);

        //혹시 카드 없으면 아무것도 표시 안하기
        if (filteredCardList.Count == 0)
        {
            isHidden = true;
            scrollRect.enabled = false;
            foreach (UIUnitCardInScroll card in cardUIList)
            {
                card.gameObject.SetActive(false);
            }
            return;
        }

        //혹시 방금 카드 없었는데 지금 있으면 다시 켜기
        if (isHidden)
        {
            isHidden = false;
            foreach (UIUnitCardInScroll card in cardUIList)
            {
                card.gameObject.SetActive(true);
            }
        }

        scrollRect.enabled = true;

        //CardUI 순서 초기화
        for (int i = 0; i < cardUIList.Count; i++)
        {
            cardUIList[i].transform.SetSiblingIndex(i);
        }

        //데이터 할당
        //현재 구조: [0 1 2 3 4] 5개의 카드 슬롯이 있고, 이 중 2번이 맨 중앙이며 맨 처음에 보여짐 => 0번이 아닌 2번부터 인덱스가 시작되어야 함
        //=> 총 카드 20개 기준 [18 19 0 1 2]
        for (int i = 0; i < cardUIList.Count; i++)
        {
            int index = (i + indexOffset + filteredCardList.Count) % filteredCardList.Count;

            cardUIList[i].TestUpdateData(filteredCardList[index]);
        }

        if (filteredCardList.Count == 1)
        {
            LockScroll();
        }

        currentFirstCardIndex = (indexOffset + filteredCardList.Count) % filteredCardList.Count;
        contentRect.anchoredPosition = Vector2.zero;
        prevContentAnchoredPos = contentRect.anchoredPosition;
    }

    void LockScroll()
    {
        scrollRect.enabled = false;
    }
    
    // 드래그 시작 시 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        isSnapping = false;
        StopAllCoroutines();
    }

    // 드래그 종료 시 호출
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    void Update()
    {
        // 사용자가 드래그 중이면 위치 보정 로직을 실행하지 않음
        if (isDragging)
        {
            // 드래그 중일 때도 prevContentAnchoredPos는 갱신
            prevContentAnchoredPos = contentRect.anchoredPosition;
            return;
        }

        CardSwap();

        HandleSnap();

        prevContentAnchoredPos = contentRect.anchoredPosition;

        //테스트용
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetCardData(testEvenList);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ResetCardData(allCardList);
        }
    }

    void CardSwap()
    {
        bool isScrollingRight = contentRect.anchoredPosition.x < prevContentAnchoredPos.x;

        // 오른쪽으로 스크롤(왼쪽으로 드래그)
        if (isScrollingRight)
        {

            if (contentRect.anchoredPosition.x < -cardWithSpaceSize)
            {
                RectTransform firstCardRect = contentRect.GetChild(0).GetComponent<RectTransform>();
                UIUnitCardInScroll firstCardUI = firstCardRect.GetComponent<UIUnitCardInScroll>();

                //예시: [0 1 2 3 4] => [1 2 3 4 5] 경우 0을 맨 뒤로 보내고 5를 새로 로드해야 하므로 nextDataIndex = 5
                int nextDataIndex = (currentFirstCardIndex + cardUIList.Count) % filteredCardList.Count;

                //데이터 로드 후 뒤로 보내기
                firstCardUI.TestUpdateData(filteredCardList[nextDataIndex]);
                firstCardRect.SetAsLastSibling();

                // Content 위치 보정: 한 카드만큼 오른쪽으로 이동
                contentRect.anchoredPosition += new Vector2(cardWithSpaceSize, 0);

                //맨 앞 인덱스 수정(예시의 경우 1)
                currentFirstCardIndex = (currentFirstCardIndex + 1) % filteredCardList.Count;
            }
        }
        // 왼쪽으로 스크롤
        else
        {
            if (contentRect.anchoredPosition.x > 0)
            {
                RectTransform lastCardRect = contentRect.GetChild(contentRect.childCount - 1).GetComponent<RectTransform>();
                UIUnitCardInScroll lastCardUI = lastCardRect.GetComponent<UIUnitCardInScroll>();

                //예시: [2 3 4 5 6] => [1 2 3 4 5] 경우 6을 맨 앞으로 보내고 1을 새로 로드해야 하므로 nextDataIndex = 1
                int nextDataIndex = (currentFirstCardIndex - 1 + filteredCardList.Count) % filteredCardList.Count;

                //데이터 로드 후 뒤로 보내기
                lastCardUI.TestUpdateData(filteredCardList[nextDataIndex]);
                lastCardRect.SetAsFirstSibling();

                // Content 위치 보정: 한 카드만큼 왼쪽으로 이동
                contentRect.anchoredPosition -= new Vector2(cardWithSpaceSize, 0);

                //맨 앞 인덱스 수정(예시의 경우 1)
                currentFirstCardIndex = nextDataIndex;
            }
        }
    }

    void HandleSnap()
    {
        //스냅 조건: 속도가 일정 이하로 떨어졌을 때(0 제외, 0이면 정지 후에도 계속 발동)
        if (!isDragging && !isSnapping && scrollRect.velocity.magnitude > 0 && scrollRect.velocity.magnitude < snapVelocityThreshold)
        {
            StartCoroutine(SnapToCenter());
        }
    }

    IEnumerator SnapToCenter()
    {
        isSnapping = true;

        //잔여 관성 제거
        scrollRect.velocity = Vector2.zero;

        //업데이트 문이 카드 다시 이동시키는거 방지
        isDragging = true;

        // 현재 Content 위치에서 가장 가까운 카드의 중앙 위치를 계산. 반올림을 사용해서 쉽게 구현
        float closestPosition = Mathf.Round(contentRect.anchoredPosition.x / cardWithSpaceSize) * cardWithSpaceSize;
        float targetX = closestPosition;

        // 부드럽게 중앙으로 이동
        float timer = 0f;
        float duration = 0.2f; // 스냅에 걸리는 시간
        Vector2 startPos = contentRect.anchoredPosition;
        Vector2 targetPos = new Vector2(targetX, contentRect.anchoredPosition.y);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            contentRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, timer / duration);
            yield return null;
        }

        contentRect.anchoredPosition = targetPos; // 정확한 위치로 마무리

        isDragging = false;
        isSnapping = false;
    }

    public int SendSelectedUnit()
    {
        int selectedIndex = -1;

        //오류 방지를 위해 터치 중이나 스냅 중이나 표시 안될 때는 선택 방지
        if (isDragging || isSnapping || isHidden)
            return -1;

        //현재 구조상 정지 상태에서 contentRect.anchoredPosition는 3번째 슬롯 중앙값(0 고정)이나 4번째 슬롯 중앙값(cardWithSpaceSize) 둘 중 하나
        //일단 귀찮으니까 둘 중에 어디인지 판단하게 하자
        float indexOffset = cardWithSpaceSize / 2;

        Debug.Log(contentRect.anchoredPosition.x);

        //현재 보고 있는 카드는 4번째 슬롯에 온 카드
        if (contentRect.anchoredPosition.x < - indexOffset)
        {
            int fourthIndex = (currentFirstCardIndex + 3) % filteredCardList.Count;
            selectedIndex = fourthIndex;
        }
        //현재 보고 있는 카드는 3번째 슬롯에 온 카드
        else
        {
            int thirdIndex = (currentFirstCardIndex + 2) % filteredCardList.Count;
            selectedIndex = thirdIndex;
        }

        return selectedIndex;
    }



}