using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PopupBackground : MonoBehaviour
{
    private BaseUI _parentPopup;

    private void Awake()
    {
        _parentPopup = GetComponentInParent<BaseUI>();

        GetComponent<Button>().onClick.AddListener(CloseParentPopup);
    }

    private void CloseParentPopup()
    {
        if (_parentPopup != null)
        {
            _parentPopup.CloseUI();
        }
    }
}