using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using System.Linq;
using TMPro;

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
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "Ready", false } });
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
            Debug.LogError("🚨 Ready 버튼이 null입니다! Inspector에서 연결하세요.");
        }

        UIManager.instance.ShowTeamUI();
    }



    void AssignTeam()
    {
        // 현재 방에 들어온 순서에 따라 Red → Blue 번갈아 배정
        int playerIndex = PhotonNetwork.PlayerList.Length;  // 방에 입장한 순서

        string assignedTeam = (playerIndex % 2 == 1) ? "Red" : "Blue";  //

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "Team", assignedTeam }, { "Ready", false } });

        Debug.Log($" {PhotonNetwork.LocalPlayer.NickName}님이 {assignedTeam} 팀에 배정되었습니다!");
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($" {newPlayer.NickName}님이 방에 입장했습니다!");
        UpdateTeamUI();
    }


    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Team") || changedProps.ContainsKey("Ready"))
        {
            Debug.Log($" 플레이어 속성 변경 감지: {targetPlayer.NickName}");
            UpdateTeamUI();
            CheckAllReady();  //  여기서 호출됨 (속성 변경 시 즉시 체크)
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
            bool isMaster = player.IsMasterClient;  //  방장 여부 확인

            //  플레이어 UI 생성
            GameObject playerUI = Instantiate(Resources.Load<GameObject>("PlayerPrefab"));
            TextMeshProUGUI playerText = playerUI.GetComponentInChildren<TextMeshProUGUI>();

            //  닉네임 + Ready 상태 + 방장 여부 표시
            string displayName = player.NickName;
            if (isMaster) displayName += "(HEAD)";  // 방장 표시
            displayName += isReady ? " O" : "X";  // Ready 상태 표시

            playerText.text = displayName;

            if (team == "Red")
                playerUI.transform.SetParent(redTeamPanel, false);
            else
                playerUI.transform.SetParent(blueTeamPanel, false);
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
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "Ready", isReady } });

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
        if (PhotonNetwork.IsMasterClient)
        {
            bool allReady = true;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                bool isPlayerReady = player.CustomProperties.ContainsKey("Ready") && (bool)player.CustomProperties["Ready"];
                Debug.Log($" {player.NickName} Ready 상태: {isPlayerReady}");

                if (!isPlayerReady)
                {
                    allReady = false;
                    break;
                }
            }

            Debug.Log($" 모든 플레이어 준비 상태 최종 결과: {allReady}");

            if (startGameButton == null)
            {
                Debug.LogError(" startGameButton이 null입니다! Inspector에서 연결하세요.");
                return;
            }

            Debug.Log($" startGameButton 상태 - 활성화: {startGameButton.gameObject.activeSelf}, 상호작용 가능: {startGameButton.interactable}");

            startGameButton.interactable = allReady;
            startGameButton.gameObject.SetActive(true);
        }
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
        PhotonNetwork.LeaveRoom();
        UIManager.instance.ShowMainUI();  // 다시 방 목록 UI로 전환
    }

    public override void OnLeftRoom()
    {
        UIManager.instance.ShowMainUI();  // 방 나가면 UI 변경
    }
}