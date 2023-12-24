using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OwnerBasedLayerSetter : NetworkBehaviour
{
    public string ownerLayer, remoteLayer;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        gameObject.layer = LayerMask.NameToLayer(IsOwner ? ownerLayer : remoteLayer);
    }
}
