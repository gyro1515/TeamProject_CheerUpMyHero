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
    [SerializeField] private string basicParameterName = "Blend Tree";
    [SerializeField] private string stopAttackParameterName = "StopAttack";

    public int BaseParameterHash {  get; private set; }
    public int AttackParameterHash { get; private set; }
    public int HitBackParameterHash { get; private set; }
    public int GetUpParameterHash {  get; private set; }
    public int DieParameterHash { get; private set; }
    public int SpeedParameterHash {  get; private set; }
    public int BasicParameterHash {  get; private set; }
    public int StopAttackParameterHash {  get; private set; }
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
        BasicParameterHash = Animator.StringToHash(basicParameterName);
        StopAttackParameterHash = Animator.StringToHash(stopAttackParameterName);

    }
}
