using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
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
                lobbyName = CreateLobbyNameTextIP.text;
                GetPlayerName();
                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions { 
                    IsPrivate = true, 
                    Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    //{KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0")}
                } 
                };
                
                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, MaxPlayers, createLobbyOptions);
            
                Debug.Log("created lobby" + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
                LobbyCodeText.text = lobby.LobbyCode;
                Debug.Log(playerName);
                NetworkManager.StartHost();
                StartGameButton.SetActive(true);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                throw;
            }
            
        }

        public async void StartGame()
        {
            if (!NetworkManager.IsHost) return;
            //NetworkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);

            try
            {
                string relayCode = await CreateRelay();
                
                //Data = new Dictionary<string, DataObject>
                //{
                //    {KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode)}
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

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
                await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
                Debug.Log("Joined lobby with " + lobbyCode);
                Debug.Log(playerName);
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

        public async Task<string> CreateRelay()
        {
            try
            {
                Allocation allocation =  await RelayService.Instance.CreateAllocationAsync(3);

                string joinCode =  await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
                
                NetworkManager.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.StartHost();

                return joinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                throw;
            }
        }

        async void JoinRelay()
        {
            try
            {
                string joinCode = "";
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                
                RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
                
                NetworkManager.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                throw;
            }
        }
        
    }
}

