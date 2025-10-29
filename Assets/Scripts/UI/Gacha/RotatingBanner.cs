using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RotatingBanner : MonoBehaviour
{
    [Header("배너 이미지 목록")]
    [Tooltip("자동 순환시킬 이미지들 (순서대로)")]
    [SerializeField] private Image[] bannerImages;

    [Header("설정")]
    [Tooltip("각 이미지를 보여줄 시간 (초)")]
    [SerializeField] private float displayTime = 5.0f;

    private Coroutine _animationCoroutine; 

    // 오브젝트가 활성화될 때마다 배너 순환을 시작합니다.
    private void OnEnable()
    {
        // 안전하게 코루틴 중복 실행 방지
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(AnimateBanners());
    }

    // 오브젝트가 비활성화되면 배너 순환을 멈춥니다.
    private void OnDisable()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
        }
    }

    // 이미지를 순환시키는 메인 로직
    private IEnumerator AnimateBanners()
    {
        if (bannerImages == null || bannerImages.Length == 0)
        {
            Debug.LogWarning("배너에 연결된 이미지가 없습니다.");
            yield break; // 이미지가 없으면 코루틴 종료
        }

        // --- 초기 상태 설정 ---
        // 1번 이미지를 맨 앞으로, 나머지는 뒤로 보냅니다.
        for (int i = 0; i < bannerImages.Length; i++)
        {
            if (bannerImages[i] != null)
            {
                bannerImages[i].transform.SetAsFirstSibling();
            }
        }

        int currentIndex = 0;

        // 이 오브젝트가 활성화되어 있는 동안 무한 반복
        while (true)
        {
            // 1. 현재 인덱스의 이미지를 가져옵니다.
            Image currentImage = bannerImages[currentIndex];

            if (currentImage != null)
            {
                // 2. 이미지를 맨 앞으로 가져옵니다 (하이어라키 맨 아래로 이동).
                currentImage.transform.SetAsLastSibling();
            }

            // 3. 5초 동안 기다립니다.
            yield return new WaitForSeconds(displayTime);

            // 4. 다음 이미지 인덱스로 이동합니다 (맨 끝이면 0으로 순환).
            currentIndex = (currentIndex + 1) % bannerImages.Length;
        }
    }
}