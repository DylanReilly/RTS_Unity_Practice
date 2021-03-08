using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGamebutton = null;
    [SerializeField] private TMP_Text[] playerNameText = new TMP_Text[4];

    private void Start()
    {
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    private void OnDestroy()
    {
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;

    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startGamebutton.gameObject.SetActive(state);
        //leaveGamebutton.gameObject.SetActive(state);
    }

    public void StartGame()
    {
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();

    }

    private void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
        //leaveGamebutton.gameObject.SetActive(true);
    }

    private void ClientHandleInfoUpdated()
    {
        List<RTSPlayer> players = ((RTSNetworkManager)NetworkManager.singleton).Players;

        for (int i = 0; i < players.Count; i++)
        {
            playerNameText[i].text = players[i].getDisplayName();
        }

        for (int i = players.Count; i < playerNameText.Length; i++)
        {
            playerNameText[i].text = "Waiting for player...";
        }

        startGamebutton.interactable = players.Count > 1;
    }    

    public void LeaveLobby()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();

            SceneManager.LoadScene(0);
        }
    }
}
