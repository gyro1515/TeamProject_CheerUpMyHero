using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRarityIconArea : MonoBehaviour
{
    [SerializeField] List<GameObject> icons = new List<GameObject>();

    public void SetIconCnt(int rarity)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            if (i <= rarity)
                icons[i].SetActive(true);
            else
                icons[i].SetActive(false);
        }
    }
}
