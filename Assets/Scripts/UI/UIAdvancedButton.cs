using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAdvancedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    //[SerializeField]
    [Tooltip("이 시간(초) 이상 누르면 '홀드'로 간주합니다.")]
    private float holdThreshold = 0.15f;

    // 1. 홀드 시간 미만으로 짧게 클릭했을 때
    public event Action onShortClick;
    // 2. 홀드 시간을 정확히 달성했을 때 (누르고 있는 상태에서 1회 호출)
    public event Action onHoldStart;
    // 3. 홀드 시간 이상 누르고 있다가 뗐을 때 (팝업 닫기 등)
    public event Action onHoldRelease;

    // 클릭가능 여부
    //public bool Interactable { get; set; } = true;

    // --- 내부 상태 변수 ---
    private bool isPointerDown = false;      // 현재 누르고 있는지
    private bool isLongHoldTriggered = false; // onHoldStart가 이미 실행되었는지
    private float pointerDownTime = 0f;     // 누르기 시작한 시간

    // 스크롤뷰 내에서 사용 시, 스크롤뷰 참조
    private ScrollRect parentScroll;
    // 드래그 임계값:
    // - 스크롤뷰 안(도감 카드 등): 3f. 작은 움직임도 스크롤 의도로 인식해야 함.
    // - 스크롤뷰 밖(일반 버튼): 15f. 모바일에서 손가락 자연 흔들림 허용.
    // Awake에서 parentScroll 유무에 따라 결정.
    float pixelDragThreshold;
    float pixelDragThresholdSqr;
    private Vector2 pressScreenPos;   // 누른 시점의 화면 좌표

    private void Awake()
    {
        parentScroll = GetComponentInParent<ScrollRect>();
        pixelDragThreshold = (parentScroll != null) ? 3f : 15f;
        pixelDragThresholdSqr = pixelDragThreshold * pixelDragThreshold;
    }

    private void OnDisable()
    {
        // 비활성화 시 상태 깨끗하게 리셋 (씬 전환 등에서 isPointerDown이 끼는 것 방지)
        ResetState();
    }

    private void ResetState()
    {
        isPointerDown = false;
        isLongHoldTriggered = false;
    }

    // 포인터가 버튼을 누르기 시작했을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        // 멀티터치/엣지케이스 보호: 이미 누르고 있는 상태라면 새 Down 무시.
        // 두 손가락이 동시에 누르거나 빠른 재탭이 들어와도 첫 입력의 상태(hold 등)를 유지.
        if (isPointerDown) { Debug.Log("Pointer Down return"); return; }

        pressScreenPos = eventData.position;
        isPointerDown = true;
        isLongHoldTriggered = false; // 상태 리셋
        pointerDownTime = Time.unscaledTime;  // 시간 기록 시작
        Debug.Log("Pointer Down");
    }
    private void Update()
    {
        // 1. 버튼이 눌려있지 않거나, 2. 이미 롱홀드(팝업)가 발동되었다면
        // Update에서 더 이상 시간을 체크x
        if (!isPointerDown || isLongHoldTriggered) return;

        // 누른 시간 계산
        float pressDuration = Time.unscaledTime - pointerDownTime;

        // 누른 시간이 설정한 임계값(1초)을 넘었는지 확인
        if (pressDuration >= holdThreshold && isPointerDown)
        {
            if(parentScroll)
            {
                // 드래그 중인지 확인
                Vector2 currentPos = Input.mousePosition;
                if ((currentPos - pressScreenPos).sqrMagnitude > pixelDragThresholdSqr)
                {
                    return; // 스크롤 중이면 롱홀드 발동 안함
                }
            }
            Debug.Log("Hold Start!");
            isLongHoldTriggered = true; // 롱홀드 발동! (Update에서 중복 실행 방지)
            onHoldStart?.Invoke();       // 팝업 띄우기 이벤트 실행
        }
    }

    // 포인터가 버튼에서 떼졌을 때 (클릭 성공 또는 홀드 종료)
    public void OnPointerUp(PointerEventData eventData)
    {
        // OnPointerExit에서 이미 처리되었다면 실행하지 않음
        if (!isPointerDown) return;

        isPointerDown = false;

        if (isLongHoldTriggered)
        {
            // 일정 시간 이상 누르다가 뗐음 (팝업 닫기)
            Debug.Log("Hold Released");
            onHoldRelease?.Invoke();
        }
        else
        {
            // 드래그 중인지 확인
            Vector2 currentPos = eventData.position;
            if ((currentPos - pressScreenPos).sqrMagnitude > pixelDragThresholdSqr)
            {
                return; // 스크롤 중이면 클릭 안함
            }
            // 일정 시간 미만으로 눌렀다 뗐음 (짧은 클릭)
            Debug.Log("Short Click");
            onShortClick?.Invoke();
        }

        // 상태 리셋
        isLongHoldTriggered = false;
    }

    // 포인터가 버튼 영역 밖으로 나갔을 때
    // 자연 드래그(pixelDragThreshold 이내)는 클릭 후보로 유지하고,
    // 그 이상 멀어진 경우에만 상태를 정리해서 isPointerDown 영구 잠금을 방지함.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPointerDown) return;

        // 약간의 드래그는 클릭으로 인정. 임계값 이내면 상태 유지하고 OnPointerUp을 기다림.
        Vector2 currentPos = eventData.position;
        if ((currentPos - pressScreenPos).sqrMagnitude <= pixelDragThresholdSqr)
        {
            return;
        }

        // 임계값을 명확히 넘어 멀어졌다면 클릭/홀드 취소 + 상태 정리
        if (isLongHoldTriggered)
        {
            Debug.Log("Hold Cancelled (Exit)");
            onHoldRelease?.Invoke();
        }
        else
        {
            Debug.Log("Short Click Cancelled (Exit)");
        }

        ResetState();
    }

}
