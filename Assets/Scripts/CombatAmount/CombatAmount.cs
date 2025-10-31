using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CombatAmount : MonoBehaviour
{
    [SerializeField] TextMeshPro text;

    protected float duration = 1.5f;
    bool isInitialized = false;
    float timer = 0f;
    BasePoolable basePoolable;
    private void Awake()
    {
        basePoolable = GetComponent<BasePoolable>();
    }
    
    private void Update()
    {
        if (!isInitialized) return;

        timer += Time.unscaledDeltaTime;
        if (timer >= duration + 0.1f)
        {
            isInitialized = false;
            timer = 0f;
            basePoolable.ReleaseSelf();
        }
    }
    public virtual void SetAmount(int amount)
    {
        text.text = amount.ToString();
        isInitialized = true;
        timer = 0f;
        //gameObject.transform.
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
        text.DOColor(new Color(text.color.r, text.color.g, text.color.b, 0f), duration).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    
}
