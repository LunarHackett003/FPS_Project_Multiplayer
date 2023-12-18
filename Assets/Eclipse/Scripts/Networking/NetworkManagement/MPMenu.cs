using Eclipse.Connections;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MPMenu : MonoBehaviour
{
    public TMP_InputField joincodeInput;
    public void JoinGameWithCode()
    {
        if (string.IsNullOrEmpty(joincodeInput.text))
            return;
        ConnectionManager.instance.JoinGameWithCode(joincodeInput.text);
    }
    public void HostGame()
    {
        ConnectionManager.instance.HostGame();
    }

}
