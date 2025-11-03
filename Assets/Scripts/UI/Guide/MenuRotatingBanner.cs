using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic; // List<> 사용을 위해 필요

/// <summary>
/// 여러 개의 배너 이미지를 일정 시간마다 순환시키고,
/// 페이지네이션(점 UI)을 함께 업데이트합니다.
/// </summary>
public class MenuRotatingBanner : MonoBehaviour
{
    [Header("배너 설정")]
    [SerializeField] private Image bannerImageUI; // 1. 배너 이미지를 표시할 UI Image 컴포넌트
    [SerializeField] private float rotationInterval = 5f; // 2. 각 이미지를 보여줄 시간 (5초)
    [SerializeField] private List<Sprite> bannerImages; // 3. 순환시킬 배너 이미지 스프라이트 목록

    [Header("페이지네이션 Dot UI 설정")]
    [SerializeField] private List<Image> dotIndicators; // 4. 점(Dot) UI Image 컴포넌트 목록 (4개)
    [SerializeField] private Sprite activeDotSprite;   // 5. 활성화된 점의 스프라이트 (채워진 원)
    [SerializeField] private Sprite inactiveDotSprite; // 6. 비활성화된 점의 스프라이트 (빈 원)

    private int currentBannerIndex = 0; // 현재 표시 중인 배너의 인덱스
    private Coroutine rotateCoroutine;    // 실행 중인 코루틴

    // 오브젝트가 활성화될 때 배너 순환을 시작합니다.
    private void OnEnable()
    {
        if (bannerImages == null || bannerImages.Count == 0)
        {
            Debug.LogWarning("MenuRotatingBanner: 배너 이미지가 없습니다.");
            return;
        }

        // 1. 첫 번째 배너로 초기화
        currentBannerIndex = 0;
        UpdateBannerAndDots();

        // 2. 순환 코루틴 시작
        StartRotating();
    }

    // 오브젝트가 비활성화될 때 코루틴을 안전하게 멈춥니다.
    private void OnDisable()
    {
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
            rotateCoroutine = null;
        }
    }

    // 배너 순환 코루틴을 (다시) 시작합니다.
    private void StartRotating()
    {
        // 이미 실행 중인 코루틴이 있다면 멈춥니다.
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
        }
        rotateCoroutine = StartCoroutine(RotateBannersRoutine());
    }

    // 5초마다 다음 배너로 넘기는 실제 순환 로직
    IEnumerator RotateBannersRoutine()
    {
        // 이 오브젝트가 활성화되어 있는 동안 무한 반복
        while (true)
        {
            // 5초(rotationInterval) 동안 기다립니다.
            yield return new WaitForSeconds(rotationInterval);

            // 다음 배너 인덱스로 이동합니다.
            // (배너 개수만큼 나눈 나머지 값을 사용해 0, 1, 2, 3, 0, 1... 순환)
            currentBannerIndex = (currentBannerIndex + 1) % bannerImages.Count;

            // UI를 갱신합니다.
            UpdateBannerAndDots();
        }
    }

    // 현재 인덱스에 맞춰 배너 이미지와 점(Dot) UI를 갱신합니다.
    private void UpdateBannerAndDots()
    {
        // 1. 메인 배너 이미지 변경
        if (bannerImageUI != null && currentBannerIndex < bannerImages.Count)
        {
            bannerImageUI.sprite = bannerImages[currentBannerIndex];
        }

        // 2. 점(Dot) UI 변경
        if (dotIndicators == null) return;

        for (int i = 0; i < dotIndicators.Count; i++)
        {
            if (dotIndicators[i] != null)
            {
                // 점의 인덱스(i)가 현재 배너 인덱스와 같으면 '활성' 스프라이트
                // 다르면 '비활성' 스프라이트로 변경
                dotIndicators[i].sprite = (i == currentBannerIndex) ? activeDotSprite : inactiveDotSprite;
            }
        }
    }
}