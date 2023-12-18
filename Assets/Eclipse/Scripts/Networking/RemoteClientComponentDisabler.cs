using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RemoteClientComponentDisabler : NetworkBehaviour
{

    [SerializeField] MonoBehaviour[] disableOnOwner, disableOnRemote;
    [SerializeField] GameObject[] go_OwnerDisable, go_RemoteDisable;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            foreach (var component in disableOnRemote)
            {
                component.enabled = false;
            }
            foreach (var item in go_RemoteDisable)
            {
                item.SetActive(false);
            }
        }
        else
        {
            foreach(var component in disableOnOwner)
            {
                component.enabled = false;
            }
            foreach(var item in go_OwnerDisable)
            {
                item.SetActive(false);
            }
        }
        
    }
}
