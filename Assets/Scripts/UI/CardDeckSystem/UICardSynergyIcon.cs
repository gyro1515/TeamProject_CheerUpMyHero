using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICardSynergyIcon : MonoBehaviour
{
    [SerializeField] List<Image> synergyIcons = new List<Image>();

    List<UISynergyIconPressHandler> uISynergyIconPressHandlers = new List<UISynergyIconPressHandler>();

    bool isInit = false;
    private void Awake()
    {
        CheckInit();
    }
    public void SetSynergyIcons(List<(Sprite, UnitSynergyType)> icons, BaseUnitData data)
    {
        CheckInit();
        gameObject.SetActive(true);
        // 아이콘 설정
        for (int i = 0; i < synergyIcons.Count; i++)
        {
            if (i < icons.Count) // 아이콘이 있으면 설정
            {
                //synergyIcons[i].sprite = icons[i];
                uISynergyIconPressHandlers[i].SetData(icons[i].Item1, icons[i].Item2, data);
            }
        }
    }
    void CheckInit()
    {
        if (isInit) return;
        isInit = true;
        for (int i = 0; i < synergyIcons.Count; i++)
        {
            synergyIcons[i].alphaHitTestMinimumThreshold = 0.1f; // 알파 테스트 설정
            UISynergyIconPressHandler uISynergyIconPressHandler = synergyIcons[i].gameObject.AddComponent<UISynergyIconPressHandler>();
            uISynergyIconPressHandler.Init();
            uISynergyIconPressHandlers.Add(uISynergyIconPressHandler);
        }
    }
}
