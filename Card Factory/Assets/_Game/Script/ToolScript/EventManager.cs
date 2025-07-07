
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public static class EventManager
{
    public static UnityEvent<int,int> OnCapitalChange = new UnityEvent<int,int>();
    public static UnityEvent OnQueueCountChange = new UnityEvent();
    public static UnityEvent<int> OnCoinchange = new UnityEvent<int>();
    public static UnityEvent OnBoosterCountChange = new UnityEvent();
    public static UnityEvent onHeartChange = new UnityEvent();
    public static UnityEvent OnLevelComplete = new UnityEvent();
}
