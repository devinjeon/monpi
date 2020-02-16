using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusTimeController : MonoBehaviour
{
    public UnityEngine.UI.Text timeText;
    private float time;
    private bool isStopped;

    void Update()
    {
        if (isStopped == false)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                StopCountDown();
                time = 0;
            }
            UpdateText();
        }
    }

    public float GetTimeLeft()
    {
        return time;
    }

    public void StartCountDown()
    {
        isStopped = false;
    }

    public void StopCountDown()
    {
        isStopped = true;
    }

    public void Reset(float t)
    {
        StopCountDown();
        time = t;
        UpdateText();
    }

    public void UpdateText()
    {
        timeText.text = time.ToString("0.00");
    }
}
