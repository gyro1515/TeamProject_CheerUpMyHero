using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//유럽에서는 광고에 사전 동의가 필요해서 물어봐야 하는데, 일단은 무조건 동의한것으로 치는 걸로 합시다.
//구글 플레이 스토어에서 출시 국가 - 대한민국이면 상관 없는데, 유럽이 포함되면 아래 코드를 진짜로 바꿔야 합니다.
public class GoogleMobileAdsConsentController : MonoBehaviour
{
    // "광고 요청해도 되니?" -> "응, 무조건 돼 (True)"
    public bool CanRequestAds => true;

    /// <summary>
    /// 동의 화면을 띄우는 척하면서 바로 성공 콜백을 보냄
    /// </summary>
    public void GatherConsent(Action<string> onComplete)
    {
        Debug.Log("가짜 동의 컨트롤러: 동의 절차를 패스합니다.");

        // 에러 없음(null)으로 즉시 완료 처리
        onComplete?.Invoke(null);
    }

    /// <summary>
    /// 개인정보 설정 화면 열기 (가짜)
    /// </summary>
    public void ShowPrivacyOptionsForm(Action<string> onComplete)
    {
        Debug.Log("가짜 동의 컨트롤러: 설정 화면을 열었습니다.");
        onComplete?.Invoke(null);
    }
}
