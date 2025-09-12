using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedGameManager : SingletonMono<SpeedGameManager>
{
    public enum SpeedState { X1 = 1, X2 = 2, X3 = 3 }

    public SpeedState CurrentSpeed { get; private set; } = SpeedState.X1;

    protected override void Awake()
    {
        base.Awake();
        ApplySpeed(CurrentSpeed);
    }

    public void ToggleSpeed()
    {
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
