using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeroSpeachForPreSpawn : MonoBehaviour
{
    [Header("소환 전 대사 설정")]
    [SerializeField] TextMeshProUGUI heroSpeachText;
    [SerializeField] CanvasGroup heroSpeachCanvasGroup;

    private void Awake()
    {
        heroSpeachCanvasGroup.alpha = 0;
        heroSpeachCanvasGroup.interactable = false;
        heroSpeachCanvasGroup.blocksRaycasts = false;
    }
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        FadeManager.FadeInUI(heroSpeachCanvasGroup, null, false);
        StartCoroutine(FadeOutRoutine());
    }
    public void InitHeroSpeachForPreSpawn(HeroData data)
    {
        heroSpeachText.text = data.preSpawnSpeech;
    }
    IEnumerator FadeOutRoutine()
    {
        // 2.65초 대사 유지 후 페이드 아웃 -> 총 3초 유니
        yield return new WaitForSeconds(2.65f);
        FadeManager.FadeOutUI(heroSpeachCanvasGroup, () => { gameObject.SetActive(false); });
    }
}
