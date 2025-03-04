using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Photon 서버 연결
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby(); // 로비에 참가
    }

    public void CreateRoom(string roomName)
    {
        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.Instantiate("PlayerPrefab", Vector3.zero, Quaternion.identity);
    }
}
