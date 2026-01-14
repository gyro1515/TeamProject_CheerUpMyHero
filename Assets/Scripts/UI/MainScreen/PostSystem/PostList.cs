using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostList : MonoBehaviour
{
    private Button _button;
    private PostBox postBox;

    private int listNum;
    
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text expireText;


    private void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void SetPostListText(int num, string title, string expire, bool isAlreadyReceived, PostBox postBox)
    {
        listNum = num;
        
        titleText.text = title;

        expireText.text = "만료: " + expire;

        this.postBox = postBox;

        if (isAlreadyReceived)
        {
            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.gray;
            }
        }

        _button = GetComponent<Button>();

        _button.onClick.AddListener(() => this.postBox.OpenPostContent(num));

    }

}
