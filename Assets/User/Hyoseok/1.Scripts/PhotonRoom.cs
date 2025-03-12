using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.Collections;
using Photon.Pun.Demo.PunBasics;
using System.Collections.Generic;


public class PhotonRoom : MonoBehaviourPunCallbacks
{
    
    public Transform redTeamPanel;
    public Transform blueTeamPanel;
    public GameObject playerPrefab;

    public Button switchTeamButton;
    public Button readyButton;
    public Button startGameButton;
    private bool isReady = false;
    private List<int> usedProfileIndices = new List<int>(); //중복프로필확인용

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
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

        //  나갔다가 다시 들어오면 Ready 상태를 false로 초기화
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Ready", false } });

        UpdateTeamUI();

        //  Ready 버튼 UI 텍스트 초기화
        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(ToggleReady);

            TextMeshProUGUI buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Ready"; //  버튼 UI 초기화
            }
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
        // ✅ 프로필 강제 초기화
        PlayerPrefab.InitializeProfiles();

        int redTeamCount = PhotonNetwork.PlayerList.Count(p => p.CustomProperties.ContainsKey("Team") && (string)p.CustomProperties["Team"] == "Red");
        int blueTeamCount = PhotonNetwork.PlayerList.Count(p => p.CustomProperties.ContainsKey("Team") && (string)p.CustomProperties["Team"] == "Blue");

        string assignedTeam = (redTeamCount <= blueTeamCount) ? "Red" : "Blue"; // ✅ 팀 자동 배정

        int assignedProfileIndex = GetUniqueProfileIndex(); // ✅ 중복 없는 프로필 선택

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
    {
        { "Team", assignedTeam },
        { "Ready", false }, // ✅ 입장할 때 무조건 Ready 상태 초기화
        { "ProfileImageIndex", assignedProfileIndex }
    };

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        Debug.Log($" {PhotonNetwork.LocalPlayer.NickName}님이 {assignedTeam} 팀에 배정되었습니다! 프로필 이미지 ID: {assignedProfileIndex}");
    }

    int GetUniqueProfileIndex()
    {
        PlayerPrefab.InitializeProfiles(); // ✅ 프로필 초기화 확인

        if (PlayerPrefab.ProfileCount == 0) return 0;

        HashSet<int> usedProfileIndices = new HashSet<int>(
            PhotonNetwork.PlayerList
            .Where(p => p.CustomProperties.ContainsKey("ProfileImageIndex"))
            .Select(p => (int)p.CustomProperties["ProfileImageIndex"])
        );

        List<int> availableProfiles = Enumerable.Range(0, PlayerPrefab.ProfileCount)
                                                .Where(index => !usedProfileIndices.Contains(index))
                                                .ToList();

        if (availableProfiles.Count == 0)
        {
            Debug.LogWarning("⚠️ 모든 프로필이 사용되었습니다. 중복 허용!");
            return Random.Range(0, PlayerPrefab.ProfileCount);
        }

        int selectedProfile = availableProfiles[Random.Range(0, availableProfiles.Count)];
        return selectedProfile;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($" {newPlayer.NickName}님이 방에 입장했습니다!");
        UpdateTeamUI();
    }


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Team") || changedProps.ContainsKey("Ready") || changedProps.ContainsKey("ProfileImageIndex"))
        {
            Debug.Log($"OnPlayerPropertiesUpdate() 호출됨 - {targetPlayer.NickName} 속성 변경 감지!");

            UpdateTeamUI(); //  UI 업데이트하여 모든 클라이언트에서 같은 프로필을 보이게 함
            CheckAllReady();
        }
    }



    void UpdateTeamUI()
    {
        foreach (Transform child in redTeamPanel) Destroy(child.gameObject);
        foreach (Transform child in blueTeamPanel) Destroy(child.gameObject);

        List<Player> sortedPlayers = PhotonNetwork.PlayerList
            .OrderBy(p => p.CustomProperties.ContainsKey("Ready") ? (bool)p.CustomProperties["Ready"] : false) // ✅ Ready 상태에 따라 정렬
            .ThenBy(p => p.CustomProperties.ContainsKey("Team") && (string)p.CustomProperties["Team"] == "Red" ? 0 : 1) // ✅ 레드팀 우선 정렬
            .ToList();

        foreach (Player player in sortedPlayers)
        {
            string team = player.CustomProperties.ContainsKey("Team") ? (string)player.CustomProperties["Team"] : "Red";
            bool isMaster = player.IsMasterClient;

            GameObject playerUI = Instantiate(Resources.Load<GameObject>("PlayerPrefab"));
            PlayerPrefab playerPrefabScript = playerUI.GetComponent<PlayerPrefab>();

            if (playerPrefabScript != null)
            {
                playerPrefabScript.Setup(player);
            }
            else
            {
                Debug.LogError(" PlayerPrefab 스크립트가 PlayerPrefab 오브젝트에 추가되지 않았습니다.");
            }

            if (team == "Red")
            {
                playerUI.transform.SetParent(redTeamPanel, false);
            }
            else
            {
                playerUI.transform.SetParent(blueTeamPanel, false);
            }

            //// ✅ 방장 표시
            //if (isMaster)
            //{
            //    playerPrefabScript.playerNameText.text = "[Master] " + player.NickName;
            //}
        }
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

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) 
        
     
            return;
        Debug.Log($" 현재 방의 플레이어 수: {PhotonNetwork.PlayerList.Length}");
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log($"👤 씬 이동 전 플레이어 - 닉네임: {player.NickName}, ID: {player.ActorNumber}, 방장 여부: {player.IsMasterClient}");
        }

        SavePlayerData(); //  씬 변경 전에 플레이어 정보 저장
        Debug.Log(" GameScene으로 씬 이동 중...");
        PhotonNetwork.LoadLevel("GameScene"); //  모든 플레이어가 동시에 이동
    }

    //유저정보 저장 (팀, 프로필 , 닉네임)
    void SavePlayerData()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            ExitGames.Client.Photon.Hashtable playerData = new ExitGames.Client.Photon.Hashtable
        {
            { "Team", player.CustomProperties["Team"] },
            { "ProfileImageIndex", player.CustomProperties["ProfileImageIndex"] },
            { "NickName", player.NickName } // 닉네임 저장
        };

            player.SetCustomProperties(playerData);
        }

        Debug.Log("✅ 모든 플레이어의 정보를 저장했습니다.");
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

        // ✅ 방장이 나갈 경우에만 방을 삭제
        if (otherPlayer.IsMasterClient)
        {
            Debug.Log(" 방장이 떠났습니다. 방을 삭제하고 모든 플레이어를 강제 퇴장합니다.");
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (!player.IsLocal)
                {
                    Debug.Log("다른 유저를 강제 퇴장시킵니다.");
                    PhotonNetwork.CloseConnection(player);
                }
            }

            StartCoroutine(DestroyRoomAndExit());
        }
        else
        {
            Debug.Log(" 방장이 아닌 플레이어가 나갔으므로 방을 유지합니다.");
        }

        UpdateTeamUI();
        CheckAllReady();
    }

    private IEnumerator DestroyRoomAndExit()
    {
        yield return new WaitForSeconds(0.5f); //  딜레이 후 퇴장 (Photon 네트워크 안정성 보장)
 ;
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom(); //  방장이 방에서 나가기
        }
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"방장이 변경되었습니다. 새로운 방장: {newMasterClient.NickName}");

        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        UpdateTeamUI();
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