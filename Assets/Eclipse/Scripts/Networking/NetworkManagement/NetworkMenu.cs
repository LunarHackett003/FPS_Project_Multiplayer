using Eclipse.Connections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMenu : MonoBehaviour
{
    public void StartSingleplayer()
    {
        ConnectionManager.instance.StartSinglePlayerGame();
    }
    
    
}
