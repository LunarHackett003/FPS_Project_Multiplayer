using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    ManagedBehaviour[] managedBehaviours;
    ManagedBehaviour currentBehaviour;
    public static UpdateManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        managedBehaviours = FindObjectsOfType<ManagedBehaviour>();
        for (int i = 0; i < managedBehaviours.Length; i++)
        {
            currentBehaviour = managedBehaviours[i];
            if(currentBehaviour != null && currentBehaviour.enabled)
            {
                currentBehaviour.ManagedPreUpdate();
                currentBehaviour.ManagedUpdate();
                currentBehaviour.ManagedPostUpdate();
            }
        }
    }
    private void FixedUpdate()
    {
        managedBehaviours = FindObjectsOfType<ManagedBehaviour>();
        for (int i = 0; i < managedBehaviours.Length; i++)
        {
            currentBehaviour = managedBehaviours[i];
            if (currentBehaviour != null && currentBehaviour.enabled)
            {
                currentBehaviour.ManagedFixedUpdate();
                currentBehaviour.ManagedLateFixedUpdate();
            }
        }
    }
    private void LateUpdate()
    {
        managedBehaviours = FindObjectsOfType<ManagedBehaviour>();
        for (int i = 0; i < managedBehaviours.Length; i++)
        {
            currentBehaviour = managedBehaviours[i];
            if (currentBehaviour != null && currentBehaviour.enabled)
            {
                currentBehaviour.ManagedLateUpdate();
            }
        }
    }
}
