using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationData : SingletonMono<AnimationData>, IAnimationData
{
    [SerializeField] private string baseParameterName = "Idle";
    [SerializeField] private string attackParameterName = "Attack";
    [SerializeField] private string hitBackParameterName = "HitBack";
    [SerializeField] private string getUpParameterName = "isGetUp";
    [SerializeField] private string dieParameterName = "Die";
    [SerializeField] private string speedParameterName = "Speed";

    public int BaseParameterHash {  get; private set; }
    public int AttackParameterHash { get; private set; }
    public int HitBackParameterHash { get; private set; }
    public int GetUpParameterHash {  get; private set; }
    public int DieParameterHash { get; private set; }
    public int SpeedParameterHash {  get; private set; }
    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }
    public void Initialize()
    {
        BaseParameterHash = Animator.StringToHash(baseParameterName);
        AttackParameterHash = Animator.StringToHash(attackParameterName);
        HitBackParameterHash = Animator.StringToHash(hitBackParameterName);
        GetUpParameterHash = Animator.StringToHash(getUpParameterName);
        DieParameterHash = Animator.StringToHash(dieParameterName);
        SpeedParameterHash = Animator.StringToHash(speedParameterName);
    }
}
