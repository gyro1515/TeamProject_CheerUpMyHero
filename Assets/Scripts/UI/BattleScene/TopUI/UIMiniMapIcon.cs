using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class UIMiniMapIcon : BasePoolable
{
    public Image IconImage {  get; private set; }
    public RectTransform RectTransform { get; private set; }
    
    private void Awake()
    {
        IconImage = GetComponent<Image>();
        RectTransform = GetComponent<RectTransform>();
    }

    public void SetColor(Color32 color)
    {
        if (IconImage != null)
            IconImage.color = color;
    }

    public void ResetRectTransform()
    {
        RectTransform.localScale = Vector3.one;
        //Debug.Log($"{RectTransform.offsetMin} / {RectTransform.offsetMax}");
        // 앵커 y =  0/1이므로 tom 0, bottom 0 설정
        RectTransform.offsetMax = new Vector2(RectTransform.offsetMax.x, 0f);
        RectTransform.offsetMin = new Vector2(RectTransform.offsetMin.x, 0f);
        // 앵커 x = 0.5/0.5이므로 sizeDelta로 width 설정
        RectTransform.sizeDelta = new Vector2(50f, RectTransform.sizeDelta.y); // width 설정
    }
    public void ResetRectTransformForHeroAndBoss()
    {
        RectTransform.localScale = Vector3.one;
        //Debug.Log($"{RectTransform.offsetMin} / {RectTransform.offsetMax}");
        RectTransform.offsetMax = new Vector2(RectTransform.offsetMax.x, -24.5f);
        RectTransform.offsetMin = new Vector2(RectTransform.offsetMin.x, 0f);
        RectTransform.sizeDelta = new Vector2(70f, RectTransform.sizeDelta.y); // width 설정
    }


    public override void ReleaseSelf()
    {
        transform.SetParent(ObjectPoolManager.Instance.poolTransformsDic[PoolType.UIMinimapIcon]);
        base.ReleaseSelf();
    }

}
