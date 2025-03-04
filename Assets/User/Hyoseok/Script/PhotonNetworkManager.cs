using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Photon 서버 연결
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon 서버에 연결됨!");
        PhotonNetwork.JoinLobby(); // 로비 참가
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 }; // 최대 4명
        PhotonNetwork.CreateRoom("TestRoom", roomOptions); // "TestRoom" 생성
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom("TestRoom"); // "TestRoom"에 참가
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방에 참가 성공!");
        PhotonNetwork.Instantiate("PlayerPrefab", Vector3.zero, Quaternion.identity);
    }
}
