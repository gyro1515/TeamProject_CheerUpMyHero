using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasRenderer))]
public class PopupBackground : MonoBehaviour
{
    [SerializeField] private BaseUI _parentPopup;

    private void Awake()
    {
        GetComponent<Image>().color = new Color(1, 1, 1, 0); 
        GetComponent<Button>().onClick.AddListener(CloseParentPopup);
    }

    private void CloseParentPopup()
    {
        //Debug.Log("꺼짐");
        if (_parentPopup != null)
        {
            _parentPopup.CloseUI();
        }
    }
}