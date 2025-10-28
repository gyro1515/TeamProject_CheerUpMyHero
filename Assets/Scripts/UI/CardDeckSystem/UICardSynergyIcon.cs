using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICardSynergyIcon : MonoBehaviour
{
    [SerializeField] List<Image> synergyIcons = new List<Image>();
    [SerializeField] List<Button> synergyIconButtons = new List<Button>();

    public void SetSynergyIcons(List<Sprite> icons, BaseUnitData data)
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
        for(int i = 0; i < synergyIconButtons.Count; i++)
        {
            Sprite tmpSprite = icons[i]; // 클로저 문제 해결용 임시 변수
            synergyIconButtons[i].onClick.AddListener(() => { Debug.Log($"{tmpSprite} 눌림"); });
        }
    }
}
