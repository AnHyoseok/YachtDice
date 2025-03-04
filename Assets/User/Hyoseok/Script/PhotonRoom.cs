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
    public Button startGameButton; // 방장만 활성화
    private bool isReady = false;

    void Start()
    {
        AssignTeam();
        UpdateTeamUI();
        readyButton.onClick.AddListener(ToggleReady);
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        startGameButton.onClick.AddListener(StartGame);
    }

    void AssignTeam()
    {
        int playerIndex = PhotonNetwork.PlayerList.Length - 1;
        string playerTeam = (playerIndex % 2 == 0) ? "Red" : "Blue";
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "Team", playerTeam }, { "Ready", false } });
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Team") || changedProps.ContainsKey("Ready"))
        {
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

            // Resources 폴더에서 PlayerPrefab 불러오기
            GameObject playerUI = Instantiate(Resources.Load<GameObject>("PlayerPrefab"));
            playerUI.GetComponent<PlayerPrefab>().Setup(player);

            if (team == "Red")
                playerUI.transform.SetParent(redTeamPanel, false);
            else
                playerUI.transform.SetParent(blueTeamPanel, false);
        }
    }


    public void ToggleReady()
    {
        if (readyButton == null)
        {
            Debug.LogError("readyButton이 null 상태입니다!");
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
            bool allReady = PhotonNetwork.PlayerList.All(p => p.CustomProperties.ContainsKey("Ready") && (bool)p.CustomProperties["Ready"]);
            startGameButton.interactable = allReady;
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
