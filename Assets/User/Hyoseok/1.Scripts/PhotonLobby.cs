using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;
using System.Collections;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public Button refreshButton;
    public TextMeshProUGUI statusText;
    public GameObject roomPrefab;
    public Transform roomListParent;
    private List<GameObject> roomButtons = new List<GameObject>();

    void Start()
    {
        refreshButton.onClick.AddListener(RefreshRoomList);
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
        if (roomListParent == null || roomPrefab == null)
        {
            //Debug.LogError("❌ roomListParent 또는 roomPrefab이 null입니다! Inspector에서 설정하세요.");
            return;
        }

        //  기존 방 목록 UI 초기화 (중복 추가 방지)
        foreach (GameObject button in roomButtons)
        {
            Destroy(button);
        }
        roomButtons.Clear();

        //  새로운 방 목록 추가
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList || room.PlayerCount == 0) // 삭제된 방 제거
            {
                //Debug.Log($"삭제된 방: {room.Name}");
                continue;
            }

            GameObject roomButton = Instantiate(roomPrefab, roomListParent);
            RoomButton roomButtonScript = roomButton.GetComponent<RoomButton>();

            if (roomButtonScript != null)
            {
                roomButtonScript.Setup(room);
            }
            else
            {
                //Debug.LogError("RoomButton 스크립트가 RoomButtonPrefab에 추가되지 않았습니다!");
            }

            roomButtons.Add(roomButton);
            //Debug.Log($"새로 추가된 방: {room.Name}");
        }

        //Debug.Log($"현재 UI에 표시된 방 개수: {roomButtons.Count}");
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
            //Debug.LogError(" CreateRoom() 호출 실패 - 현재 로비에 있지 않음!");
            return;
        }

        string roomName = "Room_" + Random.Range(1000, 9999);
        //Debug.Log($" 방 생성 시도: {roomName} (GameMode: {gameMode})");

        RoomOptions options = new RoomOptions()
        {
            MaxPlayers = (byte)(gameMode * 2),
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "GameMode", gameMode },
            { "HostName", PhotonNetwork.NickName }, //  방장 닉네임 저장
             
        },
            CustomRoomPropertiesForLobby = new string[] { "GameMode", "HostName" } // 로비에서 표시할 정보
        };

        //Debug.Log($"방 생성 데이터: RoomName={roomName}, HostName={PhotonNetwork.NickName}, GameMode={gameMode}");

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
        //Debug.Log("방에서 퇴장했습니다. 로비로 돌아갑니다.");

        //  기존 방 목록 정리
        foreach (GameObject button in roomButtons)
        {
            Destroy(button);
        }
        roomButtons.Clear();

        //  메인 UI 전환
        UIManager.instance.ShowMainUI();

        //  상태 메시지 업데이트
        statusText.text = "Left room. Back in Lobby.";

        //  로비 재입장
        PhotonNetwork.JoinLobby();
    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //Debug.LogError($"방 생성 실패! 코드: {returnCode}, 메시지: {message}");
    }


    public void RefreshRoomList()
    {
        if (PhotonNetwork.IsConnectedAndReady) //  연결 상태 확인
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
                StartCoroutine(RejoinLobby());
            }
            else
            {
                PhotonNetwork.JoinLobby();
            }
        }
        else
        {
            //Debug.LogError(" Photon이 아직 연결되지 않음. JoinLobby() 호출 불가능!");
        }
    }


    private IEnumerator RejoinLobby()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady); //  Photon 연결될 때까지 대기
        PhotonNetwork.JoinLobby();
    }

}
