using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerSliderScript : MonoBehaviour
{
    public Slider timerSlider;
    public float timeDuration = 5f;
    private float timeRemaining;
    private bool timerRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        timerSlider.maxValue = timeDuration;
        timerSlider.value = timeDuration;
        timeRemaining = timeDuration;
        timerRunning = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (timerRunning)
        {
            timeRemaining -= Time.deltaTime;
            timerSlider.value = timeRemaining;

            if (timeRemaining <= 0f)
            {
                timerRunning = false;
                timerSlider.value = 0f;
                Debug.Log("Timer ended!");
                // You can add any callback here
                Destroy(gameObject);
            }
        }
    }
}
