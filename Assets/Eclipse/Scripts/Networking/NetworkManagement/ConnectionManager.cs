using Eflatun.SceneReference;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Eclipse.Connections
{
    public class ConnectionManager : MonoBehaviour
    {
        public static ConnectionManager instance;
        public UnityTransport sp_transport, mp_transport;

        public List<SceneReference> maps;

        public Lobby privateLobby, currentMatchLobby;
        public Allocation currentMatchRelay;
        public JoinAllocation currentJoinAllocation;
        public TextMeshProUGUI joinCodeText;

        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartSinglePlayerGame()
        {
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = sp_transport;
            SceneReference chosenMap = maps[Random.Range(0, maps.Count - 1)];
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(chosenMap.Name, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        public void LeaveGame()
        {
            NetworkManager.Singleton.Shutdown();
        }
        public void HostButton()
        {
            HostGame();
        }
        public async void HostGame()
        {
            //Create lobby for match
            SceneReference chosenMap = maps[Random.Range(0, maps.Count - 1)];
            currentMatchRelay = await Relay.Instance.CreateAllocationAsync(12);
            string jc = await Relay.Instance.GetJoinCodeAsync(currentMatchRelay.AllocationId);
            CreateLobbyOptions clo = new()
            {
                Data = new()
                {
                    {"map", new(DataObject.VisibilityOptions.Public, chosenMap.Name) },
                    {"gamemode", new(DataObject.VisibilityOptions.Public, "debug") },
                    {"relaycode", new(DataObject.VisibilityOptions.Public, jc)}
                },
                IsPrivate = false
            };
            currentMatchLobby = await Lobbies.Instance.CreateLobbyAsync(StringHelpers.RandomString(20), 12, clo);
            //Create relay allocaton for match
            //assign relay for match
            joinCodeText.text = currentMatchLobby.LobbyCode;
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = mp_transport;
            mp_transport.SetHostRelayData(currentMatchRelay.RelayServer.IpV4,
                (ushort)currentMatchRelay.RelayServer.Port,
                currentMatchRelay.AllocationIdBytes,
                currentMatchRelay.Key,
                currentMatchRelay.ConnectionData,
                true);
            //Start host, change scene
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(chosenMap.Name, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        public void JoinWithCodeButton(string code)
        {
            JoinGameWithCode(code);
        }
        public async void JoinGameWithCode(string code)
        {
            currentMatchLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code);
            joinCodeText.text = currentMatchLobby.LobbyCode;
            Debug.Log(currentMatchLobby.Data["relaycode"]);
            currentJoinAllocation = await Relay.Instance.JoinAllocationAsync(currentMatchLobby.Data["relaycode"].Value);
            NetworkManager.Singleton.NetworkConfig.NetworkTransport = mp_transport;
            mp_transport.SetClientRelayData(currentJoinAllocation.RelayServer.IpV4,
                (ushort)currentJoinAllocation.RelayServer.Port,
                currentJoinAllocation.AllocationIdBytes,
                currentJoinAllocation.Key,
                currentJoinAllocation.ConnectionData,
                currentJoinAllocation.HostConnectionData,
                true);
            NetworkManager.Singleton.StartClient();
        }

    }
}