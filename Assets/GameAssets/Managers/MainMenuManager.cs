using Assets.UnityFoundation.UI.Menus.MultiplayerLobbyMenu;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class MainMenuManager : LobbyMenuManager
{
    protected override void OnStart()
    {
        NetworkManager.Instance.OnConnectedToMasterEvent
            += () => landingPanel.EnableButtons();

        NetworkManager.Instance.OnJoinRoomEvent += ClientConnectedHandle;

        NetworkManager.Instance.OnLeaveRoomEvent += ClientDisconnectedHandle;
    }

    private void ClientDisconnectedHandle()
    {
        OpenLandingPanel();
        UpdatePartyPlayerInfo();
    }

    private void ClientConnectedHandle()
    {
        OpenLobbyWattingRoom();
        UpdatePartyPlayerInfo();
    }

    private void UpdatePartyPlayerInfo()
    {
        var playersNames = new List<string>();
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            playersNames.Add(player.NickName);
        }

        InvokePlayerInfoUpdated(playersNames, PhotonNetwork.IsMasterClient);
    }

    protected override void OnSetPlayerName(string playerName)
    {
        NetworkManager.Instance.SetPlayerName(playerName);
    }

    protected override void OnHostLobby(string roomName)
    {
        NetworkManager.Instance.CreateRoom(roomName);
    }

    protected override void OnJoinLobby(string address)
    {
        NetworkManager.Instance.JoinRoom(address);
    }

    protected override void OnLeaveLobby()
    {
        NetworkManager.Instance.LeaveRoom();
    }

    protected override void OnStartGame()
    {
        NetworkManager.Instance.StartGame();
    }
}
