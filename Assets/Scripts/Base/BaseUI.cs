using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    public virtual void OpenUI()
    {
        gameObject.SetActive(true);
        //UIManager.Instance.SetCurrentPopup(this);
    }

    public virtual void CloseUI()
    {
        gameObject.SetActive(false);
        //UIManager.Instance.ClearCurrentPopup(this);
    }
}
