using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateClosedEye : MonoBehaviour
{
    private void OnDisable()
    {
        gameObject.SetActive(false);
    }
}
