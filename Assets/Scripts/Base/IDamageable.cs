using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage);
    void TakeHeal(float amount);
    void Dead();
    bool IsDead();
}
