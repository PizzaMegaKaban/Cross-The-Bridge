using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager
{
    public static UnityEvent BlockStopClick = new UnityEvent();

    public static UnityEvent OnLevelFinished = new UnityEvent();
}
