using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTimerScript : MonoBehaviour
{
    public GameManager GameManager;
    public Slider TimerSlider;

    private int _targetValue = 1;
    private float _smoothSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.OnPlaneTrigger.AddListener(OnBlockEntered);

        TimerSlider.minValue = 0f;
        TimerSlider.maxValue = GameManager.MaxProgressBlock - 1;
        TimerSlider.value = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        // Плавное обновление значения Slider
        //if (Mathf.Abs(TimerSlider.value - _targetValue) > 0.001f)
        //{
        //    TimerSlider.value = Mathf.Lerp(TimerSlider.value, _targetValue, Time.deltaTime * _smoothSpeed);
        //}

        TimerSlider.value = Mathf.Lerp(TimerSlider.value, _targetValue, Time.deltaTime * _smoothSpeed);
    }

    void OnDestroy()
    {
        EventManager.OnPlaneTrigger.RemoveListener(OnBlockEntered);
    }

    /// <summary>
    /// Вызывается при входе на новый блок.
    /// </summary>
    /// <param name="blockIndex">Индекс блока, начиная с 0.</param>
    public void OnBlockEntered(int blockIndex)
    {
        _targetValue = ++blockIndex;
    }
}