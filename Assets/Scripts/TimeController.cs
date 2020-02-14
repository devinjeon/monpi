using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public UnityEngine.UI.Text timeText;
    public float timeStart;
    private float timeLeft;
    private bool isStopped;

    void Start()
    {
        Reset();
    }

    void Update()
    {
        if (isStopped == false)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                StopCountDown();
                timeLeft = 0;
            }
            UpdateText();
        }
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

    public void StartCountDown()
    {
        isStopped = false;
    }

    public void StopCountDown()
    {
        isStopped = true;
    }

    public void SetTimeCount(float second)
    {
        timeStart = second;
    }

    public void Reset()
    {
        StopCountDown();
        timeLeft = timeStart;
        UpdateText();
    }

    public void UpdateText()
    {
        timeText.text = timeLeft.ToString("0.00");
    }
}
