using Photon.Pun;
using System;

public class NetworkManager : MonoBehaviourPunCallbacksSingleton<NetworkManager>
{
    public event Action OnConnectedToMasterEvent;
    public event Action OnJoinRoomEvent;
    public event Action OnLeaveRoomEvent;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public void CreateRoom(string roomName)
    {
        PhotonNetwork.CreateRoom(roomName);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        photonView.RPC(nameof(OnJoinedRoomEventInvoke), RpcTarget.All);
    }

    public override void OnConnectedToMaster() => OnConnectedToMasterEvent?.Invoke();

    public void SetPlayerName(string newPlayerName)
    {
        PhotonNetwork.NickName = newPlayerName;
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        OnLeaveRoomEvent?.Invoke();
    }

    public void StartGame()
    {
        photonView.RPC(nameof(ChangeScene), RpcTarget.All, "game_scene");
    }

    [PunRPC]
    private void OnJoinedRoomEventInvoke() => OnJoinRoomEvent?.Invoke();

    [PunRPC]
    public void ChangeScene(string sceneName)
        => ((PhotonSceneFader)PhotonSceneFader.Instance).FadeIn(sceneName);
}
