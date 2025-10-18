using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutSceneForFirstWave : BasePopUpUI
{
    [Header("첫 번째 웨이브 컷씬 설정")]
    [SerializeField] Image cutSceneImg;
    [SerializeField] TextMeshProUGUI cutSceneText;
    [SerializeField] float cutSceneDuration = 5f;
    bool canClose = true;
    public override void OpenUI()
    {
        base.OpenUI();
        StartCoroutine(FadeOutRoutine());
    }
    public override void OnBackPressed()
    {
        // 닫히는 중이라면 무시
        if (!canClose) return;
        canClose = false;
        base.OnBackPressed();
    }
    public void InitCutSceneForFirstWave(Sprite sprite, string cutSceneText)
    {
        cutSceneImg.sprite = sprite;
        this.cutSceneText.text = cutSceneText;
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
