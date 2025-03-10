using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI statusText;
    public GameObject roomPrefab;
    public Transform roomListParent;
    private List<GameObject> roomButtons = new List<GameObject>();

    void Start()
    {
        roomButtons = new List<GameObject>();  // 초기화 추가
        statusText.text = "Connecting to Photon...";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected! Joining Lobby...";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "Joined Lobby! Fetching Rooms...";
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (roomListParent == null)
        {
            Debug.LogError(" [ERROR] roomListParent가 null입니다! Inspector에서 설정하세요.");
            return;
        }

        if (roomPrefab == null)
        {
            Debug.LogError(" [ERROR] roomPrefab이 null입니다! Inspector에서 설정하세요.");
            return;
        }

        if (roomButtons == null)
        {
            Debug.LogError(" [ERROR] roomButtons 리스트가 초기화되지 않았습니다!");
            roomButtons = new List<GameObject>();  // 초기화
        }

        // 기존 방 목록 삭제
        foreach (GameObject button in roomButtons)
        {
            Destroy(button);
        }
        roomButtons.Clear();

        // 새로운 방 목록 생성
        foreach (RoomInfo room in roomList)
        {
            GameObject roomButton = Instantiate(roomPrefab, roomListParent);
            roomButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
            roomButton.GetComponent<Button>().onClick.AddListener(() => JoinRoom(room.Name));
            roomButtons.Add(roomButton);
        }
    }


    //  CreateRoom 버튼 클릭 시 모드 선택 UI 열기
    public void OnCreateRoomButtonClick()
    {
        UIManager.instance.ShowModeSelection();
    }

    //  1:1 방 생성
    public void CreateRoom_1v1()
    {
        UIManager.instance.HideModeSelection();
        CreateRoom(1);
    }

    //  2:2 방 생성
    public void CreateRoom_2v2()
    {
        UIManager.instance.HideModeSelection();
        CreateRoom(2);
    }

    //  방 생성 로직
    public void CreateRoom(int gameMode)
    {
        if (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InLobby)
        {
            Debug.LogError("🚨 CreateRoom() 호출 실패 - 현재 로비에 있지 않음!");
            return;
        }

        string roomName = "Room_" + Random.Range(1000, 9999);
        Debug.Log($"방 생성 시도: {roomName} (GameMode: {gameMode})");

        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = (byte)(gameMode * 2),
            CustomRoomProperties = new Hashtable() { { "GameMode", gameMode } },
            CustomRoomPropertiesForLobby = new string[] { "GameMode" }
        };

        PhotonNetwork.CreateRoom(roomName, options);
    }


    //  방 입장
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        statusText.text = "Joined Room: " + PhotonNetwork.CurrentRoom.Name;
        UIManager.instance.ShowTeamUI();  // 팀 UI 표시
    }

    public override void OnLeftRoom()
    {
        Debug.Log("방에서 퇴장했습니다. 로비로 돌아갑니다.");

        // 방 나가면 메인 로비 UI를 보여줌
        UIManager.instance.ShowMainUI();
        statusText.text = "Left room. Back in Lobby.";
        PhotonNetwork.JoinLobby();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 생성 실패! 코드: {returnCode}, 메시지: {message}");
    }

}
