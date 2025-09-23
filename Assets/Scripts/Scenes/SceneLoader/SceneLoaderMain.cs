using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderMain : MonoBehaviour
{
    private void Awake()
    {
        UIManager.Instance.GetUI<MainScreenUI>();
      //  UIManager.Instance.GetUI<UISelectCard>().CloseUI();
        UIManager.Instance.GetUI<UIStageSelect>().CloseUI();
    }
}
