using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SynergyIconSlot : MonoBehaviour
{
    [Header("시너지 표시 UI 슬롯 설정")]
    [SerializeField] Image synergeIcon;
    [SerializeField] Button iconBtn;
    [SerializeField] RectTransform iconRectTransform;
    public void Init(Sprite iconSprite, UnityAction btnAction)
    {
        synergeIcon.sprite = iconSprite;
        iconBtn.onClick.AddListener(btnAction);
    }
    public void ResizeIcon(float width)
    {
        iconRectTransform.sizeDelta = new Vector2(width, 0);
    }
}
