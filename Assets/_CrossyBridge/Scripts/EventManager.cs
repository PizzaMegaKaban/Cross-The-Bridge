using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager
{
    public static UnityEvent BlockStopClick = new UnityEvent();

    public static UnityEvent OnLevelFinished = new UnityEvent();

    public static UnityEvent OnBlackPanelMissClick = new UnityEvent();

    public static UnityEvent OnRespawnPerform = new UnityEvent();

    public static UnityEvent OnSetPlayerOnPlane = new UnityEvent();

    public static UnityEvent<Vector3> OnSetCameraByPlayer = new UnityEvent<Vector3>();

    public static UnityEvent<int> OnPlaneTrigger = new UnityEvent<int>();
}
