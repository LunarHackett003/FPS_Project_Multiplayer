using Eclipse.Connections;
using Eflatun.SceneReference;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MPMenuStarter : MonoBehaviour
{
    public GameObject errorPanelObject;
    public TextMeshProUGUI errorText;
    public SceneReference mpMenuScene;
    public async void TryLaunchMultiplayer()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            try
            {
                await UnityServices.InitializeAsync();

            }
            catch (ServicesInitializationException e)
            {
                ErrorLog($"Initialisation Exception - {e.Message}");
                Debug.LogException(e);
                return;
            }
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException e)
            {
                ErrorLog($"Sign-in Exception - {e.Message}");
                Debug.LogException(e);
                return;
            }
        }
        CreateLobbyOptions clo = new()
        {
            IsPrivate = true,
        };
        ConnectionManager.instance.privateLobby = await Lobbies.Instance.CreateLobbyAsync($"AuthenticationService.Instance.PlayerId Party", 12, clo);
        SceneManager.LoadScene(mpMenuScene.Name);
    }
    void ErrorLog(string errorMessage)
    {
        errorText.text = errorMessage;
        errorPanelObject.SetActive(true);
    }
}
