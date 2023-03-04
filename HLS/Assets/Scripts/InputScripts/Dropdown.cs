using NetworkScripts;
using UnityEngine;

namespace InputScripts
{
    public class Dropdown : MonoBehaviour
    {
        LobbyManager lobbyManager;

        void Awake()
        {
            lobbyManager = FindObjectOfType<LobbyManager>();
        }

        public void DropDownUI(int index)
        {
            switch (index)
            {
                case 0: lobbyManager.MaxPlayers = 1; break;
                case 1: lobbyManager.MaxPlayers = 2; break;
                case 2: lobbyManager.MaxPlayers = 3; break;
                case 3: lobbyManager.MaxPlayers = 4; break;
            }
        }
    }
}
