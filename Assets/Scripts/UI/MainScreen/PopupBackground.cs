using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PopupBackground : MonoBehaviour
{
    private BaseUI _parentPopup;
    [SerializeField] BaseUI _parentUI;

    private void Awake()
    {

        GetComponent<Button>().onClick.AddListener(CloseParentPopup);
    }

    private void CloseParentPopup()
    {
        Debug.Log("꺼짐");
        if (_parentPopup != null)
        {
            _parentPopup.CloseUI();
        }
    }
}