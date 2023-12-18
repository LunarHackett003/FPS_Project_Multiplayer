using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventOnStart : MonoBehaviour
{
    public UnityEvent events;
    public float delay;
    private void Start()
    {
        Invoke(nameof(InvokeEvents), delay);
    }
    void InvokeEvents()
    {
        events?.Invoke();
    }
}
