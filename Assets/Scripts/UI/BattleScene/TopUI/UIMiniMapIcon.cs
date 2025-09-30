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
        RectTransform.offsetMax = new Vector2(RectTransform.offsetMax.x, 0f);
    }


    public override void ReleaseSelf()
    {
        transform.SetParent(ObjectPoolManager.Instance.poolTransformsDic[PoolType.UIMinimapIcon]);
        base.ReleaseSelf();
    }

}
