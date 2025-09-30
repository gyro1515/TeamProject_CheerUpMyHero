using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITimer : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;


    //시작 절대 시간
    private float startTime;

    //7분 30초
    private float totalTime = 450f;

    private float remainTime;

    private bool isTimeLimt = false;
    
    
    // Start is called before the first frame update
    void Start()
    {
        startTime = GameManager.Instance.StartTime;
        remainTime = totalTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (remainTime > 0)
        {
            //흐른 시간
            float spendTime = Time.time - startTime;
            remainTime = totalTime - spendTime;

            UpdateTimer();
        }
        else if (remainTime <= 0 && !isTimeLimt)
        {
            timerText.text = "00:00";
            isTimeLimt = true;
        }
 
    }

    void UpdateTimer()
    {
        int min = Mathf.FloorToInt(remainTime / 60f);
        int sec = Mathf.FloorToInt(remainTime % 60);
        if (sec >= 10)
            timerText.text = $"{min}:{sec}";
        else
            timerText.text = $"{min}:0{sec}";
    }
}
