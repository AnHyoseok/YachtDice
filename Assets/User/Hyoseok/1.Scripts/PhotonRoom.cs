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
    public RectTransform teamPanel;
    public GameObject playerPrefab;

    public Button switchTeamButton;
    public Button readyButton;
    public Button startGameButton;
    private string[] aiNames = {
    "Seraphina", "Valeria", "Isolde", "Selene", "Freya",
    "Lilith", "Athena", "Raven", "Valkyrie", "Nyx",
    "Celestia", "Morgana", "Artemis", "Elysia", "Nova",
    "Sigrid", "Astra", "Xanthe", "Zafira", "Draven"
};
    [SerializeField] private int aiProfileCount=2;

    private bool isReady = false;
    private List<int> usedProfileIndices = new List<int>(); //중복프로필확인용
    public static PhotonRoom instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
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

        //  1:1 또는 2:2 모드에 따라 TeamPanel 크기 변경
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameMode"))
        {
            int gameMode = (int)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"];
            SetTeamPanelSize(gameMode);
        }


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

    //모드 패널 크리 조절
    void SetTeamPanelSize(int gameMode)
    {
        if (teamPanel != null)
        {
            //  1:1 모드 → Left: 411, Right: 0
            //  2:2 모드 → Left: 158, Right: -158
            int leftValue = (gameMode == 1) ? 411 : 158;
            int rightValue = (gameMode == 1) ? 0 : -158;

            teamPanel.offsetMin = new Vector2(leftValue, teamPanel.offsetMin.y);
            teamPanel.offsetMax = new Vector2(rightValue, teamPanel.offsetMax.y);

            Debug.Log($" TeamPanel 크기 조정됨: Left={leftValue}, Right={rightValue}");
        }
    }


    public override void OnJoinedLobby()
    {
        Debug.Log(" 로비에 입장했습니다. 방 목록을 불러옵니다.");
        UIManager.instance.ShowMainUI(); // UI 변경
    }

    void AssignTeam()
    {
        //  프로필 강제 초기화
        PlayerPrefab.InitializeProfiles();

        //  플레이어가 이전에 있던 방 정보 확인
        string lastRoomName = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("LastRoom")
            ? (string)PhotonNetwork.LocalPlayer.CustomProperties["LastRoom"]
            : null;

        string assignedTeam = null;

        //  같은 방에 재입장한 경우, 이전 팀 유지
        if (lastRoomName != null && lastRoomName == PhotonNetwork.CurrentRoom.Name)
        {
            assignedTeam = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team")
                ? (string)PhotonNetwork.LocalPlayer.CustomProperties["Team"]
                : null;
        }

        //  다른 방이면 새로운 팀 배정
        if (assignedTeam == null)
        {
            int redTeamCount = PhotonNetwork.PlayerList.Count(p => p.CustomProperties.ContainsKey("Team") && (string)p.CustomProperties["Team"] == "Red");
            int blueTeamCount = PhotonNetwork.PlayerList.Count(p => p.CustomProperties.ContainsKey("Team") && (string)p.CustomProperties["Team"] == "Blue");

            assignedTeam = (blueTeamCount < redTeamCount) ? "Blue" : "Red"; //  팀 균형 유지
        }

        int assignedProfileIndex = GetUniqueProfileIndex(); //  중복 없는 프로필 선택

        //  UI 먼저 변경 (네트워크 딜레이 없이 즉시 반영)
        UpdateLocalUI(PhotonNetwork.LocalPlayer, assignedTeam, assignedProfileIndex);

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
    {
        { "Team", assignedTeam },
        { "Ready", false }, // 입장할 때 무조건 Ready 상태 초기화
        { "ProfileImageIndex", assignedProfileIndex },
        { "LastRoom", PhotonNetwork.CurrentRoom.Name } //  현재 방 정보 저장
    };

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        Debug.Log($" {PhotonNetwork.LocalPlayer.NickName}님이 {PhotonNetwork.CurrentRoom.Name} 방에서 {assignedTeam} 팀에 배정됨! (프로필: {assignedProfileIndex})");

        //  UI 즉시 업데이트
        UpdateTeamUI();
    }

    void UpdateLocalUI(Player player, string team, int profileIndex)
    {
        //  플레이어 프로필과 팀 정보를 즉시 반영하여 시각적 딜레이 제거
        if (player.CustomProperties.ContainsKey("ProfileImageIndex"))
        {
            player.CustomProperties["ProfileImageIndex"] = profileIndex;
        }
        else
        {
            player.CustomProperties.Add("ProfileImageIndex", profileIndex);
        }

        if (player.CustomProperties.ContainsKey("Team"))
        {
            player.CustomProperties["Team"] = team;
        }
        else
        {
            player.CustomProperties.Add("Team", team);
        }

        UpdateTeamUI();
    }
    int GetUniqueProfileIndex()
    {
        PlayerPrefab.InitializeProfiles(); //  프로필 초기화 확인

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
            Debug.LogWarning(" 모든 프로필이 사용됨! 랜덤으로 할당.");
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

        }
        // AI 플레이어 UI 추가
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AIPlayers"))
        {
            string[] aiPlayers = (string[])PhotonNetwork.CurrentRoom.CustomProperties["AIPlayers"];
            foreach (string aiName in aiPlayers)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(aiName))
                {
                    ExitGames.Client.Photon.Hashtable aiProperties =
                        (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[aiName];

                    AddAIToTeamUI(aiName, aiProperties);
                }
            }
        }
    }


    // AI를 팀 UI에 추가하는 함수
    void AddAIToTeamUI(string aiName, ExitGames.Client.Photon.Hashtable properties)
    {
        string team = properties.ContainsKey("Team") ? (string)properties["Team"] : "Red";
        bool isReady = properties.ContainsKey("Ready") ? (bool)properties["Ready"] : false;
        int profileIndex = properties.ContainsKey("ProfileIndex") ? (int)properties["ProfileIndex"] : 0;

        GameObject aiUI = Instantiate(Resources.Load<GameObject>("PlayerPrefab"));
        PlayerPrefab playerPrefabScript = aiUI.GetComponent<PlayerPrefab>();

        if (playerPrefabScript != null)
        {
            playerPrefabScript.SetupAI(aiName, team, isReady, profileIndex);
        }
        else
        {
            Debug.LogError("PlayerPrefab 스크립트가 PlayerPrefab 오브젝트에 추가되지 않았습니다.");
        }

        // 팀에 따라 UI 배치
        if (team == "Red")
        {
            aiUI.transform.SetParent(redTeamPanel, false);
        }
        else
        {
            aiUI.transform.SetParent(blueTeamPanel, false);
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
            bool isPlayerReady = player.CustomProperties.ContainsKey("Ready") && (bool)player.CustomProperties["Ready"];
            if (!isPlayerReady)
            {
                allReady = false;
            }

            string team = player.CustomProperties.ContainsKey("Team") ? (string)player.CustomProperties["Team"] : "Red";
            if (team == "Red") redTeamCount++;
            else if (team == "Blue") blueTeamCount++;
        }

        //  AI 플레이어 확인 (팀 밸런스에 포함)
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AIPlayers"))
        {
            string[] aiPlayers = (string[])PhotonNetwork.CurrentRoom.CustomProperties["AIPlayers"];
            foreach (string aiName in aiPlayers)
            {
                ExitGames.Client.Photon.Hashtable aiProperties =
                    (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[aiName];

                bool isAIReady = aiProperties.ContainsKey("Ready") && (bool)aiProperties["Ready"];
                if (!isAIReady)
                {
                    allReady = false;
                }

                string aiTeam = aiProperties.ContainsKey("Team") ? (string)aiProperties["Team"] : "Red";
                if (aiTeam == "Red") redTeamCount++;
                else if (aiTeam == "Blue") blueTeamCount++;
            }
        }

        bool isBalanced = (redTeamCount == blueTeamCount);

        if (startGameButton == null)
        {
            Debug.LogError(" startGameButton이 null입니다! Inspector에서 연결하세요.");
            return;
        }

        bool canStartGame = allReady && isBalanced;
        startGameButton.interactable = canStartGame;
        startGameButton.gameObject.SetActive(true);

        Debug.Log($"StartGameButton 상태 - 활성화: {startGameButton.gameObject.activeSelf}, 상호작용 가능: {startGameButton.interactable}");
    }


    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) 
        
     
            return;
        Debug.Log($" 현재 방의 플레이어 수: {PhotonNetwork.PlayerList.Length}");
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log($"씬 이동 전 플레이어 - 닉네임: {player.NickName}, ID: {player.ActorNumber}, 방장 여부: {player.IsMasterClient}");
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

        Debug.Log("모든 플레이어의 정보를 저장했습니다.");
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

        // 방장이 나갈 경우에만 방을 삭제
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
            : "Red";

        string newTeam = (currentTeam == "Red") ? "Blue" : "Red";

        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        int maxTeamSize = maxPlayers / 2; // 각 팀의 최대 인원 (1:1 -> 1명, 2:2 -> 2명)

        // 🔹 현재 팀 인원 수 계산 (AI 포함)
        int redTeamCount = PhotonNetwork.PlayerList.Count(p => p.CustomProperties.ContainsKey("Team") && (string)p.CustomProperties["Team"] == "Red");
        int blueTeamCount = PhotonNetwork.PlayerList.Count(p => p.CustomProperties.ContainsKey("Team") && (string)p.CustomProperties["Team"] == "Blue");

        // AI도 팀 인원 수에 포함
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AIPlayers"))
        {
            string[] aiPlayers = (string[])PhotonNetwork.CurrentRoom.CustomProperties["AIPlayers"];
            foreach (string aiName in aiPlayers)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(aiName))
                {
                    ExitGames.Client.Photon.Hashtable aiProperties =
                        (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[aiName];

                    string aiTeam = aiProperties.ContainsKey("Team") ? (string)aiProperties["Team"] : "Red";
                    if (aiTeam == "Red") redTeamCount++;
                    else blueTeamCount++;
                }
            }
        }

        // 🔹 이동하려는 팀이 이미 꽉 차 있으면 막음 (AI 포함)
        if (newTeam == "Red" && redTeamCount >= maxTeamSize)
        {
            UIManager.ShowWarning("The Red team is already full!");
            //Debug.LogWarning(" 빨간팀이 이미 꽉 찼습니다! 팀 변경 불가.");
            return;
        }
        if (newTeam == "Blue" && blueTeamCount >= maxTeamSize)
        {
            UIManager.ShowWarning("The Blue team is already full!");
            //Debug.LogWarning(" 파란팀이 이미 꽉 찼습니다! 팀 변경 불가.");
            return;
        }

        // 팀 변경
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Team", newTeam } });

        Debug.Log($" {PhotonNetwork.LocalPlayer.NickName}님이 {currentTeam} → {newTeam} 팀으로 변경됨.");

        //  UI 즉시 반영
        UpdateTeamUI();
    }


    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("AIPlayers"))
        {
            Debug.Log("AI 플레이어 목록 변경 감지 - UI 업데이트");
            UpdateTeamUI();
        }
    }


    // AI 추가
    public void AddAIPlayer()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        int currentPlayers = PhotonNetwork.PlayerList.Length;
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

        // 현재 AI 및 플레이어 수 확인
        int redTeamCount = PhotonNetwork.PlayerList.Where(p => p.CustomProperties.ContainsKey("Team") && (string)p.CustomProperties["Team"] == "Red").Count();
        int blueTeamCount = PhotonNetwork.PlayerList.Where(p => p.CustomProperties.ContainsKey("Team") && (string)p.CustomProperties["Team"] == "Blue").Count();

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AIPlayers"))
        {
            string[] aiPlayers = (string[])PhotonNetwork.CurrentRoom.CustomProperties["AIPlayers"];
            foreach (string existingAI in aiPlayers) // 기존 AI 목록 확인
            {
                ExitGames.Client.Photon.Hashtable aiProperties = (ExitGames.Client.Photon.Hashtable)PhotonNetwork.CurrentRoom.CustomProperties[existingAI];
                string aiTeam = aiProperties.ContainsKey("Team") ? (string)aiProperties["Team"] : "Red";

                if (aiTeam == "Red") redTeamCount++;
                else blueTeamCount++;
            }
        }

        // 게임 모드 가져오기 (1:1 or 2:2)
        int gameMode = (int)PhotonNetwork.CurrentRoom.CustomProperties["GameMode"]; // 1이면 1:1, 2이면 2:2

        // AI 추가 제한 검사
        if (gameMode == 1 && (redTeamCount >= 1 && blueTeamCount >= 1))
        {
            UIManager.ShowWarning("In 1:1 mode, only 1 AI can be added to each team.");
            //Debug.LogWarning("In 1:1 mode, only 1 AI can be added to each team.");
            return;
        }
        if (gameMode == 2 && (redTeamCount >= 2 && blueTeamCount >= 2))
        {
            UIManager.ShowWarning("In 2:2 mode, only 2 AI can be added to each team.");
            //Debug.LogWarning("In 2:2 mode, only 2 AI can be added to each team.");
            return;
        }

        // 🔹 지역 변수 'aiName'을 상단에서 선언하여 범위 문제 해결
        string newAIName = aiNames[Random.Range(0, aiNames.Length)];

        // 균형 맞추기: 적은 팀에 배정
        string assignedTeam = (redTeamCount <= blueTeamCount) ? "Red" : "Blue";

        // 랜덤 프로필 인덱스 설정
        int profileIndex = Random.Range(0, aiProfileCount);

        // 기존 AI 목록 가져오기
        ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!roomProperties.ContainsKey("AIPlayers"))
        {
            roomProperties["AIPlayers"] = new string[] { };
        }

        List<string> aiList = ((string[])roomProperties["AIPlayers"]).ToList();
        aiList.Add(newAIName); // 수정된 AI 이름 사용

        // AI 속성 추가
        ExitGames.Client.Photon.Hashtable newAIProperties = new ExitGames.Client.Photon.Hashtable
    {
        { "Team", assignedTeam },
        { "Ready", true },  // AI 자동 Ready 설정
        { "IsAI", true },
        { "ProfileIndex", profileIndex }
    };

        // 방 속성 업데이트
        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
    {
        { "AIPlayers", aiList.ToArray() },
        { newAIName, newAIProperties } // 수정된 AI 이름 사용
    };

        PhotonNetwork.CurrentRoom.SetCustomProperties(newProperties);
        Debug.Log($"{newAIName} AI가 {assignedTeam} 팀에 추가됨. 프로필 인덱스: {profileIndex}, Ready: true");

        // UI 즉시 업데이트
        UpdateTeamUI();
    }




    public void RemoveSpecificAI(string aiName)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        ExitGames.Client.Photon.Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!roomProperties.ContainsKey("AIPlayers")) return;

        List<string> aiList = ((string[])roomProperties["AIPlayers"]).ToList();
        if (!aiList.Contains(aiName))
        {
            Debug.LogWarning($"AI {aiName}를 찾을 수 없습니다.");
            return;
        }

        aiList.Remove(aiName);
        roomProperties.Remove(aiName); // AI 속성 제거

        // 방 속성 업데이트
        ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable
    {
        { "AIPlayers", aiList.ToArray() }
    };

        PhotonNetwork.CurrentRoom.SetCustomProperties(newProperties);

        Debug.Log($"{aiName} AI가 삭제되었습니다.");
    }




}