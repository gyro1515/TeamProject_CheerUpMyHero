using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveEye : MonoBehaviour
{
    private void OnEnable()
    {
        gameObject.SetActive(true);
    }
}
