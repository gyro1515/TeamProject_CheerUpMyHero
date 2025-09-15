using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedGameManager : SingletonMono<SpeedGameManager>
{
    public enum SpeedState { X1 = 1, X2 = 2, X3 = 3 }

    public SpeedState CurrentSpeed { get; private set; } = SpeedState.X1;

    private bool isUnlocked = false;

    protected override void Awake()
    {
        base.Awake();

        // 해금 여부 불러오기 저장이 아직 없으므로 기본값은 False로 고정
        isUnlocked = false;
        ApplySpeed(CurrentSpeed);
    }

    public void UnlockSpeed()
    {
        isUnlocked = true;
        Debug.Log("배속 해금됨");
    }

    public void ToggleSpeed()
    {
        if (!isUnlocked)
        {
            Debug.Log("배속 해금 안됨");
            return; 
        }

        switch (CurrentSpeed)
        {
            case SpeedState.X1:
                SetSpeed(SpeedState.X2);
                break;
            case SpeedState.X2:
                SetSpeed(SpeedState.X3);
                break;
            case SpeedState.X3:
                SetSpeed(SpeedState.X1);
                break;
        }
    }

    public void SetSpeed(SpeedState speed)
    {
        CurrentSpeed = speed;
        ApplySpeed(speed);
    }

    private void ApplySpeed(SpeedState speed)
    {
        Time.timeScale = (int)speed;
    }
}
