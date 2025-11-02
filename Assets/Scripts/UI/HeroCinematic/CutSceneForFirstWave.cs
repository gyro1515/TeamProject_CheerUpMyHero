using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CutSceneForFirstWave : BasePopUpUI
{
    [Header("첫 번째 웨이브 컷씬 설정")]
    [SerializeField] Image cutSceneImg;
    [SerializeField] TextMeshProUGUI cutSceneText;
    [SerializeField] TextMeshProUGUI cutSceneNameText;
    [SerializeField] float cutSceneDuration = 4f;
    [SerializeField] RectTransform containerTransform;
    //[SerializeField] RectTransform cutSceneImgRectTransform;
    bool canClose = false;
    PoolType heroType = PoolType.None;

    // 스크랩블 텍스트
    ScrambleText scrambleTextCom;
    protected override void Awake()
    {
        base.Awake();
        scrambleTextCom = gameObject.AddComponent<ScrambleText>();
    }
    public override void OpenUI()
    {
        // 이렇게 재정의하면 안되지만, 다양한 오프닝 이펙트를 주기 위해서 불가피하게 재정의

        // 기본적으로 페이드 인, 이동 등오로 완전히 열린 다음에 닫을 수 있도록 설정
        // 닫히는 시간도 완전히 열린 후, 3초 후에 닫히도록 설정

        // 기본 페이드 인
        Ease easeEfect = Ease.OutBounce;
        switch (heroType)
        {
            case PoolType.Hero_Unit1:
                // 기본 페이드 인
                base.OpenUI(StartFadeOutTimer); 
                return;
            case PoolType.Hero_Unit4:
                // 델란 마법사 페이든인 후 스크램블 1초

                // 페이든 인 끝나고 -> 스크램블 끝나고 -> StartFadeOutTimer 실행
                string tmpText = cutSceneText.text;
                cutSceneText.text = "";
                base.OpenUI(()=> scrambleTextCom.StartScramble(cutSceneText, tmpText, 1f, StartFadeOutTimer));

                return;
            case PoolType.Hero_Unit5:
                // 리네아 힐러 y축 회전 주면서 활성
                JustOpenUI();
                containerTransform.localScale = new Vector3(0, 1, 1);
                containerTransform.DOScaleX(1, 0.5f).onComplete += StartFadeOutTimer;
                return;

            // *********위까지는 다 리턴, 아래는 이동 이펙트 ************
            case PoolType.Hero_Unit2:
                // 세아 탱커 왼쪽에서 오른쪽
                easeEfect = Ease.Unset;
                break;
            case PoolType.Hero_Unit3:
                // 아르티아 궁수 왼쪽에서 오른쪽 outBound
                easeEfect = Ease.OutBounce;
                break;
            default:
                Debug.LogError($"{heroType} = 영웅 타입이 올바르지 않습니다.");
                return;
        }
        JustOpenUI();
        Vector2 orginPos1 = containerTransform.anchoredPosition;
        Vector2 startPos1 = containerTransform.anchoredPosition;
        startPos1.x -= 1100f;
        containerTransform.anchoredPosition = startPos1;
        containerTransform.DOAnchorPosX(orginPos1.x, 1f).SetEase(easeEfect).onComplete += StartFadeOutTimer;
    }
    public override void OnBackPressed()
    {
        // 닫히는 중이라면 무시
        if (!canClose) return;
        canClose = false;
        base.OnBackPressed();
    }
    public void InitCutSceneForFirstWave(HeroData data)
    {
        cutSceneImg.sprite = data.firstWaveSprite;
        cutSceneText.text = data.firstWaveSpeech;
        cutSceneNameText.text = $"{data.heroTitle} {data.unitName}";
        heroType = data.poolType;
    }
    void StartFadeOutTimer()
    {
        // 이 함수를 실행하면 ese/뒤로가기로 닫을 수 있도록 설정하고, 페이드 아웃 루틴 시작(4초 후 닫기)
        canClose = true;
        StartCoroutine(FadeOutRoutine());
    }
    IEnumerator FadeOutRoutine()
    {
        // 닫히는 중이라면 무시
        if (!canClose) yield break;
        // 5초 유지 후 페이드 아웃
        yield return new WaitForSeconds(cutSceneDuration);
        canClose = false;
        CloseUI();
    }
}
