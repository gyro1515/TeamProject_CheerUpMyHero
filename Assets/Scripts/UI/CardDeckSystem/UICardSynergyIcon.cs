using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICardSynergyIcon : MonoBehaviour
{
    [SerializeField] List<Image> synergyIcons = new List<Image>();

    public void SetSynergyIcons(List<Sprite> icons)
    {
        gameObject.SetActive(true);
        // 아이콘 설정
        for (int i = 0; i < synergyIcons.Count; i++)
        {
            if (i < icons.Count) // 아이콘이 있으면 설정
            {
                synergyIcons[i].sprite = icons[i];
            }
        }
    }
}
