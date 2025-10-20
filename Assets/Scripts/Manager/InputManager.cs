using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : SingletonMono<InputManager>
{
    public struct BackButtonPressedEvent { }
    PlayerInput inputActions;
    IEventPublisher<BackButtonPressedEvent> OnBackButtonPressedEventPub;
    protected override void Awake()
    {
        base.Awake();
        inputActions = new PlayerInput();
        inputActions.ReturnBtn.Enable();
        inputActions.ReturnBtn.Return.started += OnBack;
        OnBackButtonPressedEventPub = EventManager.GetPublisher<BackButtonPressedEvent>();
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (inputActions != null)
            inputActions.ReturnBtn.Return.started -= OnBack;
    }
    void OnBack(InputAction.CallbackContext context)
    {
        Debug.Log("뒤로가기");
        OnBackButtonPressedEventPub.Publish();
    }
}
