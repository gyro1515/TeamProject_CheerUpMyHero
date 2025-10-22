using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutSceneForHeroSpawn : MonoBehaviour
{
    [Header("영웅 소환 설정")]
    [SerializeField] CanvasGroup cutSceneCanvasGroup;
    [SerializeField] Image cutSceneImg;
    [SerializeField] RectTransform cutSceneImgRectTransform;
    [SerializeField] Vector2 orginPos;
    [SerializeField] Vector2 startMovePos;
    [SerializeField] Ease startMoveEase;
    [SerializeField] Ease endMoveEase;
    [SerializeField] float duration = 2f;
    private void Awake()
    {
        cutSceneCanvasGroup.alpha = 0;
        cutSceneCanvasGroup.interactable = false;
        cutSceneCanvasGroup.blocksRaycasts = false;
    }
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        orginPos = cutSceneImgRectTransform.anchoredPosition;
        cutSceneImgRectTransform.anchoredPosition = startMovePos;
        cutSceneCanvasGroup.DOFade(1f, 0.2f);
        cutSceneImgRectTransform.DOAnchorPosX(orginPos.x, 0.2f).SetEase(startMoveEase).OnComplete(() =>
        {
            StartCoroutine(FadeOutRoutine());
        });
    }
    IEnumerator FadeOutRoutine()
    {
        // 유지 몇초?
        yield return new WaitForSeconds(duration);
        //cutSceneCanvasGroup.DOFade(0f, 0.2f);
        cutSceneImgRectTransform.DOAnchorPosX(startMovePos.x, 0.2f).SetEase(endMoveEase).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
    public void InitCutSceneForHeroSpawn(HeroData data)
    {
        cutSceneImg.sprite = data.spawnSprite;
    }


}
