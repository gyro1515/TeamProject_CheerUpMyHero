using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

public class UIAdvancedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
//public class UIAdvancedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField]
    [Tooltip("이 시간(초) 이상 누르면 '홀드'로 간주합니다.")]
    private float holdThreshold = 0.1f;

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

    // 포인터가 버튼을 누르기 시작했을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        isLongHoldTriggered = false; // 상태 리셋
        pointerDownTime = Time.unscaledTime;  // 시간 기록 시작
        Debug.Log("Pointer Down");
    }

    // Update()는 매 프레임 호출됩니다.
    private void Update()
    {
        // 1. 버튼이 눌려있지 않거나, 2. 이미 롱홀드(팝업)가 발동되었다면
        // Update에서 더 이상 시간을 체크x
        if (!isPointerDown || isLongHoldTriggered) return;
        
        // 누른 시간 계산
        float pressDuration = Time.unscaledTime - pointerDownTime;

        // 누른 시간이 설정한 임계값(1초)을 넘었는지 확인
        if (pressDuration >= holdThreshold)
        {
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
            // 일정 시간 미만으로 눌렀다 뗐음 (짧은 클릭)
            Debug.Log("Short Click");
            onShortClick?.Invoke();
        }

        // 상태 리셋
        isLongHoldTriggered = false;
    }

    // 포인터가 버튼 영역 밖으로 나갔을 때 (모든 행동 취소)
    /*public void OnPointerExit(PointerEventData eventData)
    {
        if(!Interactable) return; // 클릭 불가능 상태면 무시
        // 누르고 있던 상태에서 나갔다면
        if (isPointerDown)
        {
            // 팝업이 이미 떴다면 팝업을 닫아줘야 합니다.
            if (isLongHoldTriggered)
            {
                Debug.Log("Hold Cancelled");
                onHoldRelease?.Invoke(); // 팝업 닫기 이벤트 재사용
            }
            else
            {
                Debug.Log("Short Click Cancelled");
            }

            // 모든 상태를 리셋하여 OnPointerUp이 실행되지 않도록 함
            isPointerDown = false;
            isLongHoldTriggered = false;
        }
    }*/
}
