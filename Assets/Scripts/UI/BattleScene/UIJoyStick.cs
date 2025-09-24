using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIJoyStick : BaseUI
{
    [field: Header("조이스틱 캔버스 설정")]
    [field: SerializeField] public Canvas MainCanvas { get; private set; }
    [field: SerializeField] public GameObject JoyStickGO { get; private set; }
    [field: SerializeField] public GameObject JoyStickCenterGO { get; private set; }
}
