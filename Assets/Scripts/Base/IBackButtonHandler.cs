using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBackButtonHandler
{
    void OnBackPressed();
}
// 이벤트용 구조체, UI 스택에 추가할 때 사용
public struct AddUIStackEvent
{
    public IBackButtonHandler ui;
}
public struct RemoveUIStackEvent { }
