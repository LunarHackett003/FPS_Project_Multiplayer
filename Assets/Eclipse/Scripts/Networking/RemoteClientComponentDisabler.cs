using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RemoteClientComponentDisabler : NetworkBehaviour
{

    [SerializeField] MonoBehaviour[] disableOnOwner, disableOnRemote;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            foreach (var component in disableOnRemote)
            {
                component.enabled = false;
            }
        }
        else
        {
            foreach(var component in disableOnOwner)
            {
                component.enabled = false;
            }
        }
        
    }
}
