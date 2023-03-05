using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace NetworkScripts
{
    public class LobbyManager : NetworkBehaviour
    {
        string lobbyName = "test";
        public int MaxPlayers = 1;
        [SerializeField] TMP_Text CreateLobbyNameTextIP;
        [SerializeField] TMP_InputField JoinLobbyCodeTextIP;
        [SerializeField] TMP_Text LobbyCodeText;
        [SerializeField] TMP_InputField PlayerNameText;
        [SerializeField] GameObject StartGameButton;
        RelayHostData hostData;
        RelayJoinData joinData;
        string playerName;
        async void Start()
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in" + AuthenticationService.Instance.PlayerId);
            };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public async void CreateLobby()
        {
            try
            {
                Allocation allocation = await Relay.Instance.CreateAllocationAsync(3);
                hostData = new RelayHostData
                {
                    Key = allocation.Key,
                    Port = (ushort)allocation.RelayServer.Port,
                    AllocationID = allocation.AllocationId,
                    AllocationIDBytes = allocation.AllocationIdBytes,
                    ConnectionData = allocation.ConnectionData,
                    IPv4Address = allocation.RelayServer.IpV4
                };
                hostData.JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

                lobbyName = CreateLobbyNameTextIP.text;
                GetPlayerName();
                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions { 
                    IsPrivate = true, 
                    Player = GetPlayer(),
                    Data = new Dictionary<string, DataObject>()
                    {
                        {"joinCode", new DataObject(DataObject.VisibilityOptions.Member, hostData.JoinCode)}
                    }
                    
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MaxPlayers, createLobbyOptions);
            
                Debug.Log("created lobby" + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
                LobbyCodeText.text = lobby.LobbyCode;
                StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
                
                Debug.Log(playerName);
                StartGameButton.SetActive(true);

                NetworkManager.GetComponent<UnityTransport>().SetRelayServerData
                (
                    hostData.IPv4Address,
                    hostData.Port,
                    hostData.AllocationIDBytes,
                    hostData.Key,
                    hostData.ConnectionData
                );
                NetworkManager.StartHost();

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                throw;
            }
            
        }

        public void StartGame()
        {
            if (!NetworkManager.IsHost) return;
            NetworkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
        }

        public async void JoinLobby()
        {
            if (string.IsNullOrEmpty(JoinLobbyCodeTextIP.text)) return;
            try
            {
                GetPlayerName();
                JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
                {
                    Player = GetPlayer()
                };
                Debug.Log(JoinLobbyCodeTextIP.text);
                string lobbyCode = JoinLobbyCodeTextIP.text;
                Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
                Debug.Log("Joined lobby with " + lobbyCode);
                Debug.Log(playerName);
                
                string joinCode = lobby.Data["joinCode"].Value;
                Debug.Log("Received code: " + joinCode);

                JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

                joinData = new RelayJoinData
                {
                    Key = allocation.Key,
                    Port = (ushort)allocation.RelayServer.Port,
                    AllocationID = allocation.AllocationId,
                    AllocationIDBytes = allocation.AllocationIdBytes,
                    ConnectionData = allocation.ConnectionData,
                    HostConnectionData = allocation.HostConnectionData,
                    IPv4Address = allocation.RelayServer.IpV4
                };
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData
                (
                    joinData.IPv4Address,
                    joinData.Port,
                    joinData.AllocationIDBytes,
                    joinData.Key,
                    joinData.ConnectionData,
                    joinData.HostConnectionData
                );
                NetworkManager.StartClient();

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                throw;
            }
        }

        void GetPlayerName()
        {
            playerName = PlayerNameText.text + "#" + Random.Range(0, 99);
        }

        Player GetPlayer()
        {
            return new Player
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                }
            };
        }
        
        IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            var delay = new WaitForSecondsRealtime(waitTimeSeconds);
            while (true)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
                Debug.Log("Lobby Heartbeat");
                yield return delay;
            }
        }

        public async void ListLobbies()
        {
            try
            {
                QueryResponse queryResponse =  await Lobbies.Instance.QueryLobbiesAsync();
            
                Debug.Log("lobbies: " + queryResponse.Results.Count);

                foreach (Lobby lobby in queryResponse.Results)
                {
                    Debug.Log(lobby.Name + lobby.MaxPlayers);
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                throw;
            }
        }
        
        public struct RelayHostData
        {
            public string JoinCode;
            public string IPv4Address;
            public ushort Port;
            public Guid AllocationID;
            public byte[] AllocationIDBytes;
            public byte[] ConnectionData;
            public byte[] Key;
        }
        
        public struct RelayJoinData
        {
            public string JoinCode;
            public string IPv4Address;
            public ushort Port;
            public Guid AllocationID;
            public byte[] AllocationIDBytes;
            public byte[] ConnectionData;
            public byte[] HostConnectionData;
            public byte[] Key;
        }

        
    }
}

