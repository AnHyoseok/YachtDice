using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.Collections;


public class PhotonRoom : MonoBehaviourPunCallbacks
{
    public Transform redTeamPanel;
    public Transform blueTeamPanel;
    public GameObject playerPrefab;

    public Button switchTeamButton;
    public Button readyButton;
    public Button startGameButton;
    private bool isReady = false;

    void Start()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null)  //  방에 있을 때만 실행
        {
            Debug.Log($" {PhotonNetwork.LocalPlayer.NickName}님이 방에 입장했습니다! 팀 배정 시작.");
            AssignTeam();  // 방에 입장하면 즉시 팀 배정
            UpdateTeamUI();  // 팀 UI 즉시 갱신

            readyButton.onClick.AddListener(ToggleReady);
            switchTeamButton.onClick.AddListener(SwitchTeam);
            startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            startGameButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.Log(" 방에 입장하지 않았으므로 AssignTeam() 실행하지 않음.");
        }
    }


    public override void OnJoinedRoom()
    {
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName}님이 방에 입장했습니다!");

        AssignTeam();
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", false } });
        UpdateTeamUI();

        //  Ready 버튼 이벤트 추가 (중복 방지)
        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(ToggleReady);
            Debug.Log(" Ready 버튼 이벤트 추가됨.");
        }
        else
        {
            Debug.LogError(" Ready 버튼이 null입니다! Inspector에서 연결하세요.");
        }

        UIManager.instance.ShowTeamUI();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log(" 로비에 입장했습니다. 방 목록을 불러옵니다.");
        UIManager.instance.ShowMainUI(); // UI 변경
    }

    void AssignTeam()
    {
        // 현재 방에 들어온 순서에 따라 Red → Blue 번갈아 배정
        int playerIndex = PhotonNetwork.PlayerList.Length;  // 방에 입장한 순서

        string assignedTeam = (playerIndex % 2 == 1) ? "Red" : "Blue";  //

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Team", assignedTeam }, { "Ready", false } });

        Debug.Log($" {PhotonNetwork.LocalPlayer.NickName}님이 {assignedTeam} 팀에 배정되었습니다!");
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($" {newPlayer.NickName}님이 방에 입장했습니다!");
        UpdateTeamUI();
    }


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Team") || changedProps.ContainsKey("Ready"))
        {
            Debug.Log($" OnPlayerPropertiesUpdate() 호출됨 - {targetPlayer.NickName}의 속성 변경 감지!");
            Debug.Log($" 변경된 속성: {string.Join(", ", changedProps.Keys)}");

            UpdateTeamUI();
            CheckAllReady();
        }
    }

    void UpdateTeamUI()
    {
        foreach (Transform child in redTeamPanel) Destroy(child.gameObject);
        foreach (Transform child in blueTeamPanel) Destroy(child.gameObject);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string team = player.CustomProperties.ContainsKey("Team") ? (string)player.CustomProperties["Team"] : "Red";
            bool isReady = player.CustomProperties.ContainsKey("Ready") ? (bool)player.CustomProperties["Ready"] : false;
            bool isMaster = player.IsMasterClient;

            //  플레이어 UI 프리팹 생성
            GameObject playerUI = Instantiate(Resources.Load<GameObject>("PlayerPrefab"));
            PlayerPrefab playerPrefabScript = playerUI.GetComponent<PlayerPrefab>();

            if (playerPrefabScript != null)
            {
                playerPrefabScript.Setup(player);  //  플레이어 UI 업데이트
            }
            else
            {
                Debug.LogError(" PlayerPrefab 스크립트가 PlayerPrefab 오브젝트에 추가되지 않았습니다.");
            }

            // 팀에 따라 UI 배치
            if (team == "Red")
            {
                playerUI.transform.SetParent(redTeamPanel, false);
            }
            else
            {
                playerUI.transform.SetParent(blueTeamPanel, false);
            }
        }

        //  Ready 상태 체크해서 Start 버튼 활성화 여부 갱신
        CheckAllReady();
    }



    public void ToggleReady()
    {
        if (readyButton == null)
        {
            Debug.LogError(" readyButton이 null 상태입니다!");
            return;
        }

        isReady = !isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", isReady } });

        TextMeshProUGUI buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText == null)
        {
            Debug.LogError(" readyButton에 TextMeshProUGUI가 없습니다!");
            return;
        }

        buttonText.text = isReady ? "Unready" : "Ready";
    }

    void CheckAllReady()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        bool allReady = true;
        int redTeamCount = 0;
        int blueTeamCount = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            //  모든 플레이어가 "Ready" 상태인지 확인
            bool isPlayerReady = player.CustomProperties.ContainsKey("Ready") && (bool)player.CustomProperties["Ready"];
            Debug.Log($" {player.NickName} Ready 상태: {isPlayerReady}");

            if (!isPlayerReady)
            {
                allReady = false;
            }

            //  팀 인원 카운트
            if (player.CustomProperties.ContainsKey("Team"))
            {
                string team = (string)player.CustomProperties["Team"];
                if (team == "Red") redTeamCount++;
                else if (team == "Blue") blueTeamCount++;
            }
        }

        //  1:1 or 2:2 밸런스 체크
        int totalPlayers = PhotonNetwork.CurrentRoom.MaxPlayers; // 1:1 → 2명, 2:2 → 4명
        bool isBalanced = (redTeamCount == blueTeamCount) && (redTeamCount + blueTeamCount == totalPlayers);

        Debug.Log($" 모든 플레이어 준비 상태: {allReady}, 팀 밸런스 정상: {isBalanced} (Red: {redTeamCount}, Blue: {blueTeamCount})");

        if (startGameButton == null)
        {
            Debug.LogError(" startGameButton이 null입니다! Inspector에서 연결하세요.");
            return;
        }

        // 모든 조건이 충족될 때만 게임 시작 버튼 활성화
        bool canStartGame = allReady && isBalanced;
        startGameButton.interactable = canStartGame;
        startGameButton.gameObject.SetActive(true);

        Debug.Log($" startGameButton 상태 - 활성화: {startGameButton.gameObject.activeSelf}, 상호작용 가능: {startGameButton.interactable}");
    }

    void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene"); // 실제 게임 씬으로 전환
        }
    }

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log(" 방을 나갑니다...");
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            Debug.LogWarning(" 방에 있지 않습니다! LeaveRoom() 호출이 필요 없습니다.");
        }
    }



    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($" {otherPlayer.NickName}님이 방을 떠났습니다.");

        //  방장이 나갔는지 확인
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log(" 방장이 떠났습니다. 방을 삭제하고 모든 플레이어를 강제 퇴장합니다.");
            PhotonNetwork.CurrentRoom.IsOpen = false; //  방을 닫아서 새로운 입장 방지
            PhotonNetwork.CurrentRoom.IsVisible = false; //  방 목록에서 숨김

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!player.IsLocal) //  방장이 아닌 다른 플레이어들을 강제 퇴장
                {
                    Debug.Log("다른유저를 쫒아냅니다");
                    PhotonNetwork.CloseConnection(player);
                }
            }

            StartCoroutine(DestroyRoomAndExit()); //  비동기 처리로 방을 삭제 후 나가기
        }

        //  UI 업데이트
        UpdateTeamUI();
        CheckAllReady();
    }

    private IEnumerator DestroyRoomAndExit()
    {
        yield return new WaitForSeconds(0.5f); //  딜레이 후 퇴장 (Photon 네트워크 안정성 보장)
        Debug.Log("dddddddd");
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(); //  방장이 방에서 나가기
        }
    }


    public void SwitchTeam()
    {
        if (!PhotonNetwork.InRoom) return;

        string currentTeam = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team")
            ? (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"]
            : "Red";  // 기본값

        string newTeam = (currentTeam == "Red") ? "Blue" : "Red";

        Debug.Log($"🔄 {PhotonNetwork.LocalPlayer.NickName}님이 {currentTeam} → {newTeam} 변경 시도 중...");

        // 팀 속성을 변경
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Team", newTeam } });

        //  UI 즉시 업데이트
        UpdateTeamUI();

    }

    public override void OnLeftRoom()
    {
        Debug.Log(" 방 나가기 성공! 로비로 재입장합니다...");
        UIManager.instance.ShowMainUI(); // UI 갱신

        PhotonNetwork.JoinLobby(); //  로비로 다시 입장 필수!
    }


}