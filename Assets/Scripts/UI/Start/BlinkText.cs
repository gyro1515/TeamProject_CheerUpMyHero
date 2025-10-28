using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

[RequireComponent(typeof(CanvasGroup))] 
public class BlinkingText : MonoBehaviour
{
    // 깜빡이는 속도 (초). 1.0f로 설정 시 1초 보이고 1초 안 보여서 총 2초 주기
    [SerializeField] private float blinkInterval = 1.0f;

    private CanvasGroup canvasGroup;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }
        blinkCoroutine = StartCoroutine(BlinkLoop());
    }

    private void OnDisable()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(blinkInterval);

            canvasGroup.alpha = 0f;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}