using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGachaWindow : MonoBehaviour
{
    private GachaSystem _gachaSystem;

    [SerializeField] Button oneGacha;
    //[SerializeField] Button tenGacha;

    private void Awake()
    {
        _gachaSystem = GetComponent<GachaSystem>();
    }

    private void OnEnable()
    {
        oneGacha.onClick.AddListener(onOneGacha);
    }

    void onOneGacha()
    {
        _gachaSystem.DoGacha();
    }

    void onTenGacha()
    {

    }


}
